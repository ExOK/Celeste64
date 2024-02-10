
using System.Collections.ObjectModel;

namespace Celeste64;

/// <summary>
/// Welcome to the monolithic player class! This time only 2300 lines ;)
/// </summary>
public class Player : Actor, IHaveModels, IHaveSprites, IRidePlatforms, ICastPointShadow
{
	#region Constants

	public virtual float defaultAcceleration { get { return 500; } }
	public virtual float defaultPastMaxDeccel { get { return 60; } }
	public virtual float defaultAirAccelMultMin { get { return .5f; } }
	public virtual float defaultAirAccelMultMax { get { return 1f; } }
	public virtual float defaultMaxSpeed { get { return 64; } }
	public virtual float defaultRotateSpeed { get { return MathF.Tau * 1.5f; } }
	public virtual float defaultRotateSpeedAboveMax { get { return MathF.Tau * .6f; } }
	public virtual float defaultFriction { get { return 800; } }
	public virtual float defaultAirFrictionMult { get { return .1f; } }
	public virtual float defaultGravity { get { return 600; } }
	public virtual float defaultMaxFall { get { return -120; } }
	public virtual float defaultHalfGravThreshold { get { return 100; } }
	public virtual float defaultJumpHoldTime { get { return .1f; } }
	public virtual float defaultJumpSpeed { get { return 90; } }
	public virtual float defaultJumpXYBoost { get { return 10; } } 
	public virtual float defaultCoyoteTime { get { return .12f; } }

	public virtual float defaultDashSpeed { get { return 140; } }
	public virtual float defaultDashEndSpeedMult { get { return .75f; } }
	public virtual float defaultDashTime { get { return .2f; } }
	public virtual float defaultDashResetCooldown { get { return .2f; } }
	public virtual float defaultDashCooldown { get { return .1f; } }
	public virtual float defaultDashRotateSpeed { get { return MathF.Tau * .3f; } }

	public virtual float defaultDashJumpSpeed { get { return 40; } }
	public virtual float defaultDashJumpHoldSpeed { get { return 20; } }
	public virtual float defaultDashJumpHoldTime { get { return .3f; } }
	public virtual float defaultDashJumpXYBoost { get { return 16; } }

	public virtual float defaultSkidDotThreshold { get { return -.7f; } }
	public virtual float defaultSkiddingStartAccel { get { return 300; } }
	public virtual float defaultSkiddingAccel { get { return 500; } }
	public virtual float defaultSkidJumpSpeed { get { return 120; } }
	public virtual float defaultSkidJumpHoldTime { get { return .16f; } }

	public virtual float defaultWallPushoutDist { get { return 3; } }
	public virtual float defaultClimbCheckDist { get { return 4; } }
	public virtual float defaultClimbSpeed { get { return 40; } }
	public virtual float defaultClimbHopUpSpeed { get { return 80; } }
	public virtual float defaultClimbHopForwardSpeed { get { return 40; } }
	public virtual float defaultClimbHopNoMoveTime { get { return .25f; } }

	public virtual float defaultSpringJumpSpeed { get { return 160; } }
	public virtual float defaultSpringJumpHoldTime { get { return .3f; } }

	public virtual float defaultFeatherStartTime { get { return .4f; ; } }
	public virtual float defaultFeatherFlySpeed { get { return 100; } }
	public virtual float defaultFeatherStartSpeed { get { return 140; } }
	public virtual float defaultFeatherTurnSpeed { get { return MathF.Tau * .75f; } }
	public virtual float defaultFeatherAccel { get { return 60; } }
	public virtual float defaultFeatherDuration { get { return 2.2f; } } 
	public virtual float defaultFeatherExitXYMult { get { return .5f; } }
	public virtual float defaultFeatherExitZSpeed { get { return 60; } }
	#endregion


	public virtual float Acceleration { get; set; }
	public virtual float PastMaxDeccel { get; set; }
	public virtual float AirAccelMultMin { get; set; }
	public virtual float AirAccelMultMax { get; set; }
	public virtual float MaxSpeed { get; set; }
	public virtual float RotateThreshold { get { return MaxSpeed * .2f; } }
	public virtual float RotateSpeed { get; set; }
	public virtual float RotateSpeedAboveMax { get; set; }
	public virtual float Friction { get; set; }
	public virtual float AirFrictionMult { get; set; }
	public virtual float Gravity { get; set; }
	public virtual float MaxFall { get; set; }
	public virtual float HalfGravThreshold { get; set; }
	public virtual float JumpHoldTime { get; set; }
	public virtual float JumpSpeed { get; set; }
	public virtual float JumpXYBoost { get; set; }
	public virtual float CoyoteTime { get; set; }
	public virtual float WallJumpXYSpeed { get { return MaxSpeed * 1.3f; } }

	public virtual float DashSpeed { get; set; }
	public virtual float DashEndSpeedMult { get; set; }
	public virtual float DashTime { get; set; }
	public virtual float DashResetCooldown { get; set; }
	public virtual float DashCooldown { get; set; }
	public virtual float DashRotateSpeed { get; set; }

	public virtual float DashJumpSpeed { get; set; }
	public virtual float DashJumpHoldSpeed { get; set; }
	public virtual float DashJumpHoldTime { get; set; }
	public virtual float DashJumpXYBoost { get; set; }

