
namespace Celeste64;

/// <summary>
/// Welcome to the monolithic player class! This time only 2300 lines ;)
/// </summary>
public class Player : Actor, IHaveModels, IHaveSprites, IRidePlatforms, ICastPointShadow
{
	#region Constants

	private const float Acceleration = 500;
	private const float PastMaxDeccel = 60;
	private const float AirAccelMultMin = .5f;
	private const float AirAccelMultMax = 1f;
	private const float MaxSpeed = 64;
	private const float RotateThreshold = MaxSpeed * .2f;
	private const float RotateSpeed = MathF.Tau * 1.5f;
	private const float RotateSpeedAboveMax = MathF.Tau * .6f;
	private const float Friction = 800;
	private const float AirFrictionMult = .1f;
	private const float Gravity = 600;
	private const float MaxFall = -120;
	private const float HalfGravThreshold = 100;
	private const float JumpHoldTime = .1f;
	private const float JumpSpeed = 90;
	private const float JumpXYBoost = 10;
	private const float CoyoteTime = .12f;
	private const float WallJumpXYSpeed = MaxSpeed * 1.3f;

	private const float DashSpeed = 140;
	private const float DashEndSpeedMult = .75f;
	private const float DashTime = .2f;
	private const float DashResetCooldown = .2f;
	private const float DashCooldown = .1f;
	private const float DashRotateSpeed = MathF.Tau * .3f;

	private const float DashJumpSpeed = 40;
	private const float DashJumpHoldSpeed = 20;
	private const float DashJumpHoldTime = .3f;
	private const float DashJumpXYBoost = 16;

	private const float SkidDotThreshold = -.7f;
	private const float SkiddingStartAccel = 300;
	private const float SkiddingAccel = 500;
	private const float EndSkidSpeed = MaxSpeed * .8f;
	private const float SkidJumpSpeed = 120;
	private const float SkidJumpHoldTime = .16f;
	private const float SkidJumpXYSpeed = MaxSpeed * 1.4f;

	private const float WallPushoutDist = 3;
	private const float ClimbCheckDist = 4;
	private const float ClimbSpeed = 40;
	private const float ClimbHopUpSpeed = 80;
	private const float ClimbHopForwardSpeed = 40;
	private const float ClimbHopNoMoveTime = .25f;

	private const float SpringJumpSpeed = 160;
	private const float SpringJumpHoldTime = .3f;

	private const float FeatherStartTime = .4f;
	private const float FeatherFlySpeed = 100;
	private const float FeatherStartSpeed = 140;
	private const float FeatherTurnSpeed = MathF.Tau * .75f;
	private const float FeatherAccel = 60;
	private const float FeatherDuration = 2.2f;
	private const float FeatherExitXYMult = .5f;
	private const float FeatherExitZSpeed = 60;

	static private readonly Color CNormal = 0xdb2c00;
	static private readonly Color CNoDash = 0x6ec0ff;
	static private readonly Color CTwoDashes = 0xfa91ff;
	static private readonly Color CRefillFlash = Color.White;
	static private readonly Color CFeather = 0xf2d450;

	#endregion

	#region SubClasses

	private class Trail
	{
		public readonly Hair Hair;
		public readonly SkinnedModel Model;
		public Matrix Transform;
		public float Percent;
		public Color Color;

		public Trail()
		{
			Model = new(Assets.Models["player"]);
			Model.Flags = ModelFlags.Transparent;
			Model.MakeMaterialsUnique();
			foreach (var mat in Model.Materials)
			{
				mat.Texture = Assets.Textures["white"];
				mat.Effects = 0;
			}

			Hair = new();
			foreach (var mat in Hair.Materials)
			{
				mat.Texture = Assets.Textures["white"];
				mat.Effects = 0;
			}
			Hair.Flags = ModelFlags.Transparent;
		}
	}

	#endregion

	// used between respawns
	private static Vec3 storedCameraForward;
	private static float storedCameraDistance;

	private enum States { Normal, Dashing, Skidding, Climbing, StrawbGet, FeatherStart, Feather, Respawn, Dead, StrawbReveal, Cutscene, Bubble, Cassette };
	private enum Events { Land };

	public bool Dead = false;

	public Vec3 ModelScale = Vec3.One;
	public SkinnedModel Model;
	public readonly Hair Hair = new();
	public float PointShadowAlpha { get; set; } = 1.0f;

	public Vec3 Velocity => velocity;
	public Vec3 PreviousVelocity => previousVelocity;

	private Vec3 velocity;
	private Vec3 previousVelocity;
	private Vec3 groundNormal;
	private Vec3 platformVelocity;
	private float tPlatformVelocityStorage;
	private float tGroundSnapCooldown;
	private Actor? climbingWallActor;
	private Vec3 climbingWallNormal;

	private bool onGround;
	private Vec2 targetFacing = Vec2.UnitY;
	private Vec3 cameraTargetForward = new(0, 1, 0);
	private float cameraTargetDistance = 0.50f;
	private readonly StateMachine<States, Events> stateMachine;

	private record struct CameraOverride(Vec3 Position, Vec3 LookAt);
	private CameraOverride? cameraOverride = null;
	private Vec3 cameraOriginPos;

	private float tCoyote;
	private float coyoteZ;

	private bool drawModel = true;
	private bool drawHair = true;
	private bool drawOrbs = false;
	private float drawOrbsEase = 0;

	private readonly List<Trail> trails = [];
	private readonly Func<SpikeBlock, bool> spikeBlockCheck;
	private Color lastDashHairColor;

	private Sound? sfxWallSlide;
	private Sound? sfxFeather;
	private Sound? sfxBubble;

	private Vec3 SolidWaistTestPos 
		=> Position + Vec3.UnitZ * 3;
	private Vec3 SolidHeadTestPos 
		=> Position + Vec3.UnitZ * 10;

	private bool InFeatherState 
		=> stateMachine.State == States.FeatherStart
		|| stateMachine.State == States.Feather;

	private bool InBubble
		=> stateMachine.State == States.Bubble;

	public bool IsStrawberryCounterVisible
		=> stateMachine.State == States.StrawbGet;

	public bool IsAbleToPickup
		=> stateMachine.State != States.StrawbGet 
		&& stateMachine.State != States.Bubble 
		&& stateMachine.State != States.Cassette 
		&& stateMachine.State != States.StrawbReveal 
		&& stateMachine.State != States.Respawn
		&& stateMachine.State != States.Dead;

	public bool IsAbleToPause 
		=> stateMachine.State != States.StrawbReveal
		&& stateMachine.State != States.StrawbGet
		&& stateMachine.State != States.Cassette
		&& stateMachine.State != States.Dead;

	public Player()
	{
		PointShadowAlpha = 1.0f;
		LocalBounds = new BoundingBox(new Vec3(0, 0, 10), 10);
		UpdateOffScreen = true;

		// setup model
		{
			Model = new(Assets.Models["player"]);
			Model.SetBlendDuration("Idle", "Dash", 0.05f);
			Model.SetBlendDuration("Idle", "Run", 0.2f);
			Model.SetBlendDuration("Run", "Skid", .125f);
			Model.SetLooping("Dash", false);
			Model.Flags |= ModelFlags.Silhouette;
			Model.Play("Idle");

			foreach (var mat in Model.Materials)
				mat.Effects = 0.60f;
		}

		stateMachine = new();
		stateMachine.InitState(States.Normal, StNormalUpdate, StNormalEnter, StNormalExit);
		stateMachine.InitState(States.Dashing, StDashingUpdate, StDashingEnter, StDashingExit);
		stateMachine.InitState(States.Skidding, StSkiddingUpdate, StSkiddingEnter, StSkiddingExit);
		stateMachine.InitState(States.Climbing, StClimbingUpdate, StClimbingEnter, StClimbingExit);
		stateMachine.InitState(States.StrawbGet, StStrawbGetUpdate, StStrawbGetEnter, StStrawbGetExit, StStrawbGetRoutine);
		stateMachine.InitState(States.FeatherStart, StFeatherStartUpdate, StFeatherStartEnter, StFeatherStartExit);
		stateMachine.InitState(States.Feather, StFeatherUpdate, StFeatherEnter, StFeatherExit);
		stateMachine.InitState(States.Respawn, StRespawnUpdate, StRespawnEnter, StRespawnExit);
		stateMachine.InitState(States.StrawbReveal, null, StStrawbRevealEnter, StStrawbRevealExit, StStrawbRevealRoutine);
		stateMachine.InitState(States.Cutscene, StCutsceneUpdate, StCutsceneEnter);
		stateMachine.InitState(States.Dead, StDeadUpdate, StDeadEnter);
		stateMachine.InitState(States.Bubble, null, null, StBubbleExit, StBubbleRoutine);
		stateMachine.InitState(States.Cassette, null, null, StCassetteExit, StCassetteRoutine);

		spikeBlockCheck = (spike) =>
		{
			return Vec3.Dot(velocity.Normalized(), spike.Direction) < 0.5f;
		};

		SetHairColor(0xdb2c00);
	}

	#region Added / Update

	public override void Added()
	{
		if (World.Entry.Reason == World.EntryReasons.Respawned)
		{
			cameraTargetForward = storedCameraForward;
			cameraTargetDistance = storedCameraDistance;
			stateMachine.State = States.Respawn;
		}
		else if (World.Entry.Submap && World.Entry.Reason == World.EntryReasons.Entered)
		{
			stateMachine.State = States.StrawbReveal;
		}
		else
		{
			stateMachine.State = States.Normal;
		}

		sfxWallSlide = World.Add(new Sound(this, Sfx.sfx_wall_slide));
		sfxFeather = World.Add(new Sound(this, Sfx.sfx_feather_state_active_loop));
		sfxBubble = World.Add(new Sound(this, Sfx.sfx_bubble_loop));

		cameraOriginPos = Position;
		GetCameraTarget(out var orig, out var target, out _);
		World.Camera.LookAt = target;
		World.Camera.Position = orig;
	}

	public override void Update()
	{
		// only update camera if not dead
		if (stateMachine.State != States.Respawn && stateMachine.State != States.Dead && 
			stateMachine.State != States.StrawbReveal && stateMachine.State != States.Cassette)
		{
			// Rotate Camera
			{
				var rot = new Vec2(cameraTargetForward.X, cameraTargetForward.Y).Angle();
				rot -= Controls.Camera.Value.X * Time.Delta * 4;

				var angle = Calc.AngleToVector(rot);
				cameraTargetForward = new(angle, 0);
			}

			// Move Camera in / out
			if (Controls.Camera.Value.Y != 0)
			{
				cameraTargetDistance += Controls.Camera.Value.Y * Time.Delta;
				cameraTargetDistance = Calc.Clamp(cameraTargetDistance, 0, 1);
			}
			else
			{
				const float interval = 1f / 3;
				const float threshold = .1f;
				if (cameraTargetDistance % interval < threshold || cameraTargetDistance % interval > interval - threshold)
					Calc.Approach(ref cameraTargetDistance, Calc.Snap(cameraTargetDistance, interval), Time.Delta / 2);
			}
		}

		// don't do anything if dead
		if (stateMachine.State == States.Respawn || stateMachine.State == States.Dead || stateMachine.State == States.Cutscene)
		{
			stateMachine.Update();
			return;
		}

		// death plane
		if (!InBubble)
		{
			if (Position.Z < World.DeathPlane ||
				World.Overlaps<DeathBlock>(SolidWaistTestPos) ||
				World.Overlaps<SpikeBlock>(SolidWaistTestPos, spikeBlockCheck))
			{
				Kill();
				return;
			}
		}

		// enter cutscene
		if (World.All<Cutscene>().Count > 0)
			stateMachine.State = States.Cutscene;

		// run timers
		{
			if (tCoyote > 0)
				tCoyote -= Time.Delta;
			if (tHoldJump > 0)
				tHoldJump -= Time.Delta;
			if (tDashCooldown > 0)
				tDashCooldown -= Time.Delta;
			if (tDashResetCooldown > 0)
				tDashResetCooldown -= Time.Delta;
			if (tDashResetFlash > 0)
				tDashResetFlash -= Time.Delta;
			if (tNoMove > 0)
				tNoMove -= Time.Delta;
			if (tPlatformVelocityStorage > 0)
				tPlatformVelocityStorage -= Time.Delta;
			if (tGroundSnapCooldown > 0)
				tGroundSnapCooldown -= Time.Delta;
			if (tClimbCooldown > 0)
				tClimbCooldown -= Time.Delta;
		}

		previousVelocity = velocity;
		stateMachine.Update();

		// move and pop out
		if (!InBubble)
		{
			// push out of NPCs
			foreach (var actor in World.All<IHavePushout>())
			{
				var it = (actor as IHavePushout)!;
				if (it.PushoutRadius <= 0 || it.PushoutHeight <= 0)
					continue;

				if (Position.Z < actor.Position.Z - 12 || Position.Z > actor.Position.Z + it.PushoutHeight)
					continue;

				var diff = (Position.XY() - actor.Position.XY());
				if (diff.LengthSquared() > it.PushoutRadius * it.PushoutRadius)
					continue;

				var normal = diff.Normalized();
				var distance = diff.Length();
				var pushout = (it.PushoutRadius - distance);

				SweepTestMove(new Vec3(normal * pushout, 0), false);
			}

			// handle actual movement
			{
				var amount = velocity * Time.Delta;
				SweepTestMove(amount, tNoMove <= 0);
			}

			// do an idle popout for good measure
			Popout(false);
		}

		// pickups
		if (IsAbleToPickup)
		{
			foreach (var actor in World.All<IPickup>())
			{
				if (actor is IPickup pickup)
				{
					if ((SolidWaistTestPos - actor.Position).LengthSquared() < pickup.PickupRadius * pickup.PickupRadius)
						pickup.Pickup(this);
				}
			}
		}
	}

	public override void LateUpdate()
	{
		// ground checks
		{
			bool prevOnGround = onGround;
			onGround = GroundCheck(out var pushout, out var normal, out _);
			if (onGround)
				Position += pushout;

			if (tGroundSnapCooldown <= 0 && prevOnGround && !onGround)
			{
				// try to ground snap?
				if (World.SolidRayCast(Position, -Vec3.UnitZ, 5, out var hit) && FloorNormalCheck(hit.Normal))
				{
					Position = hit.Point;
					onGround = GroundCheck(out _, out normal, out _);
				}
			}

			if (onGround)
			{
				autoJump = false;
				groundNormal = normal;
				tCoyote = CoyoteTime;
				coyoteZ = Position.Z;
				if (tDashResetCooldown <= 0)
					RefillDash();
			}
			else
				groundNormal = Vec3.UnitZ;

			if (!prevOnGround && onGround)
			{
				float t = Calc.ClampedMap(previousVelocity.Z, 0, MaxFall);
				ModelScale = Vec3.Lerp(Vec3.One, new(1.4f, 1.4f, .6f), t);
				stateMachine.CallEvent(Events.Land);

				if (!Game.Instance.IsMidTransition && !InBubble)
				{
					Audio.Play(Sfx.sfx_land, Position);

					for (int i = 0; i < 16; i++)
					{
						var angle = Calc.AngleToVector((i / 16.0f) * MathF.Tau);
						var at = Position + new Vec3(angle, 0) * 4;
						var vel = (tPlatformVelocityStorage > 0 ? platformVelocity : Vec3.Zero) + new Vec3(angle, 0) * 50;
						World.Request<Dust>().Init(at, vel);
					}
				}
			}
		}

		// update camera origin position
		{
			float ZPad = stateMachine.State == States.Climbing ? 0 : 8;
			cameraOriginPos.X = Position.X;
			cameraOriginPos.Y = Position.Y;

			float targetZ;
			if (onGround)
				targetZ = Position.Z;
			else if (Position.Z < cameraOriginPos.Z)
				targetZ = Position.Z;
			else if (Position.Z > cameraOriginPos.Z + ZPad)
				targetZ = Position.Z - ZPad;
			else
				targetZ = cameraOriginPos.Z;

			if (cameraOriginPos.Z != targetZ)
				cameraOriginPos.Z += (targetZ - cameraOriginPos.Z) * (1 - MathF.Pow(.001f, Time.Delta));
		}

		// update camera position
		{
			Vec3 lookAt, cameraPos;

			if (cameraOverride.HasValue)
			{
				lookAt = cameraOverride.Value.LookAt;
				cameraPos = cameraOverride.Value.Position;
			}
			else
			{
				GetCameraTarget(out lookAt, out cameraPos, out _);
			}

			World.Camera.Position += (cameraPos - World.Camera.Position) * (1 - MathF.Pow(0.01f, Time.Delta));
			World.Camera.LookAt = lookAt;

			float targetFOV = Calc.ClampedMap(velocity.XY().Length(), MaxSpeed * 1.2f, 120, 1, 1.2f);

			World.Camera.FOVMultiplier = Calc.Approach(World.Camera.FOVMultiplier, targetFOV, Time.Delta / 4);
		}

		// update model
		{
			Calc.Approach(ref ModelScale.X, 1, Time.Delta / .8f);
			Calc.Approach(ref ModelScale.Y, 1, Time.Delta / .8f);
			Calc.Approach(ref ModelScale.Z, 1, Time.Delta / .8f);

			Facing = Calc.AngleToVector(Calc.AngleApproach(Facing.Angle(), targetFacing.Angle(), MathF.Tau * 2 * Time.Delta));

			Model.Update();
			Model.Transform = Matrix.CreateScale(ModelScale * 3);

			if (stateMachine.State != States.Feather && stateMachine.State != States.FeatherStart)
			{
				Color color;
				if (tDashResetFlash > 0)
					color = CRefillFlash;
				else if (dashes == 1)
					color = CNormal;
				else if (dashes == 0)
					color = CNoDash;
				else
					color = CTwoDashes;

				SetHairColor(color);
			}
		}

		// hair
		{
			var hairMatrix = Matrix.Identity;

			foreach (var it in Model.Instance.Armature.LogicalNodes)
			{
				if (it.Name == "Head")
				{
					hairMatrix = it.ModelMatrix * SkinnedModel.BaseTranslation * Model.Transform * Matrix;
				}
			}

			Hair.Flags = Model.Flags;
			Hair.Forward = -new Vec3(Facing, 0);
			Hair.Squish = ModelScale;
			Hair.Materials[0].Effects = 0;
			Hair.Grounded = onGround;
			Hair.Update(hairMatrix);
		}

		// trails
		for (int i = trails.Count - 1; i >= 0; i--)
		{
			if (trails[i].Percent < 1)
				trails[i].Percent += Time.Delta / 0.5f;
		}
	}