	public virtual float SkidDotThreshold { get; set; }
	public virtual float SkiddingStartAccel { get; set; }
	public virtual float SkiddingAccel { get; set; }
	public virtual float EndSkidSpeed { get { return MaxSpeed * 0.8f;} }
	public virtual float SkidJumpSpeed { get; set; }
	public virtual float SkidJumpHoldTime { get; set; }
	public virtual float SkidJumpXYSpeed { get { return MaxSpeed * 1.4f; } }

	public virtual float WallPushoutDist { get; set; }
	public virtual float ClimbCheckDist { get; set; }
	public virtual float ClimbSpeed { get; set; }
	public virtual float ClimbHopUpSpeed { get; set; }
	public virtual float ClimbHopForwardSpeed { get; set; }
	public virtual float ClimbHopNoMoveTime { get; set; }

	public virtual float SpringJumpSpeed { get; set; }
	public virtual float SpringJumpHoldTime { get; set; }

	public virtual float FeatherStartTime { get; set; }
	public virtual float FeatherFlySpeed { get; set; }
	public virtual float FeatherStartSpeed { get; set; }
	public virtual float FeatherTurnSpeed { get; set; }
	public virtual float FeatherAccel { get; set; }
	public virtual float FeatherDuration { get; set; }
	public virtual float FeatherExitXYMult { get; set; }
	public virtual float FeatherExitZSpeed { get; set; }

	protected void ResetDefaultValues()
	{
		Acceleration = defaultAcceleration;
		PastMaxDeccel = defaultPastMaxDeccel;
		AirAccelMultMin = defaultAirAccelMultMin;
		AirAccelMultMax = defaultAirAccelMultMax;
		MaxSpeed = defaultMaxSpeed;
		RotateSpeed = defaultRotateSpeed;
		RotateSpeedAboveMax = defaultRotateSpeedAboveMax;
		Friction = defaultFriction;
		AirFrictionMult = defaultAirFrictionMult;
		Gravity = defaultGravity;
		MaxFall = defaultMaxFall;
		HalfGravThreshold = defaultHalfGravThreshold;
		JumpHoldTime = defaultJumpHoldTime;
		JumpSpeed = defaultJumpSpeed;
		JumpXYBoost = defaultJumpXYBoost;
		CoyoteTime = defaultCoyoteTime;

		DashSpeed = defaultDashSpeed;
		DashEndSpeedMult = defaultDashEndSpeedMult;
		DashTime = defaultDashTime;
		DashResetCooldown = defaultDashResetCooldown;
		DashCooldown = defaultDashCooldown;
		DashRotateSpeed = defaultDashRotateSpeed;

		DashJumpSpeed = defaultDashJumpSpeed;
		DashJumpHoldSpeed = defaultDashJumpHoldSpeed;
		DashJumpHoldTime = defaultDashJumpHoldTime;
		DashJumpXYBoost = defaultDashJumpXYBoost;

		SkidDotThreshold = defaultSkidDotThreshold;
		SkiddingStartAccel = defaultSkiddingStartAccel;
		SkiddingAccel = defaultSkiddingAccel;
		SkidJumpSpeed = defaultSkidJumpSpeed;
		SkidJumpHoldTime = defaultSkidJumpHoldTime;

		WallPushoutDist = defaultWallPushoutDist;
		ClimbCheckDist = defaultClimbCheckDist;
		ClimbSpeed = defaultClimbSpeed;
		ClimbHopUpSpeed = defaultClimbHopUpSpeed;
		ClimbHopForwardSpeed = defaultClimbHopForwardSpeed;
		ClimbHopNoMoveTime = defaultClimbHopNoMoveTime;

		SpringJumpSpeed = defaultSpringJumpSpeed;
		SpringJumpHoldTime = defaultSpringJumpHoldTime;

		FeatherStartTime = defaultFeatherStartTime;
		FeatherFlySpeed = defaultFeatherFlySpeed;
		FeatherStartSpeed = defaultFeatherStartSpeed;
		FeatherTurnSpeed = defaultFeatherTurnSpeed;
		FeatherAccel = defaultFeatherAccel;
		FeatherDuration = defaultFeatherDuration;
		FeatherExitXYMult = defaultFeatherExitXYMult;
	}

	// These are no longer used. This gets populated from SkinInfo.
	protected static readonly Color CNormal = 0xdb2c00;
	protected static readonly Color CNoDash = 0x6ec0ff;
	protected static readonly Color CTwoDashes = 0xfa91ff;
	protected static readonly Color CRefillFlash = Color.White;
	protected static readonly Color CFeather = 0xf2d450;


	#region SubClasses

	protected class Trail
	{
		public readonly Hair Hair;
		public readonly SkinnedModel Model;
		public Matrix Transform;
		public float Percent;
		public Color Color;