	#endregion

	#region Camera Calculation
	
	public void GetCameraTarget(out Vec3 cameraLookAt, out Vec3 cameraPosition, out bool snapRequested)
	{
		snapRequested = false;

		// get default values
		cameraLookAt = cameraOriginPos;
		cameraPosition = cameraLookAt
			- cameraTargetForward * Utils.Lerp3(30, 60, 110, 110, cameraTargetDistance)
			+ Vec3.UnitZ * Utils.Lerp3(1, 30, 80, 180, cameraTargetDistance);
		cameraLookAt += Vec3.UnitZ * 12;

		// inside a fixed camera zone
		if (World.OverlapsFirst<FixedCamera>(SolidWaistTestPos) is {} fixedCamera 
		&& (cameraLookAt - fixedCamera.Position).Length() > 5)
		{
			cameraPosition = fixedCamera.Point;
			cameraTargetForward = new Vec3((cameraLookAt.XY() - cameraPosition.XY()).Normalized(), 0);
			snapRequested = true;
		}
		// try to push out of solids if we're in them
		else
		{
			var from = cameraLookAt;// - Vec3.UnitZ * (onGround ? 0 : 6);
			var to = cameraPosition;
			var normal = (to - from).Normalized();

			// reduce distance by a bit to account for near plane cutoff
			var distance = (to - from).Length();
			if (distance > World.Camera.NearPlane + 1)
				distance -= World.Camera.NearPlane;

			// inside a wall, push out
			if (World.SolidRayCast(from, normal, distance, out var hit, false, true))
			{
				if ((hit.Intersections % 2) == 1)
				{
					snapRequested = true;
					cameraPosition = hit.Point;
				}
			}

			// push down from ceilings a bit
			if (World.SolidRayCast(cameraPosition, Vec3.UnitZ, 5, out hit, true, true))
			{
				cameraPosition = hit.Point - Vec3.UnitZ * 5;
			}
		}
	}

	#endregion

	#region Various Methods

	private Vec2 RelativeMoveInput
	{
		get
		{
			if (Controls.Move.Value == Vec2.Zero)
				return Vec2.Zero;

			Vec2 forward, side;
			var cameraForward = World.Camera.Forward.XY();
			if (cameraForward.X == 0 && cameraForward.Y == 0)
				forward = targetFacing;
			else
				forward = cameraForward.Normalized();
			side = Vec2.Transform(forward, Matrix3x2.CreateRotation(MathF.PI / 2));

			Vec2 input = -Controls.Move.Value.Normalized();
			if (Vec2.Dot(input, Vec2.UnitY) >= .985f)
				input = Vec2.UnitY;

			return forward * input.Y + side * input.X;
		}
	}

	public void SetTargetFacing(Vec2 facing)
	{
		targetFacing = facing;
	}

	public void SetHairColor(Color color)
	{
		foreach (var mat in Model.Materials)
		{
			if (mat.Name == "Hair")
			{
				mat.Color = color;
				mat.Effects = 0;
			}
            mat.SilhouetteColor = color;
		}

		Hair.Color = color;
		Hair.Nodes = (InFeatherState ? 18 : (dashes >= 2 ? 16 : 10));
	}

	public void SweepTestMove(Vec3 delta, bool resolveImpact)
	{
		if (delta.LengthSquared() <= 0)
			return;

		var remaining = delta.Length();
		var stepSize = 2.0f;
		var stepNormal = delta / remaining;

		while (remaining > 0)
		{
			// perform step
			var step = MathF.Min(remaining, stepSize);
			remaining -= step;
			Position += stepNormal * step;

			if (Popout(resolveImpact))
			{
				// don't repeatedly resolve wall impacts
				resolveImpact = false;
			}
		}
	}

	/// <summary>
	/// Pops out of Solid Geometry. Returns true if popped out of a wall
	/// </summary>
	public bool Popout(bool resolveImpact)
	{
		// ground test
		if (GroundCheck(out var pushout, out _, out _))
		{
			Position += pushout;
			if (resolveImpact)
				velocity.Z = MathF.Max(velocity.Z, 0);
		}

		// ceiling test
		else if (CeilingCheck(out pushout))
		{
			Position += pushout;
			if (resolveImpact)
				velocity.Z = MathF.Min(velocity.Z, 0);
		}

		// wall test
		if (World.SolidWallCheckNearest(SolidWaistTestPos, WallPushoutDist, out var hit) ||
			World.SolidWallCheckNearest(SolidHeadTestPos, WallPushoutDist, out hit))
		{
			// feather state handling
			if (resolveImpact && stateMachine.State == States.Feather && tFeatherWallBumpCooldown <= 0 && !(Controls.Climb.Down && TryClimb()))
			{
				Position += hit.Pushout;
				velocity = velocity.WithXY(Vec2.Reflect(velocity.XY(), hit.Normal.XY().Normalized()));
				tFeatherWallBumpCooldown = 0.50f;
				Audio.Play(Sfx.sfx_feather_state_bump_wall, Position);
			}
			// is it a breakable thing?
			else if (resolveImpact && hit.Actor is BreakBlock breakable && !breakable.Destroying && velocity.XY().Length() > 90)
			{
				BreakBlock(breakable, velocity.Normalized());
			}
			// normal wall
			else
			{
				Position += hit.Pushout;
				
				if (resolveImpact)
				{
					var dot = MathF.Min(0.0f, Vec3.Dot(velocity.Normalized(), hit.Normal));
					velocity -= hit.Normal * velocity.Length() * dot;
				}
			}

			return true;
		}

		return false;
	}

	public void CancelGroundSnap() =>
		tGroundSnapCooldown = 0.1f;

	private void Jump()
	{
		Position = Position with { Z = coyoteZ };
		holdJumpSpeed = velocity.Z = JumpSpeed;
		tHoldJump = JumpHoldTime;
		tCoyote = 0;
		autoJump = false;

		var input = RelativeMoveInput;
		if (input != Vec2.Zero)
		{
			input = input.Normalized();
			targetFacing = input;
			velocity += new Vec3(input * JumpXYBoost, 0);
		}

		AddPlatformVelocity(true);
		CancelGroundSnap();

		ModelScale = new(.6f, .6f, 1.4f);
		Audio.Play(Sfx.sfx_jump, Position);
	}

	private void WallJump()
	{
		holdJumpSpeed = velocity.Z = JumpSpeed;
		tHoldJump = JumpHoldTime;
		autoJump = false;

		var velXY = targetFacing * WallJumpXYSpeed;
		velocity = velocity.WithXY(velXY);

		AddPlatformVelocity(false);
		CancelGroundSnap();

		ModelScale = new(.6f, .6f, 1.4f);
		Audio.Play(Sfx.sfx_jump_wall, Position);
	}

	private void SkidJump()
	{
		Position = Position with { Z = coyoteZ };
		holdJumpSpeed = velocity.Z = SkidJumpSpeed;
		tHoldJump = SkidJumpHoldTime;
		tCoyote = 0;

		var velXY = targetFacing * SkidJumpXYSpeed;
		velocity = velocity.WithXY(velXY);

		AddPlatformVelocity(false);
		CancelGroundSnap();
		
		for (int i = 0; i < 16; i ++)
		{
			var dir = new Vec3(Calc.AngleToVector((i / 16f) * MathF.Tau), 0);
			World.Request<Dust>().Init(Position + dir * 8, new Vec3(velocity.XY() * 0.5f, 10) - dir * 50, 0x666666);
		}

		ModelScale = new(.6f, .6f, 1.4f);
		Audio.Play(Sfx.sfx_jump, Position);
		Audio.Play(Sfx.sfx_jump_skid, Position);
	}

	private void DashJump()
	{
		Position = Position with { Z = coyoteZ };
		velocity.Z = DashJumpSpeed;
		holdJumpSpeed = DashJumpHoldSpeed;
		tHoldJump = DashJumpHoldTime;
		tCoyote = 0;
		autoJump = false;
		dashes = 1;

		if (DashJumpXYBoost != 0)
		{
			var input = RelativeMoveInput;
			if (input != Vec2.Zero)
			{
				input = input.Normalized();
				targetFacing = input;
				velocity += new Vec3(input * DashJumpXYBoost, 0);
			}
		}

		AddPlatformVelocity(false);
		CancelGroundSnap();

		ModelScale = new(.6f, .6f, 1.4f);
		Audio.Play(Sfx.sfx_jump, Position);
		Audio.Play(Sfx.sfx_jump_superslide, Position);
	}

	private void AddPlatformVelocity(bool playSound)
	{
		if (tPlatformVelocityStorage > 0)
		{
			Vec3 add = platformVelocity;

			add.Z = Calc.Clamp(add.Z, 0, 180);
			if (add.XY().LengthSquared() > 300 * 300)
				add = add.WithXY(add.XY().Normalized() * 300);

			velocity += add;
			platformVelocity = Vec3.Zero;
			tPlatformVelocityStorage = 0;

			if (playSound && (add.Z >= 10 || add.XY().Length() > 10))
				Audio.Play(Sfx.sfx_jump_assisted, Position);
		}
	}

	public void Kill()
	{
		stateMachine.State = States.Dead;
		storedCameraForward = cameraTargetForward;
		storedCameraDistance = cameraTargetDistance;
		Save.CurrentRecord.Deaths++;
		Dead = true;
	}

	private bool ClimbCheckAt(Vec3 offset, out WallHit hit)
	{
		if (World.SolidWallCheckClosestToNormal(SolidWaistTestPos + offset, ClimbCheckDist, -new Vec3(targetFacing, 0), out hit)
		&& (RelativeMoveInput == Vec2.Zero || Vec2.Dot(hit.Normal.XY().Normalized(), RelativeMoveInput) <= -0.5f)
		&& ClimbNormalCheck(hit.Normal))
			return true;
		return false;
	}

	private bool TryClimb()
	{
		var result = ClimbCheckAt(Vec3.Zero, out var wall);

		// let us snap up to walls if we're jumping for them
		// note: if vel.z is allowed to be downwards then we awkwardly re-grab when sliding off
		// the bottoms of walls, which is really bad feeling
		if (!result && Velocity.Z > 0 && !onGround && stateMachine.State != States.Climbing)
			result = ClimbCheckAt(Vec3.UnitZ * 4, out wall);

		if (result)
		{
			climbingWallNormal = wall.Normal;
			climbingWallActor = wall.Actor;
			var moveTo = wall.Point + (Position - SolidWaistTestPos) + wall.Normal * WallPushoutDist;
			SweepTestMove(moveTo - Position, false);
			targetFacing = -climbingWallNormal.XY().Normalized();
			return true;
		}
		else
		{
			climbingWallActor = default;
			climbingWallNormal = default;
			return false;
		}
	}

	private bool ClimbNormalCheck(in Vec3 normal)
	{
		return MathF.Abs(normal.Z) < 0.35f; 
	}

	private bool FloorNormalCheck(in Vec3 normal)
		=> !ClimbNormalCheck(normal) && normal.Z > 0;

	private bool WallJumpCheck()
	{
		if (Controls.Jump.Pressed 
		&& World.SolidWallCheckClosestToNormal(SolidWaistTestPos, ClimbCheckDist, -new Vec3(targetFacing, 0), out var hit))
		{
			Controls.Jump.ConsumePress();
			Position += (hit.Pushout * (WallPushoutDist / ClimbCheckDist));
			targetFacing = hit.Normal.XY().Normalized();
			return true;
		}
		else
			return false;
	}

	private void BreakBlock(BreakBlock block, Vec3 direction)
	{
		World.HitStun = 0.1f;
		block.Break(direction);

		if (block.BouncesPlayer)
		{
			velocity.X = -velocity.X * 0.80f;
			velocity.Y = -velocity.Y * 0.80f;
			velocity.Z = 100;

			stateMachine.State = States.Normal;
			CancelGroundSnap();
		}
	}

	internal void Spring(Spring spring)
	{
		stateMachine.State = States.Normal;

		Position = Position with { Z = Calc.Approach(Position.Z, spring.Position.Z + 3, 2) };

		var posXY = Position.XY();
		Calc.Approach(ref posXY, spring.Position.XY(), 4);
		Position = Position.WithXY(posXY);

		holdJumpSpeed = velocity.Z = SpringJumpSpeed;
		tHoldJump = SpringJumpHoldTime;
		tCoyote = 0;
		autoJump = true;

		var velXY = velocity.XY();
		Calc.Approach(ref velXY, Vec2.Zero, 30);
		velocity = velocity.WithXY(velXY);

		dashes = Math.Max(dashes, 1);
		CancelGroundSnap();
	}

	#endregion

	#region Normal State

	private const float FootstepInterval = .3f;

	private float tHoldJump;
	private float holdJumpSpeed;
	private bool autoJump;
	private float tNoMove;
	private float tFootstep;

	private void StNormalEnter()
	{
		tHoldJump = 0;
		tFootstep = FootstepInterval;
	}

	private void StNormalExit()
	{
		tHoldJump = 0;
		tNoMove = 0;
		autoJump = false;
		Model.Rate = 1;
	}

	private void StNormalUpdate()
	{
		// Check for NPC interaction
		if (onGround)
		{
			foreach (var actor in World.All<NPC>())
				if (actor is NPC npc && npc.InteractEnabled)
				{
					if ((Position - npc.Position).LengthSquared() < npc.InteractRadius * npc.InteractRadius &&
						Vec2.Dot((npc.Position - Position).XY(), targetFacing) > 0 &&
						MathF.Abs(npc.Position.Z - Position.Z) < 2)
					{
						npc.IsPlayerOver = true;

						if (Controls.Dash.ConsumePress())
						{
							npc.Interact(this);
							return;
						}

						break;
					}
				}
		}

		// movement
		{
			var velXY = velocity.XY();

			if (Controls.Move.Value == Vec2.Zero || tNoMove > 0)
			{
				// if not moving, simply apply friction

				float fric = Friction;
				if (!onGround)
					fric *= AirFrictionMult;

				// friction
				Calc.Approach(ref velXY, Vec2.Zero, fric * Time.Delta);
			}
			else if (onGround)
			{
				float max = MaxSpeed;

				// change max speed based on ground slope angle
				if (groundNormal != Vec3.UnitZ)
				{
					float slopeDot = 1 - Calc.Clamp(Vec3.Dot(groundNormal, Vec3.UnitZ), 0, 1);
					slopeDot *= Vec2.Dot(groundNormal.XY().Normalized(), targetFacing) * 2;
					max += max * slopeDot;
				}

				// trueMax is the max XY speed before applying analog stick magnitude
				float trueMax = max;

				// apply analog stick magnitude
				{
					float mag = Calc.ClampedMap(Controls.Move.Value.Length(), .4f, .92f, .3f, 1);
					max *= mag;
				}

				var input = RelativeMoveInput;

				// TODO: Solve this way better! Ugh I hate this!!
				// move lightly away from ledges by checking for no floor, and then sweeping in until we find floor
				// Please don't look at this code
				// if I had more time to solve this nicely I would do something else
				{
					var d = 4;

					if (input != Vec2.Zero &&
						!World.SolidRayCast(Position + new Vec3(input, 1) * d, -Vec3.UnitZ, 8, out var hit) &&
						!World.SolidRayCast(Position + new Vec3(0, 0, d), new Vec3(input, 0), d, out hit))
					{
						var left = Calc.AngleToVector(Calc.Angle(input) + 0.3f);
						var right = Calc.AngleToVector(Calc.Angle(input) - 0.3f);
						var count = 0;

						if (World.SolidRayCast(Position + new Vec3(left, 1) * d, -Vec3.UnitZ, 8, out hit))
						{
							while (World.SolidRayCast(Position + new Vec3(left, 1) * d, -Vec3.UnitZ, 8, out hit) && count++ < 10)
								left = Calc.AngleToVector(Calc.Angle(left) - 0.1f);
							input = Calc.AngleToVector(Calc.Angle(left) + 0.1f); ;
						}
						else if (World.SolidRayCast(Position + new Vec3(right, 1) * d, -Vec3.UnitZ, 8, out hit))
						{
							while (World.SolidRayCast(Position + new Vec3(right, 1) * d, -Vec3.UnitZ, 8, out hit) && count++ < 10)
								right = Calc.AngleToVector(Calc.Angle(right) + 0.1f);
							input = Calc.AngleToVector(Calc.Angle(right) - 0.1f); ;
						}
					}
				}

				// if travelling faster than our "true max" (ie. our max not accounting for analog stick magnitude),
				// then we switch into a slower decceleration to help the player preserve high speeds
				float accel;
				if (velXY.LengthSquared() >= trueMax * trueMax && Vec2.Dot(input, velXY) >= .7f)
					accel = PastMaxDeccel;
				else
					accel = Acceleration;

				// if our XY velocity is above the Rotate Threshold, then our XY velocity begins rotating
				// instead of using a simple approach to accelerate
				if (velXY.LengthSquared() >= RotateThreshold * RotateThreshold)
				{
					if (Vec2.Dot(input, velXY.Normalized()) <= SkidDotThreshold)
					{
						Facing = targetFacing = input;
						stateMachine.State = States.Skidding;
						return;
					}
					else
					{
						// Rotate speed is less when travelling above our "true max" speed
						// this gives high speeds less fine control
						float rotate;
						if (velXY.LengthSquared() > trueMax * trueMax)
							rotate = RotateSpeedAboveMax;
						else
							rotate = RotateSpeed;

						targetFacing = Calc.RotateToward(targetFacing, input, rotate * Time.Delta, 0);
						velXY = targetFacing * Calc.Approach(velXY.Length(), max, accel * Time.Delta);
					}
				}
				else
				{
					// if we're below the RotateThreshold, acceleration is very simple
					Calc.Approach(ref velXY, input * max, accel * Time.Delta);

					targetFacing = input.Normalized();
				}
			}
			else
			{
				float accel;
				if (velXY.LengthSquared() >= MaxSpeed * MaxSpeed && Vec2.Dot(RelativeMoveInput.Normalized(), velXY.Normalized()) >= .7f)
				{
					accel = PastMaxDeccel;

					var dot = Vec2.Dot(RelativeMoveInput.Normalized(), targetFacing);
					accel *= Calc.ClampedMap(dot, -1, 1, AirAccelMultMax, AirAccelMultMin);
				}
				else
				{
					accel = Acceleration;

					var dot = Vec2.Dot(RelativeMoveInput.Normalized(), targetFacing);
					accel *= Calc.ClampedMap(dot, -1, 1, AirAccelMultMin, AirAccelMultMax);
				}

				Calc.Approach(ref velXY, RelativeMoveInput * MaxSpeed, accel * Time.Delta);
			}

			velocity = velocity.WithXY(velXY);
		}

		// Footstep sounds
		if (onGround && velocity.XY().Length() > 10)
		{
			tFootstep -= Time.Delta * Model.Rate;
			if (tFootstep <= 0)
			{
				tFootstep = FootstepInterval;
				Audio.Play(Sfx.sfx_footstep_general, Position);
			}

			if (Time.OnInterval(0.05f))
			{
				var at = Position + new Vec3(World.Rng.Float(-3, 3), World.Rng.Float(-3, 3), 0);
				var vel = tPlatformVelocityStorage > 0 ? platformVelocity : Vec3.Zero;
				World.Request<Dust>().Init(at, vel);
			}
		}
		else
			tFootstep = FootstepInterval;

		// start climbing
		if (Controls.Climb.Down && tClimbCooldown <= 0 && TryClimb())
		{
			stateMachine.State = States.Climbing;
			return;
		}

		// dashing
		if (TryDash())
			return;

		// jump & gravity
		if (tCoyote > 0 && Controls.Jump.ConsumePress())
			Jump();
		else if (WallJumpCheck())
			WallJump();
		else
		{
			if (tHoldJump > 0 && (autoJump || Controls.Jump.Down))
			{
				if (velocity.Z < holdJumpSpeed)
					velocity.Z = holdJumpSpeed;
			}
			else
			{
				float mult;
				if ((Controls.Jump.Down || autoJump) && MathF.Abs(velocity.Z) < HalfGravThreshold)
					mult = .5f;
				else
				{
					mult = 1;
					autoJump = false;
				}

				Calc.Approach(ref velocity.Z, MaxFall, Gravity * mult * Time.Delta);
				tHoldJump = 0;

			}
		}

		// Update Model Animations
		if (onGround)
		{
			var velXY = velocity.XY();
			if (velXY.LengthSquared() > 1)
			{
				// TODO: this was jittery, turning off for now
				Model.Play("Run");
				// if (turning == 0)
				// 	Model.Play("Run");
				// else if (turning < 0)
				// 	Model.Play("Run", "Turn.R", -turning);
				// else if (turning > 0)
				// 	Model.Play("Run", "Turn.L", turning);

				Model.Rate = Calc.ClampedMap(velXY.Length(), 0, MaxSpeed * 2, .1f, 3);
			}
			else
			{
				Model.Play("Idle");
				Model.Rate = 1;
			}
		}
		else
		{
			// basically resets everything to the first frame of Run over and over
			Model.Clear();
			Model.Play("Run");
		}
	}