		public Trail(string model = "player")
		{
			Model = new(Assets.Models[model]);
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
	public static Vec3 storedCameraForward;
	public static float storedCameraDistance;

	public enum States { Normal, Dashing, Skidding, Climbing, StrawbGet, FeatherStart, Feather, Respawn, Dead, StrawbReveal, Cutscene, Bubble, Cassette };
	public enum Events { Land };

	public bool Dead = false;

	public Vec3 ModelScale = Vec3.One;
	public SkinnedModel Model;
	public readonly Hair Hair = new();
	public virtual float PointShadowAlpha { get; set; } = 1.0f;

	public virtual Vec3 Velocity => velocity;
	public virtual Vec3 PreviousVelocity => previousVelocity;

	public SkinInfo Skin;

	public Vec3 velocity;
	public Vec3 previousVelocity;
	public Vec3 groundNormal;
	public Vec3 platformVelocity;
	public float tPlatformVelocityStorage;
	public float tGroundSnapCooldown;
	public Actor? climbingWallActor;
	public Vec3 climbingWallNormal;

	public bool onGround;
	public Vec2 targetFacing = Vec2.UnitY;
	public Vec3 cameraTargetForward = new(0, 1, 0);
	public float cameraTargetDistance = 0.50f;
	public readonly StateMachine<States, Events> StateMachine;

	protected record struct CameraOverride(Vec3 Position, Vec3 LookAt);
	protected CameraOverride? cameraOverride = null;
	protected Vec3 cameraOriginPos;
	protected Vec3 cameraDestinationPos;

	protected float tCoyote;
	protected float coyoteZ;

	protected bool drawModel = true;
	protected bool drawHair = true;
	protected bool drawOrbs = false;
	protected float drawOrbsEase = 0;

	protected readonly List<Trail> trails = [];
	protected readonly Func<SpikeBlock, bool> spikeBlockCheck;
	protected Color lastDashHairColor;

	protected Sound? sfxWallSlide;
	protected Sound? sfxFeather;
	protected Sound? sfxBubble;

	protected Vec3 SolidWaistTestPos 
		=> Position + Vec3.UnitZ * 3;
	protected Vec3 SolidHeadTestPos 
		=> Position + Vec3.UnitZ * 10;

	public virtual bool InFeatherState 
		=> StateMachine.State == States.FeatherStart
		|| StateMachine.State == States.Feather;

	public virtual bool InBubble
		=> StateMachine.State == States.Bubble;

	public virtual bool IsStrawberryCounterVisible
		=> StateMachine.State == States.StrawbGet;

	public virtual bool IsAbleToPickup
		=> StateMachine.State != States.StrawbGet 
		&& StateMachine.State != States.Bubble 
		&& StateMachine.State != States.Cassette 
		&& StateMachine.State != States.StrawbReveal 
		&& StateMachine.State != States.Respawn
		&& StateMachine.State != States.Dead
		&& GetCurrentCustomState() is not { IsAbleToPickup: false };

	public virtual bool IsAbleToPause 
		=> StateMachine.State != States.StrawbReveal
		&& StateMachine.State != States.StrawbGet
		&& StateMachine.State != States.Cassette
		&& StateMachine.State != States.Dead
		&& GetCurrentCustomState() is not { IsAbleToPause: false };

	private List<StatusEffect> statusEffects { get; } = new List<StatusEffect>();

	public ReadOnlyCollection<StatusEffect> StatusEffects => statusEffects.AsReadOnly();


	public Player()
	{
		ResetDefaultValues();
		PointShadowAlpha = 1.0f;
		LocalBounds = new BoundingBox(new Vec3(0, 0, 10), 10);
		UpdateOffScreen = true;
		Skin = Save.Instance.GetSkin();

		// setup model
		{
			Model = new(Assets.Models[Skin.Model]);
			Model.SetBlendDuration("Idle", "Dash", 0.05f);
			Model.SetBlendDuration("Idle", "Run", 0.2f);
			Model.SetBlendDuration("Run", "Skid", .125f);
			Model.SetLooping("Dash", false);
			Model.Flags |= ModelFlags.Silhouette;
			Model.Play("Idle");

			for (int i = 0; i < Model.Materials.Count; i++)
			{
				string name = Model.Materials[i].Name;
				Model.Materials[i] = Model.Materials[i].Clone();
				Model.Materials[i].Name = name;
				Model.Materials[i].Effects = 0.60f;
			}
		}

		StateMachine = new(additionalStateCount: CustomPlayerStateRegistry.RegisteredStates.Count);
		StateMachine.InitState(States.Normal, StNormalUpdate, StNormalEnter, StNormalExit);
		StateMachine.InitState(States.Dashing, StDashingUpdate, StDashingEnter, StDashingExit);
		StateMachine.InitState(States.Skidding, StSkiddingUpdate, StSkiddingEnter, StSkiddingExit);
		StateMachine.InitState(States.Climbing, StClimbingUpdate, StClimbingEnter, StClimbingExit);
		StateMachine.InitState(States.StrawbGet, StStrawbGetUpdate, StStrawbGetEnter, StStrawbGetExit, StStrawbGetRoutine);
		StateMachine.InitState(States.FeatherStart, StFeatherStartUpdate, StFeatherStartEnter, StFeatherStartExit);
		StateMachine.InitState(States.Feather, StFeatherUpdate, StFeatherEnter, StFeatherExit);
		StateMachine.InitState(States.Respawn, StRespawnUpdate, StRespawnEnter, StRespawnExit);
		StateMachine.InitState(States.StrawbReveal, null, StStrawbRevealEnter, StStrawbRevealExit, StStrawbRevealRoutine);
		StateMachine.InitState(States.Cutscene, StCutsceneUpdate, StCutsceneEnter);
		StateMachine.InitState(States.Dead, StDeadUpdate, StDeadEnter);
		StateMachine.InitState(States.Bubble, null, null, StBubbleExit, StBubbleRoutine);
		StateMachine.InitState(States.Cassette, null, null, StCassetteExit, StCassetteRoutine);
		// Register custom player states
		var nextId = CustomPlayerStateRegistry.BaseId;
		foreach (var customState in CustomPlayerStateRegistry.RegisteredStates)
		{
			StateMachine.InitState(nextId++, 
				() => customState.Update(this),
				() => customState.OnBegin(this),
				() => customState.OnEnd(this),
				() => customState.Routine(this)
			);
		}
		StateMachine.OnStateChanged += HandleStateChange;

		spikeBlockCheck = (spike) =>
		{
			return Vec3.Dot(velocity.Normalized(), spike.Direction) < 0.5f;
		};

		SetHairColor(0xdb2c00);
	}

	/// <summary>
	/// If the player is in a custom state, returns its definition.
	/// Otherwise, returns null.
	/// </summary>
	public virtual CustomPlayerState? GetCurrentCustomState()
	{
		if (StateMachine.State is not { } state)
		{
			return null;
		}

		return CustomPlayerStateRegistry.GetById(state);
	}
	
	/// <summary>
	/// Checks whether the player is currently in the provided custom state.
	/// </summary>
	public virtual bool IsInState<T>() where T : CustomPlayerState
	{
		var stateDef = GetCurrentCustomState();

		return stateDef is T;
	}

	/// <summary>
	/// Sets the player's state to the provided custom state.
	/// </summary>
	public virtual void SetState<T>() where T : CustomPlayerState
	{
		var id = CustomPlayerStateRegistry.GetId<T>();

		StateMachine.State = id;
	}

	/// <summary>
	/// Sets the player's state to the provided vanilla state.
	/// </summary>
	public virtual void SetState(States state)
	{
		StateMachine.State = state;
	}

	protected virtual void HandleStateChange(States? state)
	{
		ModManager.Instance.OnPlayerStateChanged(this, state);
	}

	public StatusEffect AddStatusEffect<T>(bool RemoveAfterDuration = false, float DurationOverride = 10) where T : StatusEffect, new()
	{
		StatusEffect? existingEffect = GetStatusEffect<T>();
		if (existingEffect != null && !existingEffect.RemoveOnReapply)
		{
			return existingEffect;
		}
		else if (existingEffect != null)
		{
			RemoveStatusEffect(existingEffect);
		}
		StatusEffect newEffect = new T()
		{
			Player = this,
			World = World,
			Duration = DurationOverride,
			RemoveAfterDuration = RemoveAfterDuration
		};
		statusEffects.Add(newEffect);
		newEffect.OnStatusEffectAdded();
		return newEffect;
	}

	public void RemoveStatusEffect<T>() where T : StatusEffect
	{
		StatusEffect? existingEffect = GetStatusEffect<T>();
		if (existingEffect != null)
		{
			existingEffect.OnStatusEffectRemoved();

			statusEffects.Remove(existingEffect);
		}
	}

	public void RemoveStatusEffect(StatusEffect effect)
	{
		effect.OnStatusEffectRemoved();
		statusEffects.Remove(effect);
	}

	public bool HasStatusEffect<T>() where T : StatusEffect
	{
		return statusEffects.Any(effect => effect.GetType() == typeof(T));
	}

	public StatusEffect? GetStatusEffect<T>() where T : StatusEffect
	{
		return statusEffects.FirstOrDefault(effect => effect.GetType() == typeof(T));
	}

	#region Added / Update

	public override void Added()
	{
		if (World.Entry.Reason == World.EntryReasons.Respawned)
		{
			cameraTargetForward = storedCameraForward;
			cameraTargetDistance = storedCameraDistance;
			StateMachine.State = States.Respawn;
		}
		else if (World.Entry.Submap && World.Entry.Reason == World.EntryReasons.Entered)
		{
			StateMachine.State = States.StrawbReveal;
		}
		else
		{
			StateMachine.State = States.Normal;
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
		if (StateMachine.State != States.Respawn && StateMachine.State != States.Dead && 
			StateMachine.State != States.StrawbReveal && StateMachine.State != States.Cassette)
		{
			// Rotate Camera
			{
				var invertX = Save.Instance.InvertCamera == Save.InvertCameraOptions.X || Save.Instance.InvertCamera == Save.InvertCameraOptions.Both;
				var rot = new Vec2(cameraTargetForward.X, cameraTargetForward.Y).Angle();
				rot -= Controls.Camera.Value.X * Time.Delta * 4 * (invertX ? -1 : 1);

				var angle = Calc.AngleToVector(rot);
				cameraTargetForward = new(angle, 0);
			}

			// Move Camera in / out
			if (Controls.Camera.Value.Y != 0)
			{
				var invertY = Save.Instance.InvertCamera == Save.InvertCameraOptions.Y || Save.Instance.InvertCamera == Save.InvertCameraOptions.Both;
				cameraTargetDistance += Controls.Camera.Value.Y * Time.Delta * (invertY ? -1 : 1);
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
		if (StateMachine.State == States.Respawn || StateMachine.State == States.Dead || StateMachine.State == States.Cutscene)
		{
			StateMachine.Update();
			return;
		}

		foreach (var statusEffect in statusEffects.ToList())
		{
			statusEffect.Update(Time.Delta);
			statusEffect.UpdateDuration(Time.Delta);
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
			StateMachine.State = States.Cutscene;

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
		StateMachine.Update();

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
					{
						pickup.Pickup(this);
						ModManager.Instance.OnItemPickup(this, pickup);
					}
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
				StateMachine.CallEvent(Events.Land);
				ModManager.Instance.OnPlayerLanded(this);

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
			float ZPad = StateMachine.State == States.Climbing ? 0 : 8;
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

			if (StateMachine.State != States.Feather && StateMachine.State != States.FeatherStart
			    && GetCurrentCustomState() is not { ControlHairColor: true })
			{
				Color color;
				if (tDashResetFlash > 0)
					color = Skin.HairRefillFlash;
				else if (dashes == 1)
					color = Skin.HairNormal;
				else if (dashes == 0)
					color = Skin.HairNoDash;
				else
					color = Skin.HairTwoDash;

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
	
	public virtual void GetCameraTarget(out Vec3 cameraLookAt, out Vec3 cameraPosition, out bool snapRequested)
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

	public virtual void SetSkin(SkinInfo skin)
	{
		if(this.Skin != skin)
		{
			this.Skin = skin;

			Model = new(Assets.Models[this.Skin.Model]);
			Model.SetBlendDuration("Idle", "Dash", 0.05f);
			Model.SetBlendDuration("Idle", "Run", 0.2f);
			Model.SetBlendDuration("Run", "Skid", .125f);
			Model.SetLooping("Dash", false);
			Model.Flags |= ModelFlags.Silhouette;
			Model.Play("Idle");

			for (int i = 0; i < Model.Materials.Count; i++)
			{
				string name = Model.Materials[i].Name;
				Model.Materials[i] = Model.Materials[i].Clone();
				Model.Materials[i].Name = name;
				Model.Materials[i].Effects = 0.60f;
			}

			trails.Clear();
			dashTrailsCreated = 0;
		}
	}

	public virtual Vec2 RelativeMoveInput
	{
		get
		{
			if (Controls.Move.Value == Vec2.Zero)
				return Vec2.Zero;

			Vec2 forward, side;
			
            var cameraForward = (World.Camera.LookAt - World.Camera.Position).Normalized().XY();
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

	public virtual void SetTargetFacing(Vec2 facing)
	{
		targetFacing = facing;
	}

	public virtual void SetHairColor(Color color)
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

	public virtual void SweepTestMove(Vec3 delta, bool resolveImpact)
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
	public virtual bool Popout(bool resolveImpact)
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
			if (resolveImpact && StateMachine.State == States.Feather && tFeatherWallBumpCooldown <= 0 && !(Controls.Climb.Down && TryClimb()))
			{
				Position += hit.Pushout;
				velocity = velocity.WithXY(Vec2.Reflect(velocity.XY(), hit.Normal.XY().Normalized()));
				tFeatherWallBumpCooldown = 0.50f;
				Audio.Play(Sfx.sfx_feather_state_bump_wall, Position);
			}
			// does it handle being dashed into?
			else if (resolveImpact && hit.Actor is IDashTrigger trigger && !hit.Actor.Destroying && velocity.XY().Length() > 90)
			{
                World.HitStun = 0.1f;
                trigger.HandleDash(velocity);

                if (trigger.BouncesPlayer)
                {
                    velocity.X = -velocity.X * 0.80f;
                    velocity.Y = -velocity.Y * 0.80f;
                    velocity.Z = 100;

                    StateMachine.State = States.Normal;
                    CancelGroundSnap();
                }
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

	public virtual void CancelGroundSnap() =>
		tGroundSnapCooldown = 0.1f;

	protected virtual void Jump()
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

	protected virtual void WallJump()
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

	protected virtual void SkidJump()
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

	protected virtual void DashJump()
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

	protected virtual void AddPlatformVelocity(bool playSound)
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

	public virtual void Kill()
	{
		StateMachine.State = States.Dead;
		storedCameraForward = cameraTargetForward;
		storedCameraDistance = cameraTargetDistance;
		Save.CurrentRecord.Deaths++;
		Dead = true;
		ModManager.Instance.OnPlayerKill(this);
		foreach (var statusEffect in statusEffects.ToList())
		{
			statusEffect.Update(Time.Delta);
			statusEffect.UpdateDuration(Time.Delta);
		}
	}

	protected virtual bool ClimbCheckAt(Vec3 offset, out WallHit hit)
	{
		if (World.SolidWallCheckClosestToNormal(SolidWaistTestPos + offset, ClimbCheckDist, -new Vec3(targetFacing, 0), out hit)
		&& (RelativeMoveInput == Vec2.Zero || Vec2.Dot(hit.Normal.XY().Normalized(), RelativeMoveInput) <= -0.5f)
		&& ClimbNormalCheck(hit.Normal))
			return true;
		return false;
	}

	protected virtual bool TryClimb()
	{
		var result = ClimbCheckAt(Vec3.Zero, out var wall);

		// let us snap up to walls if we're jumping for them
		// note: if vel.z is allowed to be downwards then we awkwardly re-grab when sliding off
		// the bottoms of walls, which is really bad feeling
		if (!result && Velocity.Z > 0 && !onGround && StateMachine.State != States.Climbing)
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

	protected virtual bool ClimbNormalCheck(in Vec3 normal)
	{
		return MathF.Abs(normal.Z) < 0.35f; 
	}

	protected virtual bool FloorNormalCheck(in Vec3 normal)
		=> !ClimbNormalCheck(normal) && normal.Z > 0;

	protected virtual bool WallJumpCheck()
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

	internal void Spring(Spring spring)
	{
		StateMachine.State = States.Normal;

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

	protected const float FootstepInterval = .3f;

	protected float tHoldJump;
	protected float holdJumpSpeed;
	protected bool autoJump;
	protected float tNoMove;
	protected float tFootstep;

	protected virtual void StNormalEnter()
	{
		tHoldJump = 0;
		tFootstep = FootstepInterval;
	}

	protected virtual void StNormalExit()
	{
		tHoldJump = 0;
		tNoMove = 0;
		autoJump = false;
		Model.Rate = 1;
	}

	protected virtual void StNormalUpdate()
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
						StateMachine.State = States.Skidding;
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
			StateMachine.State = States.Climbing;
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

	public virtual int Dashes => dashes;
	protected int dashes = 1;
	protected float tDash;
	protected float tDashCooldown;
	protected float tDashResetCooldown;
	protected float tDashResetFlash;
	protected float tNoDashJump;
	protected bool dashedOnGround;
	protected int dashTrailsCreated;

	protected virtual bool TryDash()
	{
		if (dashes > 0 && tDashCooldown <= 0 && Controls.Dash.ConsumePress())
		{
			dashes--;
			StateMachine.State = States.Dashing;
			return true;
		}
		else return false;
	}

	protected virtual void StDashingEnter()
	{
		if (RelativeMoveInput != Vec2.Zero)
			targetFacing = RelativeMoveInput;
		Facing = targetFacing;

		lastDashHairColor = dashes <= 0 ? Skin.HairNoDash : Skin.HairNormal;
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

	protected virtual void StDashingExit()
	{
		tDashCooldown = DashCooldown;
		CreateDashtTrail();
	}

	protected virtual void StDashingUpdate()
	{
		Model.Play("Dash");

		tDash -= Time.Delta;
		if (tDash <= 0)
		{
			if (!onGround)
				velocity *= DashEndSpeedMult;
			StateMachine.State = States.Normal;
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
			StateMachine.State = States.Normal;
			DashJump();
			return;
		}
	}

	protected virtual void CreateDashtTrail()
	{
		Trail? trail = null;
		foreach (var it in trails)
			if (it.Percent >= 1)
			{
				trail = it;
				break;
			}
		if (trail == null)
			trails.Add(trail = new(Skin.Model));

		trail.Model.SetBlendedWeights(Model.GetBlendedWeights());
		trail.Hair.CopyState(Hair);
		trail.Percent = 0.0f;
		trail.Transform = Model.Transform * Matrix;
		trail.Color = lastDashHairColor;
	}

	public virtual bool RefillDash(int amount = 1)
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

	protected virtual void SetDashSpeed(in Vec2 dir)
	{
		if (dashedOnGround)
			velocity = new Vec3(dir, 0) * DashSpeed;
		else
			velocity = new Vec3(dir, .4f).Normalized() * DashSpeed;

	}

	#endregion

	#region Skidding State

	protected float tNoSkidJump;

	protected virtual void StSkiddingEnter()
	{
		tNoSkidJump = .1f;
		Model.Play("Skid", true);
		Audio.Play(Sfx.sfx_skid, Position);

		for (int i = 0; i < 5; i ++)
			World.Request<Dust>().Init(Position + new Vec3(targetFacing, 0) * i, new Vec3(-targetFacing, 0.0f).Normalized() * 50, 0x666666);
	}

	protected virtual void StSkiddingExit()
	{
		Model.Play("Idle", true);
	}

	protected virtual void StSkiddingUpdate()
	{
		if (tNoSkidJump > 0)
			tNoSkidJump -= Time.Delta;

		if (TryDash())
			return;

		if (RelativeMoveInput.LengthSquared() < .2f * .2f || Vec2.Dot(RelativeMoveInput, targetFacing) < .7f || !onGround)
		{
			//cancelling
			StateMachine.State = States.Normal;
			return;
		}
		else
		{
			var velXY = velocity.XY();

			// skid jump
			if (tNoSkidJump <= 0 && Controls.Jump.ConsumePress())
			{
				StateMachine.State = States.Normal;
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
				StateMachine.State = States.Normal;
				return;
			}
		}
	}

	#endregion

	#region Climbing State

	protected float climbCornerEase = 0;
	protected Vec3 climbCornerFrom;
	protected Vec3 climbCornerTo;
	protected Vec2 climbCornerFacingFrom;
	protected Vec2 climbCornerFacingTo;
	protected Vec2? climbCornerCameraFrom;
	protected Vec2? climbCornerCameraTo;
	protected int climbInputSign = 1;
	protected float tClimbCooldown = 0;

	protected virtual void StClimbingEnter()
	{
		Model.Play("Climb.Idle", true);
		Model.Rate = 1.8f;
		velocity = Vec3.Zero;
		climbCornerEase = 0;
		climbInputSign = 1;
		Audio.Play(Sfx.sfx_grab, Position);
	}

	protected virtual void StClimbingExit()
	{
		Model.Play("Idle");
		Model.Rate = 1.0f;
		climbingWallActor = default;
		sfxWallSlide?.Stop();
	}

	protected virtual void StClimbingUpdate()
	{
		if (!Controls.Climb.Down)
		{
			Audio.Play(Sfx.sfx_let_go, Position);
			StateMachine.State = States.Normal;
			return;
		}

		if (Controls.Jump.ConsumePress())
		{
			StateMachine.State = States.Normal;
			targetFacing = -targetFacing;
			WallJump();
			return;
		}

		if (dashes > 0 && tDashCooldown <= 0 && Controls.Dash.ConsumePress())
		{
			StateMachine.State = States.Dashing;
			dashes--;
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
			StateMachine.State = States.Normal;
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
			StateMachine.State = States.Normal;
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

	protected Strawberry? lastStrawb;
	protected Vec2 strawbGetForward;

	protected virtual void StStrawbGetEnter()
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

	protected virtual void StStrawbGetExit()
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

	protected virtual void StStrawbGetUpdate()
	{
		Facing = targetFacing = Calc.AngleToVector(strawbGetForward.Angle() - MathF.PI / 7);
		cameraOverride = new CameraOverride(Position + new Vec3(strawbGetForward * 50, 40), Position + Vec3.UnitZ * 6);
	}

	protected virtual CoEnumerator StStrawbGetRoutine()
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
			StateMachine.State = States.Normal;
		}
	}

	public virtual void StrawbGet(Strawberry strawb)
	{
		if (StateMachine.State != States.StrawbGet)
		{
			lastStrawb = strawb;
			StateMachine.State = States.StrawbGet;
			Position = strawb.Position + Vec3.UnitZ * -3;
			lastStrawb.Position = Position + Vec3.UnitZ * 12;
		}
	}

	#endregion

	#region FeatherStart State

	protected float tFeatherStart;

	protected virtual void StFeatherStartEnter()
	{
		tFeatherStart = FeatherStartTime;
	}

	protected virtual void StFeatherStartExit()
	{
	}

	protected virtual void StFeatherStartUpdate()
	{
		var input = RelativeMoveInput;
		if (input != Vec2.Zero)
			targetFacing = input.Normalized();

		SetHairColor(Skin.HairFeather);
		HandleFeatherZ();

		tFeatherStart -= Time.Delta;
		if (tFeatherStart <= 0)
		{
			StateMachine.State = States.Feather;
			return;
		}

		var velXY = velocity.XY();
		Calc.Approach(ref velXY, Vec2.Zero, 200 * Time.Delta);
		velocity = velocity.WithXY(velXY);

		// dashing
		if (dashes > 0 && tDashCooldown <= 0 && Controls.Dash.ConsumePress())
		{
			StateMachine.State = States.Dashing;
			dashes--;
			return;
		}
	}

	public virtual void FeatherGet(Feather feather)
	{
		Audio.Play(Sfx.sfx_dashcrystal, Position);
		World.HitStun = 0.05f;

		if (StateMachine.State == States.Feather)
		{
			tFeather = FeatherDuration;
			featherZ = feather.Position.Z - 2;
			Audio.Play(Sfx.sfx_feather_renew, Position);
		}
		else
		{
			StateMachine.State = States.FeatherStart;
			featherZ = feather.Position.Z - 2;
			dashes = Math.Max(dashes, 1);
			Audio.Play(Sfx.sfx_feather_get, Position);
		}
	}

	protected virtual void HandleFeatherZ()
		=> Calc.Approach(ref velocity.Z, (featherZ - Position.Z) * 40, 600 * Time.Delta);

	#endregion

	#region Feather State

	protected float featherZ;
	protected float tFeather;
	protected float tFeatherWallBumpCooldown;
	protected bool featherPlayedEndWarn = false;

	protected virtual void StFeatherEnter()
	{
		velocity = velocity.WithXY(targetFacing * FeatherStartSpeed);
		tFeather = FeatherDuration;
		Hair.Roundness = 1;
		drawModel = false;
		featherPlayedEndWarn = false;
		tFeatherWallBumpCooldown = 0;
		sfxFeather?.Resume();
	}

	protected virtual void StFeatherExit()
	{
		Hair.Roundness = 0;
		drawModel = true;
		sfxFeather?.Stop();
	}

	protected virtual void StFeatherUpdate()
	{
		const float EndWarningTime = 0.8f;

		if (tFeather > EndWarningTime || Time.BetweenInterval(.1f))
			SetHairColor(Skin.HairFeather);
		else if (dashes == 2)
			SetHairColor(Skin.HairTwoDash);
		else
			SetHairColor(Skin.HairNormal);

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
			StateMachine.State = States.Normal;

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
			StateMachine.State = States.Dashing;
			dashes--;
			return;
		}

		// start climbing
		if (Controls.Climb.Down && TryClimb())
		{
			StateMachine.State = States.Climbing;
			return;
		}
	}

	#endregion

	#region Respawn State

	protected virtual void StRespawnEnter()
	{
		drawModel = drawHair = false;
		drawOrbs = true;
		drawOrbsEase = 1;
		PointShadowAlpha = 0;
		Audio.Play(Sfx.sfx_revive, Position);
	}

	protected virtual void StRespawnUpdate()
	{
		drawOrbsEase -= Time.Delta * 2;
		if (drawOrbsEase <= 0)
			StateMachine.State = States.Normal;
	}

	protected virtual void StRespawnExit()
	{
		PointShadowAlpha = 1;
		drawModel = drawHair = true;
		drawOrbs = false;
	}

	#endregion

	#region B-Side Strawb Reveal

	// TODO: should maybe be a cutscene object? idk

	protected Actor? enterLookAt;

	protected virtual void StStrawbRevealEnter()
	{
	}

	protected virtual CoEnumerator StStrawbRevealRoutine()
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
				cameraOverride = new(Utils.Bezier(fromPos, control, toPos, Ease.Sine.In(p)), lookAt);
				yield return Co.SingleFrame;
			}

			for (float p = 0; p < 1.0f; p += Time.Delta / 1f)
			{
				GetCameraTarget(out var lookAtTo, out var posTo, out _);

				var t = Ease.Sine.Out(p);
				cameraOverride = new(Vec3.Lerp(toPos, posTo, t), Vec3.Lerp(lookAt, lookAtTo, t));
				yield return Co.SingleFrame;
			}

			yield return .02f;
		}

		StateMachine.State = States.Normal;
	}

	protected virtual void StStrawbRevealExit()
	{
		cameraOverride = null;
	}

	#endregion

	#region Dead State

	protected virtual void StDeadEnter()
	{
		drawModel = drawHair = false;
		drawOrbs = true;
		drawOrbsEase = 0;
		PointShadowAlpha = 0;
		Audio.Play(Sfx.sfx_death, Position);
	}

	protected virtual void StDeadUpdate()
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

	protected virtual void StCutsceneEnter()
	{
		Model.Play("Idle");
		// Fix white hair in cutscene bug
		if (tDashResetFlash > 0)
		{
			tDashResetFlash = 0;
			SetHairColor(CNormal);
		}
	}

	protected virtual void StCutsceneUpdate()
	{
		if (World.All<Cutscene>().Count == 0)
			StateMachine.State = States.Normal;
	}

	#endregion

	#region Bubble State

	protected Vec3 bubbleTo;

	public virtual void BubbleTo(Vec3 target)
	{
		bubbleTo = target;
		Model.Play("StrawberryGrab");
		StateMachine.State = States.Bubble;
		PointShadowAlpha = 0;
		Audio.Play(Sfx.sfx_bubble_in, Position);
	}

	protected virtual CoEnumerator StBubbleRoutine()
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
		StateMachine.State = States.Normal;
	}

	protected virtual void StBubbleExit()
	{
		Audio.Play(Sfx.sfx_bubble_out, Position);
		sfxBubble?.Stop();
		PointShadowAlpha = 1;
	}

	#endregion

	#region Cassette State

	protected Cassette? cassette;

	public virtual void EnterCassette(Cassette it)
	{
		if (StateMachine.State != States.Cassette)
		{
			cassette = it;
			StateMachine.State = States.Cassette;
			Position = it.Position - Vec3.UnitZ * 3;
			drawModel = drawHair = false;
			PointShadowAlpha = 0;
			cameraOverride = new(World.Camera.Position, it.Position);
			Game.Instance.Ambience.Stop();
			Audio.StopBus(Sfx.bus_gameplay_world, false);
			Audio.Play(Sfx.sfx_cassette_enter, Position);
		}
	}

	protected virtual CoEnumerator StCassetteRoutine()
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

		StateMachine.State = States.Normal;
		velocity = Vec3.UnitZ * 25;
		holdJumpSpeed = velocity.Z;
		tHoldJump = .1f;
		autoJump = true;

	}

	protected virtual void StCassetteExit()
	{
		cassette?.SetCooldown();
		cassette = null;
		drawModel = drawHair = true;
		cameraOverride = null;
		PointShadowAlpha = 1;
	}

	#endregion

	#region Graphics

	public virtual void CollectSprites(List<Sprite> populate)
	{
		// debug: draw camera origin pos
		if (World.DebugDraw)
		{
			populate.Add(Sprite.CreateBillboard(World, cameraOriginPos, "circle", 1, Color.Red));
		}

		// debug: draw wall up-normal
		if (World.DebugDraw)
		{
			if (StateMachine.State == States.Climbing)
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
			populate.Add(Sprite.CreateBillboard(World, Position + Forward * 4 + Vec3.UnitZ * 8, "gradient", 12, new Color(Skin.HairFeather) * 0.50f));
		}

		if (drawOrbs && drawOrbsEase > 0)
		{
			var ease = drawOrbsEase;
			var col = Math.Floor(ease * 10) % 2 == 0 ? Hair.Color : Color.White;
			var s = (ease < 0.5f) ? (0.5f + ease) : (Ease.Cube.Out(1 - (ease - 0.5f) * 2));
			for (int i = 0; i < 8; i ++)
			{
				var rot = (i / 8f + ease * 0.25f) * MathF.Tau;
				var rad = Ease.Cube.Out(ease) * 16;
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

	public virtual void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		if ((World.Camera.Position - (Position + Vec3.UnitZ * 8)).LengthSquared() > World.Camera.NearPlane * World.Camera.NearPlane)
		{
			if ((!Skin.HideHair || InFeatherState) && drawHair)
				populate.Add((this, Hair));

			if (drawModel)
				populate.Add((this, Model));
		}

		foreach (var trail in trails)
		{
			if (trail.Percent >= 1)
				continue;

			// I HATE this alpha fade out but don't have time to make some kind of full-model fade out effect
			var alpha = Ease.Cube.Out(Calc.ClampedMap(trail.Percent, 0.5f, 1.0f, 1, 0));

			foreach (var mat in trail.Model.Materials)
				mat.Color = trail.Color * alpha;
			trail.Hair.Color = trail.Color * alpha;

			if (Matrix.Invert(Matrix, out var inverse))
				trail.Model.Transform = trail.Transform * inverse;

			populate.Add((this, trail.Model));
			if (!Skin.HideHair || InFeatherState)
				populate.Add((this, trail.Hair));
		}
	}

	#endregion

	#region Platform Riding / Solid Checks

	public virtual void RidingPlatformSetVelocity(in Vec3 value)
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

	public virtual bool RidingPlatformCheck(Actor platform)
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

	public virtual void RidingPlatformMoved(in Vec3 delta)
	{
		var was = Position;
		SweepTestMove(delta, false);
		var newDelta = (Position - was);
		climbCornerFrom += newDelta;
		climbCornerTo += newDelta;
	}

	public virtual bool GroundCheck(out Vec3 pushout, out Vec3 normal, out Actor? floor)
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

	public virtual bool CeilingCheck(out Vec3 pushout)
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

	public virtual void Stop() => velocity = Vec3.Zero;

	#endregion
}