	#endregion

	#region Dashing State

	public int Dashes => dashes;
	private int dashes = 1;
	private float tDash;
	private float tDashCooldown;
	private float tDashResetCooldown;
	private float tDashResetFlash;
	private float tNoDashJump;
	private bool dashedOnGround;
	private int dashTrailsCreated;

	private bool TryDash()
	{
		if (dashes > 0 && tDashCooldown <= 0 && Controls.Dash.ConsumePress())
		{
			dashes--;
			stateMachine.State = States.Dashing;
			return true;
		}
		else return false;
	}

	private void StDashingEnter()
	{
		if (RelativeMoveInput != Vec2.Zero)
			targetFacing = RelativeMoveInput;
		Facing = targetFacing;

		lastDashHairColor = dashes <= 0 ? CNoDash : CNormal;
		dashedOnGround = onGround;
		SetDashSpeed(targetFacing);
		autoJump = true;

		tDash = DashTime;
		tDashResetCooldown = DashResetCooldown;
		tNoDashJump = .1f;
		dashTrailsCreated = 0;

		World.HitStun = .02f;

		if (dashes <= 1)
			Audio.Play(Sfx.sfx_dash_red, Position);
		else
			Audio.Play(Sfx.sfx_dash_pink, Position);

		//CancelGroundSnap();
	}

	private void StDashingExit()
	{
		tDashCooldown = DashCooldown;
		CreateDashtTrail();
	}

	private void StDashingUpdate()
	{
		Model.Play("Dash");

		tDash -= Time.Delta;
		if (tDash <= 0)
		{
			if (!onGround)
				velocity *= DashEndSpeedMult;
			stateMachine.State = States.Normal;
			return;
		}

		if (dashTrailsCreated <= 0 || (dashTrailsCreated == 1 && tDash <= DashTime * .5f))
		{
			dashTrailsCreated++;
			CreateDashtTrail();
		}

		if (Controls.Move.Value != Vec2.Zero && Vec2.Dot(Controls.Move.Value, targetFacing) >= -.2f)
		{
			targetFacing = Calc.RotateToward(targetFacing, RelativeMoveInput, DashRotateSpeed * Time.Delta, 0);
			SetDashSpeed(targetFacing);
		}

		if (tNoDashJump > 0)
			tNoDashJump -= Time.Delta;

		// dash jump
		if (dashedOnGround && tCoyote > 0 && tNoDashJump <= 0 && Controls.Jump.ConsumePress())
		{
			stateMachine.State = States.Normal;
			DashJump();
			return;
		}
	}

	private void CreateDashtTrail()
	{
		Trail? trail = null;
		foreach (var it in trails)
			if (it.Percent >= 1)
			{
				trail = it;
				break;
			}
		if (trail == null)
			trails.Add(trail = new());

		trail.Model.SetBlendedWeights(Model.GetBlendedWeights());
		trail.Hair.CopyState(Hair);
		trail.Percent = 0.0f;
		trail.Transform = Model.Transform * Matrix;
		trail.Color = lastDashHairColor;
	}

	public bool RefillDash(int amount = 1)
	{
		if (dashes < amount)
		{
			dashes = amount;
			tDashResetFlash = .05f;
			return true;
		}
		else
			return false;
	}

	private void SetDashSpeed(in Vec2 dir)
	{
		if (dashedOnGround)
			velocity = new Vec3(dir, 0) * DashSpeed;
		else
			velocity = new Vec3(dir, .4f).Normalized() * DashSpeed;

	}

	#endregion

	#region Skidding State

	private float tNoSkidJump;

	private void StSkiddingEnter()
	{
		tNoSkidJump = .1f;
		Model.Play("Skid", true);
		Audio.Play(Sfx.sfx_skid, Position);

		for (int i = 0; i < 5; i ++)
			World.Request<Dust>().Init(Position + new Vec3(targetFacing, 0) * i, new Vec3(-targetFacing, 0.0f).Normalized() * 50, 0x666666);
	}

	private void StSkiddingExit()
	{
		Model.Play("Idle", true);
	}

	private void StSkiddingUpdate()
	{
		if (tNoSkidJump > 0)
			tNoSkidJump -= Time.Delta;

		if (TryDash())
			return;

		if (RelativeMoveInput.LengthSquared() < .2f * .2f || Vec2.Dot(RelativeMoveInput, targetFacing) < .7f || !onGround)
		{
			//cancelling
			stateMachine.State = States.Normal;
			return;
		}
		else
		{
			var velXY = velocity.XY();

			// skid jump
			if (tNoSkidJump <= 0 && Controls.Jump.ConsumePress())
			{
				stateMachine.State = States.Normal;
				SkidJump();
				return;
			}

			bool dotMatches = Vec2.Dot(velXY.Normalized(), targetFacing) >= .7f;

			// acceleration
			float accel;
			if (dotMatches)
				accel = SkiddingAccel;
			else
				accel = SkiddingStartAccel;
			Calc.Approach(ref velXY, RelativeMoveInput * MaxSpeed, accel * Time.Delta);
			velocity = velocity.WithXY(velXY);
			
			// reached target
			if (dotMatches && velXY.LengthSquared() >= EndSkidSpeed * EndSkidSpeed)
			{
				stateMachine.State = States.Normal;
				return;
			}
		}
	}

	#endregion

	#region Climbing State

	private float climbCornerEase = 0;
	private Vec3 climbCornerFrom;
	private Vec3 climbCornerTo;
	private Vec2 climbCornerFacingFrom;
	private Vec2 climbCornerFacingTo;
	private Vec2? climbCornerCameraFrom;
	private Vec2? climbCornerCameraTo;
	private int climbInputSign = 1;
	private float tClimbCooldown = 0;

	private void StClimbingEnter()
	{
		Model.Play("Climb.Idle", true);
		Model.Rate = 1.8f;
		velocity = Vec3.Zero;
		climbCornerEase = 0;
		climbInputSign = 1;
		Audio.Play(Sfx.sfx_grab, Position);
	}

	private void StClimbingExit()
	{
		Model.Play("Idle");
		Model.Rate = 1.0f;
		climbingWallActor = default;
		sfxWallSlide?.Stop();
	}

	private void StClimbingUpdate()
	{
		if (!Controls.Climb.Down)
		{
			Audio.Play(Sfx.sfx_let_go, Position);
			stateMachine.State = States.Normal;
			return;
		}

		if (Controls.Jump.ConsumePress())
		{
			stateMachine.State = States.Normal;
			targetFacing = -targetFacing;
			WallJump();
			return;
		}

		CancelGroundSnap();

		var forward = new Vec3(targetFacing, 0);
		var wallUp = climbingWallNormal.UpwardPerpendicularNormal();
		var wallRight = Vec3.TransformNormal(wallUp, Matrix.CreateFromAxisAngle(climbingWallNormal, -MathF.PI / 2));
		var forceCorner = false;
		var wallSlideSoundEnabled = false;

		// only change the input direction based on the camera when we stop moving
		// so if we keep holding a direction, we keep moving the same way (even if it's flipped in the perspective)
		if (MathF.Abs(Controls.Move.Value.X) < .5f)
			climbInputSign = (Vec2.Dot(targetFacing, cameraTargetForward.XY().Normalized()) < -.4f) ? -1 : 1;

		Vec2 inputTranslated = Controls.Move.Value;
		inputTranslated.X *= climbInputSign;

		// move around
		if (climbCornerEase <= 0)
		{
			var side = wallRight * inputTranslated.X;
			var up = -wallUp * inputTranslated.Y;
			var move = side + up;

			// cancel down vector if we're on the ground
			if (move.Z < 0 && GroundCheck(out _, out _, out _))
				move.Z = 0;

			// don't climb over ledges into spikes
			// (you can still climb up into spikes if they're on the same wall as you)
			if (move.Z > 0 && World.Overlaps<SpikeBlock>(Position + Vec3.UnitZ * ClimbCheckDist + forward * (ClimbCheckDist + 1)))
				move.Z = 0;

			// don't move left/right around into a spikes
			// (you can still climb up into spikes if they're on the same wall as you)
			if (World.Overlaps<SpikeBlock>(SolidWaistTestPos + side + forward * (ClimbCheckDist + 1)))
				move -= side;

			if (MathF.Abs(move.X) < 0.1f) move.X = 0;
			if (MathF.Abs(move.Y) < 0.1f) move.Y = 0;
			if (MathF.Abs(move.Z) < 0.1f) move.Z = 0;

			if (move != Vec3.Zero)
				SweepTestMove(move * ClimbSpeed * Time.Delta, false);

			if (MathF.Abs(inputTranslated.X) < 0.25f && inputTranslated.Y >= 0)
			{
				if (inputTranslated.Y > 0 && !onGround)
				{
					if (Time.OnInterval(0.05f))
					{
						var at = Position + wallUp * 5 + new Vec3(Facing, 0) * 2;
						var vel = tPlatformVelocityStorage > 0 ? platformVelocity : Vec3.Zero;
						World.Request<Dust>().Init(at, vel);
					}
					wallSlideSoundEnabled = true;
				}

				Model.Play("Climb.Idle");
			}
			else
			{
				Model.Play("Climb.Up");

				if (Time.OnInterval(0.3f))
					Audio.Play(Sfx.sfx_handhold, Position);
			}

		}
		// perform corner lerp
		else
		{
			var ease = 1.0f - climbCornerEase;

			velocity = Vec3.Zero;
			Position = Vec3.Lerp(climbCornerFrom, climbCornerTo, ease);
			targetFacing = Calc.AngleToVector(Calc.AngleLerp(climbCornerFacingFrom.Angle(), climbCornerFacingTo.Angle(), ease));

			if (climbCornerCameraFrom.HasValue && climbCornerCameraTo.HasValue)
			{
				var angle = Calc.AngleLerp(climbCornerCameraFrom.Value.Angle(), climbCornerCameraTo.Value.Angle(), ease * 0.50f);
				cameraTargetForward = new Vec3(Calc.AngleToVector(angle), cameraTargetForward.Z);
			}

			Calc.Approach(ref climbCornerEase, 0, Time.Delta / 0.20f);
			return;
		}

		// reset corner lerp data in case we use it
		climbCornerFrom = Position;
		climbCornerFacingFrom = targetFacing;
		climbCornerCameraFrom = null;
		climbCornerCameraTo = null;

		// move around inner corners
		if (inputTranslated.X != 0 && World.SolidRayCast(SolidWaistTestPos, wallRight * inputTranslated.X, ClimbCheckDist, out RayHit hit))
		{
			Position = hit.Point + (Position - SolidWaistTestPos) + hit.Normal * WallPushoutDist;
			targetFacing = -hit.Normal.XY();
			climbingWallNormal = hit.Normal;
			climbingWallActor = hit.Actor;
		}

		// snap to walls that slope away from us
		else if (World.SolidRayCast(SolidWaistTestPos, -climbingWallNormal, ClimbCheckDist + 2, out hit) && ClimbNormalCheck(hit.Normal))
		{
			Position = hit.Point + (Position - SolidWaistTestPos) + hit.Normal * WallPushoutDist;
			targetFacing = -hit.Normal.XY();
			climbingWallNormal = hit.Normal;
			climbingWallActor = hit.Actor;
		}

		// rotate around corners due to input
		else if (
			inputTranslated.X != 0 && 
			World.SolidRayCast(SolidWaistTestPos + forward * (ClimbCheckDist + 1) + wallRight * inputTranslated.X, wallRight * -inputTranslated.X, ClimbCheckDist * 2, out hit) &&
			ClimbNormalCheck(hit.Normal))
		{
			Position = hit.Point + (Position - SolidWaistTestPos) + hit.Normal * WallPushoutDist;
			targetFacing = -hit.Normal.XY();
			climbingWallNormal = hit.Normal;
			climbingWallActor = hit.Actor;

			//if (Vec2.Dot(targetFacing, CameraForward.XY().Normalized()) < -.3f)
			{
				climbCornerCameraFrom = cameraTargetForward.XY();
				climbCornerCameraTo = targetFacing;
			}

			Model.Play("Climb.Idle");
			forceCorner = true;
		}
		// hops over tops
		else if (inputTranslated.Y < 0 && !ClimbCheckAt(Vec3.UnitZ, out _))
		{
			Audio.Play(Sfx.sfx_climb_ledge, Position);
			stateMachine.State = States.Normal;
			velocity = new(targetFacing * ClimbHopForwardSpeed, ClimbHopUpSpeed);
			tNoMove = ClimbHopNoMoveTime;
			tClimbCooldown = 0.3f;
			autoJump = false;
			AddPlatformVelocity(false);
			return;
		}
		// fall off
		else if (!TryClimb())
		{
			stateMachine.State = States.Normal;
			return;
		}

		// update wall slide sfx
		if (wallSlideSoundEnabled)
			sfxWallSlide?.Resume();
		else
			sfxWallSlide?.Stop();

		// rotate around corners nicely
		if (forceCorner || (Position - climbCornerFrom).Length() > 2)
		{
			climbCornerEase = 1.0f;
			climbCornerTo = Position;
			climbCornerFacingTo = targetFacing;
			Position = climbCornerFrom;
			targetFacing = climbCornerFacingFrom;
		}
	}

	#endregion

	#region StrawbGet State

	private Strawberry? lastStrawb;
	private Vec2 strawbGetForward;

	private void StStrawbGetEnter()
	{
		Model.Play("StrawberryGrab");
		Model.Flags = ModelFlags.StrawberryGetEffect;
		Hair.Flags = ModelFlags.StrawberryGetEffect;
		if (lastStrawb is { } strawb)
			strawb.Model.Flags = ModelFlags.StrawberryGetEffect;
		velocity = Vec3.Zero;
		strawbGetForward = (World.Camera.Position - Position).XY().Normalized();
		cameraOverride = new(World.Camera.Position, World.Camera.LookAt);
	}

	private void StStrawbGetExit()
	{
		cameraOverride = null;

		Model.Flags = ModelFlags.Default | ModelFlags.Silhouette;
		Hair.Flags = ModelFlags.Default | ModelFlags.Silhouette;

		if (lastStrawb != null && lastStrawb.BubbleTo.HasValue)
		{
			BubbleTo(lastStrawb.BubbleTo.Value);
		}

		if (lastStrawb != null)
			World.Destroy(lastStrawb);
	}

	private void StStrawbGetUpdate()
	{
		Facing = targetFacing = Calc.AngleToVector(strawbGetForward.Angle() - MathF.PI / 7);
		cameraOverride = new CameraOverride(Position + new Vec3(strawbGetForward * 50, 40), Position + Vec3.UnitZ * 6);
	}

	private CoEnumerator StStrawbGetRoutine()
	{
		yield return 2.0f;

		if (lastStrawb != null)
			Save.CurrentRecord.Strawberries.Add(lastStrawb.ID);
		
		yield return 1.2f;

		if (World.Entry.Submap)
		{
			Save.CurrentRecord.CompletedSubMaps.Add(World.Entry.Map);
			Game.Instance.Goto(new Transition()
			{
				Mode = Transition.Modes.Pop,
				ToPause = true,
				ToBlack = new SpotlightWipe(),
				StopMusic = true,
				Saving = true
			});
		}
		else
		{
			stateMachine.State = States.Normal;
		}
	}

	public void StrawbGet(Strawberry strawb)
	{
		if (stateMachine.State != States.StrawbGet)
		{
			lastStrawb = strawb;
			stateMachine.State = States.StrawbGet;
			Position = strawb.Position + Vec3.UnitZ * -3;
			lastStrawb.Position = Position + Vec3.UnitZ * 12;
		}
	}

	#endregion

	#region FeatherStart State

	private float tFeatherStart;

	private void StFeatherStartEnter()
	{
		tFeatherStart = FeatherStartTime;
	}

	private void StFeatherStartExit()
	{
	}

	private void StFeatherStartUpdate()
	{
		var input = RelativeMoveInput;
		if (input != Vec2.Zero)
			targetFacing = input.Normalized();

		SetHairColor(CFeather);
		HandleFeatherZ();

		tFeatherStart -= Time.Delta;
		if (tFeatherStart <= 0)
		{
			stateMachine.State = States.Feather;
			return;
		}

		var velXY = velocity.XY();
		Calc.Approach(ref velXY, Vec2.Zero, 200 * Time.Delta);
		velocity = velocity.WithXY(velXY);

		// dashing
		if (dashes > 0 && tDashCooldown <= 0 && Controls.Dash.ConsumePress())
		{
			stateMachine.State = States.Dashing;
			dashes--;
			return;
		}
	}

	public void FeatherGet(Feather feather)
	{
		Audio.Play(Sfx.sfx_dashcrystal, Position);
		World.HitStun = 0.05f;

		if (stateMachine.State == States.Feather)
		{
			tFeather = FeatherDuration;
			featherZ = feather.Position.Z - 2;
			Audio.Play(Sfx.sfx_feather_renew, Position);
		}
		else
		{
			stateMachine.State = States.FeatherStart;
			featherZ = feather.Position.Z - 2;
			dashes = Math.Max(dashes, 1);
			Audio.Play(Sfx.sfx_feather_get, Position);
		}
	}

	private void HandleFeatherZ()
		=> Calc.Approach(ref velocity.Z, (featherZ - Position.Z) * 40, 600 * Time.Delta);

	#endregion

	#region Feather State

	private float featherZ;
	private float tFeather;
	private float tFeatherWallBumpCooldown;
	private bool featherPlayedEndWarn = false;

	private void StFeatherEnter()
	{
		velocity = velocity.WithXY(targetFacing * FeatherStartSpeed);
		tFeather = FeatherDuration;
		Hair.Roundness = 1;
		drawModel = false;
		featherPlayedEndWarn = false;
		tFeatherWallBumpCooldown = 0;
		sfxFeather?.Resume();
	}

	private void StFeatherExit()
	{
		Hair.Roundness = 0;
		drawModel = true;
		sfxFeather?.Stop();
	}

	private void StFeatherUpdate()
	{
		const float EndWarningTime = 0.8f;

		if (tFeather > EndWarningTime || Time.BetweenInterval(.1f))
			SetHairColor(CFeather);
		else if (dashes == 2)
			SetHairColor(CTwoDashes);
		else
			SetHairColor(CNormal);

		HandleFeatherZ();

		var velXY = velocity.XY();

		var input = RelativeMoveInput;
		if (input != Vec2.Zero)
			input = input.Normalized();
		else
			input = targetFacing;

		velXY = Calc.RotateToward(velXY, input * FeatherFlySpeed, FeatherTurnSpeed * Time.Delta, FeatherAccel * Time.Delta);
		targetFacing = velXY.Normalized();
		velocity = velocity.WithXY(velXY);

		tFeather -= Time.Delta;
		tFeatherWallBumpCooldown -= Time.Delta;

		if (tFeather <= EndWarningTime && !featherPlayedEndWarn)
		{
			featherPlayedEndWarn = true;
			Audio.Play(Sfx.sfx_feather_state_end_warning, Position);
		}

		if (tFeather <= 0)
		{
			stateMachine.State = States.Normal;

			velocity.X *= FeatherExitXYMult;
			velocity.Y *= FeatherExitXYMult;
			holdJumpSpeed = velocity.Z = FeatherExitZSpeed;
			tHoldJump = .1f;
			autoJump = true;
			Audio.Play(Sfx.sfx_feather_state_end, Position);

			return;
		}

		// dashing
		if (dashes > 0 && tDashCooldown <= 0 && Controls.Dash.ConsumePress())
		{
			stateMachine.State = States.Dashing;
			dashes--;
			return;
		}

		// start climbing
		if (Controls.Climb.Down && TryClimb())
		{
			stateMachine.State = States.Climbing;
			return;
		}
	}

	#endregion

	#region Respawn State

	private void StRespawnEnter()
	{
		drawModel = drawHair = false;
		drawOrbs = true;
		drawOrbsEase = 1;
		PointShadowAlpha = 0;
		Audio.Play(Sfx.sfx_revive, Position);
	}

	private void StRespawnUpdate()
	{
		drawOrbsEase -= Time.Delta * 2;
		if (drawOrbsEase <= 0)
			stateMachine.State = States.Normal;
	}

	private void StRespawnExit()
	{
		PointShadowAlpha = 1;
		drawModel = drawHair = true;
		drawOrbs = false;
	}

	#endregion

	#region B-Side Strawb Reveal

	// TODO: should maybe be a cutscene object? idk

	private Actor? enterLookAt;

	private void StStrawbRevealEnter()
	{
	}

	private CoEnumerator StStrawbRevealRoutine()
	{
		yield return Co.SingleFrame;

		enterLookAt = World.Get<Strawberry>();

		if (enterLookAt != null)
		{
			targetFacing = (enterLookAt.Position - Position).XY().Normalized();

			var lookAt = enterLookAt.Position + new Vec3(0, 0, 3);
			var normal = (Position - lookAt).Normalized();
			var fromPos = lookAt + normal * 40 + Vec3.UnitZ * 20;
			var toPos = Position + new Vec3(0, 0, 16) + normal * 40;
			var control = (fromPos + toPos) * .5f + Vec3.UnitZ * 40;

			cameraOverride = new(fromPos, lookAt);
			World.Camera.Position = cameraOverride.Value.Position;
			World.Camera.LookAt = cameraOverride.Value.LookAt;

			yield return 1f;

			for (float p = 0; p < 1.0f; p += Time.Delta / 3)
			{
				cameraOverride = new(Utils.Bezier(fromPos, control, toPos, Ease.SineIn(p)), lookAt);
				yield return Co.SingleFrame;
			}

			for (float p = 0; p < 1.0f; p += Time.Delta / 1f)
			{
				GetCameraTarget(out var lookAtTo, out var posTo, out _);

				var t = Ease.SineOut(p);
				cameraOverride = new(Vec3.Lerp(toPos, posTo, t), Vec3.Lerp(lookAt, lookAtTo, t));
				yield return Co.SingleFrame;
			}

			yield return .02f;
		}

		stateMachine.State = States.Normal;
	}

	private void StStrawbRevealExit()
	{
		cameraOverride = null;
	}

	#endregion

	#region Dead State

	private void StDeadEnter()
	{
		drawModel = drawHair = false;
		drawOrbs = true;
		drawOrbsEase = 0;
		PointShadowAlpha = 0;
		Audio.Play(Sfx.sfx_death, Position);
	}

	private void StDeadUpdate()
	{
		if (drawOrbsEase < 1.0f)
			drawOrbsEase += Time.Delta * 2.0f;

		if (!Game.Instance.IsMidTransition && drawOrbsEase > 0.30f)
		{
			var entry = World.Entry with { Reason = World.EntryReasons.Respawned };
			Game.Instance.Goto(new Transition()
			{
				Mode = Transition.Modes.Replace,
				Scene = () => new World(entry),
				ToBlack = new AngledWipe()
			});
		}
	}

	#endregion

	#region Cutscene State

	private void StCutsceneEnter()
	{
		Model.Play("Idle");
	}

	private void StCutsceneUpdate()
	{
		if (World.All<Cutscene>().Count == 0)
			stateMachine.State = States.Normal;
	}

	#endregion

	#region Bubble State

	private Vec3 bubbleTo;

	public void BubbleTo(Vec3 target)
	{
		bubbleTo = target;
		Model.Play("StrawberryGrab");
		stateMachine.State = States.Bubble;
		PointShadowAlpha = 0;
		Audio.Play(Sfx.sfx_bubble_in, Position);
	}

	private CoEnumerator StBubbleRoutine()
	{
		Vec3 bubbleFrom = Position;
		Vec3 control = (bubbleTo + bubbleFrom) * .5f + Vec3.UnitZ * 40;
		float duration = (bubbleTo - bubbleFrom).Length() / 220;
		float ease = 0.0f;

		yield return .2f;

		sfxBubble?.Resume();
		while (ease < 1.0f)
		{			
			Calc.Approach(ref ease, 1.0f, Time.Delta / duration);
			Position = Utils.Bezier(bubbleFrom, control, bubbleTo, Utils.SineInOut(ease));
			yield return Co.SingleFrame;
		}

		yield return .2f;
		stateMachine.State = States.Normal;
	}

	private void StBubbleExit()
	{
		Audio.Play(Sfx.sfx_bubble_out, Position);
		sfxBubble?.Stop();
		PointShadowAlpha = 1;
	}

	#endregion

	#region Cassette State

	private Cassette? cassette;

	public void EnterCassette(Cassette it)
	{
		if (stateMachine.State != States.Cassette)
		{
			cassette = it;
			stateMachine.State = States.Cassette;
			Position = it.Position - Vec3.UnitZ * 3;
			drawModel = drawHair = false;
			PointShadowAlpha = 0;
			cameraOverride = new(World.Camera.Position, it.Position);
			Game.Instance.Ambience.Stop();
			Audio.StopBus(Sfx.bus_gameplay_world, false);
			Audio.Play(Sfx.sfx_cassette_enter, Position);
		}
	}

	private CoEnumerator StCassetteRoutine()
	{
		yield return 1.0f;

		if (cassette != null)
		{
			if (World.Entry.Submap) 
			{
				Game.Instance.Goto(new Transition() 
				{
					Mode = Transition.Modes.Pop,
					ToPause = true,
					ToBlack = new SpotlightWipe(),
					StopMusic = true
				});
			} 
			//Saves and quits game if you collect a cassette with an empty map property when you're not in a submap
			else if (!Assets.Maps.ContainsKey(cassette.Map)) 
			{
				Game.Instance.Goto(new Transition() 
				{
					Mode = Transition.Modes.Replace,
					Scene = () => new Overworld(true),
					ToPause = true,
					ToBlack = new SpotlightWipe(),
					FromBlack = new SlideWipe(),
					StopMusic = true,
					Saving = true
				});
  			}
			else
			{
				Game.Instance.Goto(new Transition()
				{
					Mode = Transition.Modes.Push,
					Scene = () => new World(new(cassette.Map, string.Empty, true, World.EntryReasons.Entered)),
					ToPause = true,
					ToBlack = new SpotlightWipe(),
					StopMusic = true
				});
			}
		}

		yield return 1.0f;

		Audio.Play(Sfx.sfx_cassette_exit, Position);
		cassette?.PlayerExit();

		stateMachine.State = States.Normal;
		velocity = Vec3.UnitZ * 25;
		holdJumpSpeed = velocity.Z;
		tHoldJump = .1f;
		autoJump = true;

	}

	private void StCassetteExit()
	{
		cassette?.SetCooldown();
		cassette = null;
		drawModel = drawHair = true;
		cameraOverride = null;
		PointShadowAlpha = 1;
	}

	#endregion

	#region Graphics

	public void CollectSprites(List<Sprite> populate)
	{
		// debug: draw camera origin pos
		if (World.DebugDraw)
		{
			populate.Add(Sprite.CreateBillboard(World, cameraOriginPos, "circle", 1, Color.Red));
		}

		// debug: draw wall up-normal
		if (World.DebugDraw)
		{
			if (stateMachine.State == States.Climbing)
			{
				var up = climbingWallNormal.UpwardPerpendicularNormal();

				for (int i = 0; i < 12; i++)
				{
					populate.Add(Sprite.CreateBillboard(World, SolidWaistTestPos + up * i * 1.5f, "circle", 1, Color.Red));
				}
			}
		}

		if (InBubble)
		{
			populate.Add(Sprite.CreateBillboard(World, Position + Vec3.UnitZ * 8, "bubble", 10, Color.White) with { Post = true });
		}

		if (InFeatherState)
		{
			populate.Add(Sprite.CreateBillboard(World, Position + Forward * 4 + Vec3.UnitZ * 8, "gradient", 12, CFeather * 0.50f));
		}

		if (drawOrbs && drawOrbsEase > 0)
		{
			var ease = drawOrbsEase;
			var col = Math.Floor(ease * 10) % 2 == 0 ? Hair.Color : Color.White;
			var s = (ease < 0.5f) ? (0.5f + ease) : (Ease.CubeOut(1 - (ease - 0.5f) * 2));
			for (int i = 0; i < 8; i ++)
			{
				var rot = (i / 8f + ease * 0.25f) * MathF.Tau;
				var rad = Ease.CubeOut(ease) * 16;
				var pos = SolidWaistTestPos + World.Camera.Left * MathF.Cos(rot) * rad + World.Camera.Up * MathF.Sin(rot) * rad;
				var size = 3 * s;
				populate.Add(Sprite.CreateBillboard(World, pos, "circle", size + 0.5f, Color.Black) with { Post = true });
				populate.Add(Sprite.CreateBillboard(World, pos, "circle", size, col) with { Post = true });
			}
		}

		if (!onGround && !Dead && PointShadowAlpha > 0 && !InBubble && Save.Instance.ZGuide)
		{
			var distance = 1000.0f;
			if (World.SolidRayCast(Position, -Vec3.UnitZ, distance, out var hit))
				distance = hit.Distance;

			for (int i = 3; i < distance; i += 5)
				populate.Add(Sprite.CreateBillboard(World, Position - Vec3.UnitZ * i, "circle", 0.5f, Color.Gray * 0.50f));
		}	
	}

	public void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		if ((World.Camera.Position - (Position + Vec3.UnitZ * 8)).LengthSquared() > World.Camera.NearPlane * World.Camera.NearPlane)
		{
			if (drawHair)
				populate.Add((this, Hair));

			if (drawModel)
				populate.Add((this, Model));
		}

		foreach (var trail in trails)
		{
			if (trail.Percent >= 1)
				continue;

			// I HATE this alpha fade out but don't have time to make some kind of full-model fade out effect
			var alpha = Ease.CubeOut(Calc.ClampedMap(trail.Percent, 0.5f, 1.0f, 1, 0));

			foreach (var mat in trail.Model.Materials)
				mat.Color = trail.Color * alpha;
			trail.Hair.Color = trail.Color * alpha;

			if (Matrix.Invert(Matrix, out var inverse))
				trail.Model.Transform = trail.Transform * inverse;

			populate.Add((this, trail.Model));
			populate.Add((this, trail.Hair));
		}
	}

	#endregion

	#region Platform Riding / Solid Checks

	public void RidingPlatformSetVelocity(in Vec3 value)
	{
		if (value == Vec3.Zero)
			return;

		if (tPlatformVelocityStorage < 0 || value.Z >= velocity.Z 
		|| value.XY().LengthSquared() + .1f >= velocity.XY().LengthSquared()
		|| (value.XY() != Vec2.Zero && Vec2.Dot(value.XY().Normalized(), velocity.XY().Normalized()) < .5f))
		{
			platformVelocity = value;
			tPlatformVelocityStorage = .1f;
		}
	}

	public bool RidingPlatformCheck(Actor platform)
	{
		// check if we're climbing this thing
		if (platform == climbingWallActor)
			return true;

		// check if we're anywhere near it first before doing a ground check
		if (!WorldBounds.Inflate(10).Intersects(platform.WorldBounds))
			return false;

		// check for ground below us
		return GroundCheck(out _, out _, out var floor) && floor == platform;
	}

	public void RidingPlatformMoved(in Vec3 delta)
	{
		var was = Position;
		SweepTestMove(delta, false);
		var newDelta = (Position - was);
		climbCornerFrom += newDelta;
		climbCornerTo += newDelta;
	}

	public bool GroundCheck(out Vec3 pushout, out Vec3 normal, out Actor? floor)
	{
		pushout = default;
		floor = null;
		normal = Vec3.UnitZ;

		if (World.SolidRayCast(Position + Vec3.UnitZ * 5, -Vec3.UnitZ, 5.01f, out var hit))
		{
			pushout = hit.Point - Position;
			floor = hit.Actor;
			normal = hit.Normal;
			return true;
		}

		return false;
	}

	public bool CeilingCheck(out Vec3 pushout)
	{
		const float Height = 12;

		pushout = default;

		if (World.SolidRayCast(Position + Vec3.UnitZ, Vec3.UnitZ, Height - 1, out var hit))
		{
			pushout = hit.Point - Vec3.UnitZ * Height - Position;
			return true;
		}

		return false;
	}

	public void Stop() => velocity = Vec3.Zero;

	#endregion
}
