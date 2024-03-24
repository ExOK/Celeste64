using Celeste64.Mod;
using System.Collections.ObjectModel;

namespace Celeste64;

/// <summary>
/// Welcome to the monolithic player class! This time only 2300 lines ;)
/// </summary>
public class Player : Actor, IHaveModels, IHaveSprites, IRidePlatforms, ICastPointShadow
{
	#region Default Movement Properties
	// These default movement properties have been broken out from the actual movement properties
	// So we can always know what they were set at for their default values, in case we need to reset them.
	public virtual float DefaultAcceleration => 500;
	public virtual float DefaultPastMaxDeccel => 60;
	public virtual float DefaultAirAccelMultMin => .5f;
	public virtual float DefaultAirAccelMultMax => 1f;
	public virtual float DefaultMaxSpeed => 64;
	public virtual float DefaultRotateSpeed => MathF.Tau * 1.5f;
	public virtual float DefaultRotateSpeedAboveMax => MathF.Tau * .6f;
	public virtual float DefaultFriction => 800;
	public virtual float DefaultAirFrictionMult => .1f;
	public virtual float DefaultGravity => 600;
	public virtual float DefaultMaxFall => -120;
	public virtual float DefaultHalfGravThreshold => 100;
	public virtual float DefaultJumpHoldTime => .1f;
	public virtual float DefaultJumpSpeed => 90;
	public virtual float DefaultJumpXYBoost => 10;
	public virtual float DefaultCoyoteTime => .12f;

	public virtual float DefaultDashSpeed => 140;
	public virtual float DefaultDashEndSpeedMult => .75f;
	public virtual float DefaultDashTime => .2f;
	public virtual float DefaultDashResetCooldown => .2f;
	public virtual float DefaultDashCooldown => .1f;
	public virtual float DefaultDashRotateSpeed => MathF.Tau * .3f;

	public virtual float DefaultDashJumpSpeed => 40;
	public virtual float DefaultDashJumpHoldSpeed => 20;
	public virtual float DefaultDashJumpHoldTime => .3f;
	public virtual float DefaultDashJumpXYBoost => 16;

	public virtual float DefaultSkidDotThreshold => -.7f;
	public virtual float DefaultSkiddingStartAccel => 300;
	public virtual float DefaultSkiddingAccel => 500;
	public virtual float DefaultSkidJumpSpeed => 120;
	public virtual float DefaultSkidJumpHoldTime => .16f;

	public virtual float DefaultWallPushoutDist => 3;
	public virtual float DefaultClimbCheckDist => 4;
	public virtual float DefaultClimbSpeed => 40;
	public virtual float DefaultClimbHopUpSpeed => 80;
	public virtual float DefaultClimbHopForwardSpeed => 40;
	public virtual float DefaultClimbHopNoMoveTime => .25f;

	public virtual float DefaultSpringJumpSpeed => 160;
	public virtual float DefaultSpringJumpHoldTime => .3f;

	public virtual float DefaultFeatherStartTime => .4f;
	public virtual float DefaultFeatherFlySpeed => 100;
	public virtual float DefaultFeatherStartSpeed => 140;
	public virtual float DefaultFeatherTurnSpeed => MathF.Tau * .75f;
	public virtual float DefaultFeatherAccel => 60;
	public virtual float DefaultFeatherDuration => 2.2f;
	public virtual float DefaultFeatherExitXYMult => .5f;
	public virtual float DefaultFeatherExitZSpeed => 60;

	#endregion

	#region Movement Properties

	public virtual float Acceleration { get; set; }
	public virtual float PastMaxDeccel { get; set; }
	public virtual float AirAccelMultMin { get; set; }
	public virtual float AirAccelMultMax { get; set; }
	public virtual float MaxSpeed { get; set; }
	public virtual float RotateThreshold => MaxSpeed * .2f;
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
	public virtual float WallJumpXYSpeed => MaxSpeed * 1.3f;

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
	public virtual float EndSkidSpeed => MaxSpeed * 0.8f;
	public virtual float SkidJumpSpeed { get; set; }
	public virtual float SkidJumpHoldTime { get; set; }
	public virtual float SkidJumpXYSpeed => MaxSpeed * 1.4f;

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

	#endregion

	// Resets all the movement properties to their default values.
	public void ResetDefaultValues()
	{
		Acceleration = DefaultAcceleration;
		PastMaxDeccel = DefaultPastMaxDeccel;
		AirAccelMultMin = DefaultAirAccelMultMin;
		AirAccelMultMax = DefaultAirAccelMultMax;
		MaxSpeed = DefaultMaxSpeed;
		RotateSpeed = DefaultRotateSpeed;
		RotateSpeedAboveMax = DefaultRotateSpeedAboveMax;
		Friction = DefaultFriction;
		AirFrictionMult = DefaultAirFrictionMult;
		Gravity = DefaultGravity;
		MaxFall = DefaultMaxFall;
		HalfGravThreshold = DefaultHalfGravThreshold;
		JumpHoldTime = DefaultJumpHoldTime;
		JumpSpeed = DefaultJumpSpeed;
		JumpXYBoost = DefaultJumpXYBoost;
		CoyoteTime = DefaultCoyoteTime;

		DashSpeed = DefaultDashSpeed;
		DashEndSpeedMult = DefaultDashEndSpeedMult;
		DashTime = DefaultDashTime;
		DashResetCooldown = DefaultDashResetCooldown;
		DashCooldown = DefaultDashCooldown;
		DashRotateSpeed = DefaultDashRotateSpeed;

		DashJumpSpeed = DefaultDashJumpSpeed;
		DashJumpHoldSpeed = DefaultDashJumpHoldSpeed;
		DashJumpHoldTime = DefaultDashJumpHoldTime;
		DashJumpXYBoost = DefaultDashJumpXYBoost;

		SkidDotThreshold = DefaultSkidDotThreshold;
		SkiddingStartAccel = DefaultSkiddingStartAccel;
		SkiddingAccel = DefaultSkiddingAccel;
		SkidJumpSpeed = DefaultSkidJumpSpeed;
		SkidJumpHoldTime = DefaultSkidJumpHoldTime;

		WallPushoutDist = DefaultWallPushoutDist;
		ClimbCheckDist = DefaultClimbCheckDist;
		ClimbSpeed = DefaultClimbSpeed;
		ClimbHopUpSpeed = DefaultClimbHopUpSpeed;
		ClimbHopForwardSpeed = DefaultClimbHopForwardSpeed;
		ClimbHopNoMoveTime = DefaultClimbHopNoMoveTime;

		SpringJumpSpeed = DefaultSpringJumpSpeed;
		SpringJumpHoldTime = DefaultSpringJumpHoldTime;

		FeatherStartTime = DefaultFeatherStartTime;
		FeatherFlySpeed = DefaultFeatherFlySpeed;
		FeatherStartSpeed = DefaultFeatherStartSpeed;
		FeatherTurnSpeed = DefaultFeatherTurnSpeed;
		FeatherAccel = DefaultFeatherAccel;
		FeatherDuration = DefaultFeatherDuration;
		FeatherExitXYMult = DefaultFeatherExitXYMult;
	}

	// These are no longer used. This gets populated from SkinInfo.
	public static readonly Color CNormal = 0xdb2c00;
	public static readonly Color CNoDash = 0x6ec0ff;
	public static readonly Color CTwoDashes = 0xfa91ff;
	public static readonly Color CRefillFlash = Color.White;
	public static readonly Color CFeather = 0xf2d450;

	#region SubClasses

	public class Trail
	{
		public readonly Hair Hair;
		public readonly SkinnedModel Model;
		public Matrix Transform;
		public float Percent;
		public Color Color;

		public Trail(string model = "player")
		{
			Model = new(Assets.Models[model]) { Flags = ModelFlags.Transparent };
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
	public static Vec3 StoredCameraForward;
	public static float StoredCameraDistance;

	public enum States { Normal, Dashing, Skidding, Climbing, StrawbGet, FeatherStart, Feather, Respawn, Dead, StrawbReveal, Cutscene, Bubble, Cassette, DebugFly };
	public enum Events { Land };
	public enum JumpType { Jumped, WallJumped, SkidJumped, DashJumped };

	public bool Dead = false;

	public Vec3 ModelScale = Vec3.One;
	public SkinnedModel Model;
	public readonly Hair Hair = new();
	public virtual float PointShadowAlpha { get; set; } = 1.0f;

	public SkinInfo Skin;

	protected Vec3 velocity;
	protected Vec3 previousVelocity;

	public virtual Vec3 Velocity { get => velocity; set => velocity = value; }
	public virtual Vec3 PreviousVelocity { get => previousVelocity; set => previousVelocity = value; }
	public Vec3 GroundNormal;
	public Vec3 PlatformVelocity;
	public float TPlatformVelocityStorage;
	public float TGroundSnapCooldown;
	public Actor? ClimbingWallActor;
	public Vec3 ClimbingWallNormal;

	public bool OnGround;
	public Vec2 TargetFacing = Vec2.UnitY;
	public Vec3 CameraTargetForward = new(0, 1, 0);
	public float CameraTargetDistance = 0.50f;
	public readonly StateMachine<States, Events> StateMachine;

	public record struct CameraOverrideStruct(Vec3 Position, Vec3 LookAt);
	public CameraOverrideStruct? CameraOverride = null;
	public Vec3 CameraOriginPos;
	public Vec3 CameraDestinationPos;

	public float TCoyote;
	public float CoyoteZ;

	public bool DrawModel = true;
	public bool DrawHair = true;
	public bool DrawOrbs = false;
	public float DrawOrbsEase = 0;

	public readonly List<Trail> Trails = [];
	public readonly Func<SpikeBlock, bool> SpikeBlockCheck;
	public Color LastDashHairColor;

	public Sound? SfxWallSlide;
	public Sound? SfxFeather;
	protected Sound? SfxBubble;

	public virtual Vec3 SolidWaistTestPos
		=> Position + Vec3.UnitZ * 3;
	public virtual Vec3 SolidHeadTestPos
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

	protected List<StatusEffect> statusEffects { get; } = new List<StatusEffect>();

	public ReadOnlyCollection<StatusEffect> StatusEffects => statusEffects.AsReadOnly();

	public Player()
	{
		ResetDefaultValues();
		PointShadowAlpha = 1.0f;
		LocalBounds = new BoundingBox(new Vec3(0, 0, 10), 10);
		UpdateOffScreen = true;
		Skin = Save.GetSkin();

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

		Skin.OnEquipped(this, Model);

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
		StateMachine.InitState(States.DebugFly, StDebugFlyUpdate, StDebugFlyEnter, StDebugFlyExit);
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

		SpikeBlockCheck = spike => Vec3.Dot(velocity.Normalized(), spike.Direction) < 0.5f;

		SetHairColor(0xdb2c00);
	}

	/// <summary>
	/// If the player is in a custom state, returns its definition.
	/// Otherwise, returns null.
	/// </summary>
	[DisallowHooks]
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
	[DisallowHooks]
	public virtual bool IsInState<T>() where T : CustomPlayerState
	{
		var stateDef = GetCurrentCustomState();

		return stateDef is T;
	}

	/// <summary>
	/// Sets the player's state to the provided custom state.
	/// </summary>
	[DisallowHooks]
	public virtual void SetState<T>() where T : CustomPlayerState
	{
		var id = CustomPlayerStateRegistry.GetId<T>();

		StateMachine.State = id;
	}

	/// <summary>
	/// Sets the player's state to the provided vanilla state.
	/// </summary>
	[DisallowHooks]
	public virtual void SetState(States state)
	{
		StateMachine.State = state;
	}

	[DisallowHooks]
	protected virtual void HandleStateChange(States? state)
	{
		ModManager.Instance.OnPlayerStateChanged(this, state);
	}

	[DisallowHooks]
	public StatusEffect AddStatusEffect<T>(bool RemoveAfterDuration = false, float DurationOverride = 10) where T : StatusEffect, new()
	{
		var existingEffect = GetStatusEffect<T>();
		if (existingEffect is { RemoveOnReapply: false })
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

	[DisallowHooks]
	public void RemoveStatusEffect<T>() where T : StatusEffect
	{
		var existingEffect = GetStatusEffect<T>();
		if (existingEffect != null)
		{
			existingEffect.OnStatusEffectRemoved();

			statusEffects.Remove(existingEffect);
		}
	}

	[DisallowHooks]
	public void RemoveStatusEffect(StatusEffect effect)
	{
		effect.OnStatusEffectRemoved();
		statusEffects.Remove(effect);
	}

	[DisallowHooks]
	public bool HasStatusEffect<T>() where T : StatusEffect
	{
		return statusEffects.Any(effect => effect.GetType() == typeof(T));
	}

	[DisallowHooks]
	public StatusEffect? GetStatusEffect<T>() where T : StatusEffect
	{
		return statusEffects.FirstOrDefault(effect => effect.GetType() == typeof(T));
	}

	#region Added / Update

	public override void Added()
	{
		if (World.Entry.Reason == World.EntryReasons.Respawned)
		{
			CameraTargetForward = StoredCameraForward;
			CameraTargetDistance = StoredCameraDistance;
			StateMachine.State = States.Respawn;
		}
		else if (World.Entry is { Submap: true, Reason: World.EntryReasons.Entered })
		{
			StateMachine.State = States.StrawbReveal;
		}
		else
		{
			StateMachine.State = States.Normal;
		}

		SfxWallSlide = World.Add(new Sound(this, Sfx.sfx_wall_slide));
		SfxFeather = World.Add(new Sound(this, Sfx.sfx_feather_state_active_loop));
		SfxBubble = World.Add(new Sound(this, Sfx.sfx_bubble_loop));

		CameraOriginPos = Position;
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
				var invertX = Settings.InvertCamera == InvertCameraOptions.X || Settings.InvertCamera == InvertCameraOptions.Both;
				var rot = new Vec2(CameraTargetForward.X, CameraTargetForward.Y).Angle();
				rot -= Controls.Camera.Value.X * Time.Delta * 4 * (invertX ? -1 : 1);

				var angle = Calc.AngleToVector(rot);
				CameraTargetForward = new(angle, 0);
			}

			// Move Camera in / out
			if (Controls.Camera.Value.Y != 0)
			{
				var invertY = Settings.InvertCamera == InvertCameraOptions.Y || Settings.InvertCamera == InvertCameraOptions.Both;
				CameraTargetDistance += Controls.Camera.Value.Y * Time.Delta * (invertY ? -1 : 1);
				CameraTargetDistance = Calc.Clamp(CameraTargetDistance, 0, 1);
			}
			else
			{
				const float interval = 1f / 3;
				const float threshold = .1f;
				if (CameraTargetDistance % interval < threshold || CameraTargetDistance % interval > interval - threshold)
					Calc.Approach(ref CameraTargetDistance, Calc.Snap(CameraTargetDistance, interval), Time.Delta / 2);
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
		if (!InBubble && StateMachine.State != States.DebugFly)
		{
			if (Position.Z < World.DeathPlane ||
				World.Overlaps<DeathBlock>(SolidWaistTestPos) ||
				World.Overlaps<SpikeBlock>(SolidWaistTestPos, SpikeBlockCheck))
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
			if (TCoyote > 0)
				TCoyote -= Time.Delta;
			if (THoldJump > 0)
				THoldJump -= Time.Delta;
			if (TDashCooldown > 0)
				TDashCooldown -= Time.Delta;
			if (TDashResetCooldown > 0)
				TDashResetCooldown -= Time.Delta;
			if (TDashResetFlash > 0)
				TDashResetFlash -= Time.Delta;
			if (TNoMove > 0)
				TNoMove -= Time.Delta;
			if (TPlatformVelocityStorage > 0)
				TPlatformVelocityStorage -= Time.Delta;
			if (TGroundSnapCooldown > 0)
				TGroundSnapCooldown -= Time.Delta;
			if (TClimbCooldown > 0)
				TClimbCooldown -= Time.Delta;
		}

		previousVelocity = velocity;
		StateMachine.Update();

		// move and pop out
		if (!InBubble && StateMachine.State != States.DebugFly)
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
				SweepTestMove(amount, TNoMove <= 0);
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
		if (StateMachine.State != States.DebugFly)
		{
			bool prevOnGround = OnGround;
			OnGround = GroundCheck(out var pushout, out var normal, out _);
			if (OnGround)
				Position += pushout;

			if (TGroundSnapCooldown <= 0 && prevOnGround && !OnGround)
			{
				// try to ground snap?
				if (World.SolidRayCast(Position, -Vec3.UnitZ, 5, out var hit) && FloorNormalCheck(hit.Normal))
				{
					Position = hit.Point;
					OnGround = GroundCheck(out _, out normal, out _);
				}
			}

			if (OnGround)
			{
				AutoJump = false;
				GroundNormal = normal;
				TCoyote = CoyoteTime;
				CoyoteZ = Position.Z;
				if (TDashResetCooldown <= 0)
					RefillDash();
			}
			else
				GroundNormal = Vec3.UnitZ;

			if (!prevOnGround && OnGround)
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
						var vel = (TPlatformVelocityStorage > 0 ? PlatformVelocity : Vec3.Zero) + new Vec3(angle, 0) * 50;
						World.Request<Dust>().Init(at, vel);
					}
				}
			}
		}

		// update camera origin position
		{
			float ZPad = StateMachine.State == States.Climbing ? 0 : 8;
			CameraOriginPos.X = Position.X;
			CameraOriginPos.Y = Position.Y;

			float targetZ;
			if (OnGround)
				targetZ = Position.Z;
			else if (Position.Z < CameraOriginPos.Z)
				targetZ = Position.Z;
			else if (Position.Z > CameraOriginPos.Z + ZPad)
				targetZ = Position.Z - ZPad;
			else
				targetZ = CameraOriginPos.Z;

			if (CameraOriginPos.Z != targetZ)
				CameraOriginPos.Z += (targetZ - CameraOriginPos.Z) * (1 - MathF.Pow(.001f, Time.Delta));
		}

		// update camera position
		{
			Vec3 lookAt, cameraPos;

			if (CameraOverride.HasValue)
			{
				lookAt = CameraOverride.Value.LookAt;
				cameraPos = CameraOverride.Value.Position;
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

			Facing = Calc.AngleToVector(Calc.AngleApproach(Facing.Angle(), TargetFacing.Angle(), MathF.Tau * 2 * Time.Delta));
			Model.Update();
			Model.Transform = Matrix.CreateScale(ModelScale * 3);

			if (StateMachine.State != States.Feather && StateMachine.State != States.FeatherStart
				&& GetCurrentCustomState() is not { ControlHairColor: true })
			{
				Color color;
				if (TDashResetFlash > 0)
					color = Skin.HairRefillFlash;
				else if (DashesLocal == 1)
					color = Skin.HairNormal;
				else if (DashesLocal == 0)
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
			Hair.Grounded = OnGround;
			Hair.Update(hairMatrix);
		}

		// trails
		for (int i = Trails.Count - 1; i >= 0; i--)
		{
			if (Trails[i].Percent < 1)
				Trails[i].Percent += Time.Delta / 0.5f;
		}
	}

	#endregion

	#region Camera Calculation

	public virtual void GetCameraTarget(out Vec3 cameraLookAt, out Vec3 cameraPosition, out bool snapRequested)
	{
		snapRequested = false;

		// get default values
		cameraLookAt = CameraOriginPos;
		cameraPosition = cameraLookAt
			- CameraTargetForward * Utils.Lerp3(30, 60, 110, 110, CameraTargetDistance)
			+ Vec3.UnitZ * Utils.Lerp3(1, 30, 80, 180, CameraTargetDistance);
		cameraLookAt += Vec3.UnitZ * 12;

		// inside a fixed camera zone
		if (World.OverlapsFirst<FixedCamera>(SolidWaistTestPos) is { } fixedCamera
		&& (cameraLookAt - fixedCamera.Position).Length() > 5)
		{
			cameraPosition = fixedCamera.Point;
			CameraTargetForward = new Vec3((cameraLookAt.XY() - cameraPosition.XY()).Normalized(), 0);
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
		if (this.Skin != skin)
		{
			this.Skin.OnRemoved(this, Model);
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

			Trails.Clear();
			DashTrailsCreated = 0;

			skin.OnEquipped(this, Model);
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
			if (cameraForward is { X: 0, Y: 0 })
				forward = TargetFacing;
			else
				forward = cameraForward.Normalized();
			side = Vec2.Transform(forward, Matrix3x2.CreateRotation(MathF.PI / 2));

			var input = -Controls.Move.Value.Normalized();
			if (Vec2.Dot(input, Vec2.UnitY) >= .985f)
				input = Vec2.UnitY;

			return forward * input.Y + side * input.X;
		}
	}

	public virtual void SetTargetFacing(Vec2 facing)
	{
		TargetFacing = facing;
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
		Hair.Nodes = (InFeatherState ? 18 : (DashesLocal >= 2 ? 16 : 10));
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
			if (resolveImpact && StateMachine.State == States.Feather && TFeatherWallBumpCooldown <= 0 && !(Controls.Climb.Down && TryClimb()))
			{
				Position += hit.Pushout;
				velocity = velocity.WithXY(Vec2.Reflect(velocity.XY(), hit.Normal.XY().Normalized()));
				TFeatherWallBumpCooldown = 0.50f;
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
		TGroundSnapCooldown = 0.1f;

	public virtual void Jump()
	{
		Position = Position with { Z = CoyoteZ };
		HoldJumpSpeed = velocity.Z = JumpSpeed;
		THoldJump = JumpHoldTime;
		TCoyote = 0;
		AutoJump = false;

		var input = RelativeMoveInput;
		if (input != Vec2.Zero)
		{
			input = input.Normalized();
			TargetFacing = input;
			velocity += new Vec3(input * JumpXYBoost, 0);
		}

		AddPlatformVelocity(true);
		CancelGroundSnap();

		ModelScale = new(.6f, .6f, 1.4f);
		Audio.Play(Sfx.sfx_jump, Position);
		ModManager.Instance.OnPlayerJumped(this, JumpType.Jumped);
	}

	public virtual void WallJump()
	{
		HoldJumpSpeed = velocity.Z = JumpSpeed;
		THoldJump = JumpHoldTime;
		AutoJump = false;

		var velXY = TargetFacing * WallJumpXYSpeed;
		velocity = velocity.WithXY(velXY);

		AddPlatformVelocity(false);
		CancelGroundSnap();

		ModelScale = new(.6f, .6f, 1.4f);
		Audio.Play(Sfx.sfx_jump_wall, Position);
		ModManager.Instance.OnPlayerJumped(this, JumpType.WallJumped);
	}

	public virtual void SkidJump()
	{
		Position = Position with { Z = CoyoteZ };
		HoldJumpSpeed = velocity.Z = SkidJumpSpeed;
		THoldJump = SkidJumpHoldTime;
		TCoyote = 0;

		var velXY = TargetFacing * SkidJumpXYSpeed;
		velocity = velocity.WithXY(velXY);

		AddPlatformVelocity(false);
		CancelGroundSnap();

		for (int i = 0; i < 16; i++)
		{
			var dir = new Vec3(Calc.AngleToVector((i / 16f) * MathF.Tau), 0);
			World.Request<Dust>().Init(Position + dir * 8, new Vec3(velocity.XY() * 0.5f, 10) - dir * 50, 0x666666);
		}

		ModelScale = new(.6f, .6f, 1.4f);
		Audio.Play(Sfx.sfx_jump, Position);
		Audio.Play(Sfx.sfx_jump_skid, Position);
		ModManager.Instance.OnPlayerJumped(this, JumpType.SkidJumped);
	}

	public virtual void DashJump()
	{
		Position = Position with { Z = CoyoteZ };
		velocity.Z = DashJumpSpeed;
		HoldJumpSpeed = DashJumpHoldSpeed;
		THoldJump = DashJumpHoldTime;
		TCoyote = 0;
		AutoJump = false;
		DashesLocal = 1;

		if (DashJumpXYBoost != 0)
		{
			var input = RelativeMoveInput;
			if (input != Vec2.Zero)
			{
				input = input.Normalized();
				TargetFacing = input;
				velocity += new Vec3(input * DashJumpXYBoost, 0);
			}
		}

		AddPlatformVelocity(false);
		CancelGroundSnap();

		ModelScale = new(.6f, .6f, 1.4f);
		Audio.Play(Sfx.sfx_jump, Position);
		Audio.Play(Sfx.sfx_jump_superslide, Position);
		ModManager.Instance.OnPlayerJumped(this, JumpType.DashJumped);
	}

	public virtual void AddPlatformVelocity(bool playSound)
	{
		if (TPlatformVelocityStorage > 0)
		{
			var add = PlatformVelocity;

			add.Z = Calc.Clamp(add.Z, 0, 180);
			if (add.XY().LengthSquared() > 300 * 300)
				add = add.WithXY(add.XY().Normalized() * 300);

			velocity += add;
			PlatformVelocity = Vec3.Zero;
			TPlatformVelocityStorage = 0;

			if (playSound && (add.Z >= 10 || add.XY().Length() > 10))
				Audio.Play(Sfx.sfx_jump_assisted, Position);
		}
	}

	public virtual void Kill()
	{
		StateMachine.State = States.Dead;
		StoredCameraForward = CameraTargetForward;
		StoredCameraDistance = CameraTargetDistance;
		Save.CurrentRecord.Deaths++;
		Dead = true;
		ModManager.Instance.OnPlayerKill(this);
		foreach (var statusEffect in statusEffects.ToList())
		{
			statusEffect.Update(Time.Delta);
			statusEffect.UpdateDuration(Time.Delta);
		}
	}

	public virtual bool ClimbCheckAt(Vec3 offset, out WallHit hit)
	{
		if (World.SolidWallCheckClosestToNormal(SolidWaistTestPos + offset, ClimbCheckDist, -new Vec3(TargetFacing, 0), out hit)
		&& (RelativeMoveInput == Vec2.Zero || Vec2.Dot(hit.Normal.XY().Normalized(), RelativeMoveInput) <= -0.5f)
		&& (hit.Actor is not Solid || hit.Actor is Solid { IsClimbable: true }) && ClimbNormalCheck(hit.Normal)
		&& World.SolidRayCast(SolidWaistTestPos, -hit.Normal, ClimbCheckDist + 2, out var rayHit) && ClimbNormalCheck(rayHit.Normal)
		&& (rayHit.Actor is not Solid || rayHit.Actor is Solid { IsClimbable: true }))
			return true;
		return false;
	}

	public virtual bool TryClimb()
	{
		var result = ClimbCheckAt(Vec3.Zero, out var wall);

		// let us snap up to walls if we're jumping for them
		// note: if vel.z is allowed to be downwards then we awkwardly re-grab when sliding off
		// the bottoms of walls, which is really bad feeling
		if (!result && velocity.Z > 0 && !OnGround && StateMachine.State != States.Climbing)
			result = ClimbCheckAt(Vec3.UnitZ * 4, out wall);

		if (result)
		{
			ClimbingWallNormal = wall.Normal;
			ClimbingWallActor = wall.Actor;
			var moveTo = wall.Point + (Position - SolidWaistTestPos) + wall.Normal * WallPushoutDist;
			SweepTestMove(moveTo - Position, false);
			TargetFacing = -ClimbingWallNormal.XY().Normalized();
			return true;
		}
		else
		{
			ClimbingWallActor = default;
			ClimbingWallNormal = default;
			return false;
		}
	}

	public virtual bool ClimbNormalCheck(in Vec3 normal)
	{
		return MathF.Abs(normal.Z) < 0.35f;
	}

	public virtual bool FloorNormalCheck(in Vec3 normal)
		=> !ClimbNormalCheck(normal) && normal.Z > 0;

	public virtual bool WallJumpCheck()
	{
		if (Controls.Jump.Pressed
		&& World.SolidWallCheckClosestToNormal(SolidWaistTestPos, ClimbCheckDist, -new Vec3(TargetFacing, 0), out var hit) && hit.Actor is Solid { CanWallJump: true })
		{
			Controls.Jump.ConsumePress();
			Position += (hit.Pushout * (WallPushoutDist / ClimbCheckDist));
			TargetFacing = hit.Normal.XY().Normalized();
			return true;
		}
		else
			return false;
	}

	public virtual void Spring(Spring spring)
	{
		StateMachine.State = States.Normal;

		Position = Position with { Z = Calc.Approach(Position.Z, spring.Position.Z + 3, 2) };

		var posXY = Position.XY();
		Calc.Approach(ref posXY, spring.Position.XY(), 4);
		Position = Position.WithXY(posXY);

		HoldJumpSpeed = velocity.Z = SpringJumpSpeed;
		THoldJump = SpringJumpHoldTime;
		TCoyote = 0;
		AutoJump = true;

		var velXY = velocity.XY();
		Calc.Approach(ref velXY, Vec2.Zero, 30);
		velocity = velocity.WithXY(velXY);

		DashesLocal = Math.Max(DashesLocal, 1);
		CancelGroundSnap();
	}

	#endregion

	#region Normal State

	public virtual float FootstepInterval => 0.3f;

	public float THoldJump;
	public float HoldJumpSpeed;
	public bool AutoJump;
	public float TNoMove;
	public float TFootstep;

	public virtual void StNormalEnter()
	{
		THoldJump = 0;
		TFootstep = FootstepInterval;
	}

	public virtual void StNormalExit()
	{
		THoldJump = 0;
		TNoMove = 0;
		AutoJump = false;
		Model.Rate = 1;
	}

	public virtual void StNormalUpdate()
	{
		// Check for NPC interaction
		if (OnGround)
		{
			foreach (var actor in World.All<NPC>())
				if (actor is NPC { InteractEnabled: true } npc)
				{
					if ((Position - npc.Position).LengthSquared() < npc.InteractRadius * npc.InteractRadius &&
						Vec2.Dot((npc.Position - Position).XY(), TargetFacing) > 0 &&
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

			if (Controls.Move.Value == Vec2.Zero || TNoMove > 0)
			{
				// if not moving, simply apply friction

				float fric = Friction;
				if (!OnGround)
					fric *= AirFrictionMult;

				// friction
				Calc.Approach(ref velXY, Vec2.Zero, fric * Time.Delta);
			}
			else if (OnGround)
			{
				float max = MaxSpeed;

				// change max speed based on ground slope angle
				if (GroundNormal != Vec3.UnitZ)
				{
					float slopeDot = 1 - Calc.Clamp(Vec3.Dot(GroundNormal, Vec3.UnitZ), 0, 1);
					slopeDot *= Vec2.Dot(GroundNormal.XY().Normalized(), TargetFacing) * 2;
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
						TargetFacing = input;
						Facing = TargetFacing = input;
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

						TargetFacing = Calc.RotateToward(TargetFacing, input, rotate * Time.Delta, 0);
						velXY = TargetFacing * Calc.Approach(velXY.Length(), max, accel * Time.Delta);
					}
				}
				else
				{
					// if we're below the RotateThreshold, acceleration is very simple
					Calc.Approach(ref velXY, input * max, accel * Time.Delta);

					TargetFacing = input.Normalized();
				}
			}
			else
			{
				float accel;
				if (velXY.LengthSquared() >= MaxSpeed * MaxSpeed && Vec2.Dot(RelativeMoveInput.Normalized(), velXY.Normalized()) >= .7f)
				{
					accel = PastMaxDeccel;

					var dot = Vec2.Dot(RelativeMoveInput.Normalized(), TargetFacing);
					accel *= Calc.ClampedMap(dot, -1, 1, AirAccelMultMax, AirAccelMultMin);
				}
				else
				{
					accel = Acceleration;

					var dot = Vec2.Dot(RelativeMoveInput.Normalized(), TargetFacing);
					accel *= Calc.ClampedMap(dot, -1, 1, AirAccelMultMin, AirAccelMultMax);
				}

				Calc.Approach(ref velXY, RelativeMoveInput * MaxSpeed, accel * Time.Delta);
			}

			velocity = velocity.WithXY(velXY);
		}

		// Footstep sounds
		if (OnGround && velocity.XY().Length() > 10)
		{
			TFootstep -= Time.Delta * Model.Rate;
			if (TFootstep <= 0)
			{
				TFootstep = FootstepInterval;
				Audio.Play(Sfx.sfx_footstep_general, Position);
			}

			if (Time.OnInterval(0.05f))
			{
				var at = Position + new Vec3(World.Rng.Float(-3, 3), World.Rng.Float(-3, 3), 0);
				var vel = TPlatformVelocityStorage > 0 ? PlatformVelocity : Vec3.Zero;
				World.Request<Dust>().Init(at, vel);
			}
		}
		else
			TFootstep = FootstepInterval;

		// start climbing
		if (Controls.Climb.Down && TClimbCooldown <= 0 && TryClimb())
		{
			StateMachine.State = States.Climbing;
			return;
		}

		// dashing
		if (TryDash())
			return;

		// jump & gravity
		if (TCoyote > 0 && Controls.Jump.ConsumePress())
			Jump();
		else if (WallJumpCheck())
			WallJump();
		else
		{
			if (THoldJump > 0 && (AutoJump || Controls.Jump.Down))
			{
				if (velocity.Z < HoldJumpSpeed)
					velocity.Z = HoldJumpSpeed;
			}
			else
			{
				float mult;
				if ((Controls.Jump.Down || AutoJump) && MathF.Abs(velocity.Z) < HalfGravThreshold)
					mult = .5f;
				else
				{
					mult = 1;
					AutoJump = false;
				}

				Calc.Approach(ref velocity.Z, MaxFall, Gravity * mult * Time.Delta);
				THoldJump = 0;

			}
		}

		// Update Model Animations
		if (OnGround)
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

	public virtual int Dashes => DashesLocal;
	public int DashesLocal = 1;
	public float TDash;
	public float TDashCooldown;
	public float TDashResetCooldown;
	public float TDashResetFlash;
	public float TNoDashJump;
	public bool DashedOnGround;
	public int DashTrailsCreated;

	public virtual bool TryDash()
	{
		if (DashesLocal > 0 && TDashCooldown <= 0 && Controls.Dash.ConsumePress())
		{
			DashesLocal--;
			StateMachine.State = States.Dashing;
			return true;
		}
		else return false;
	}

	public virtual void StDashingEnter()
	{
		if (RelativeMoveInput != Vec2.Zero)
			TargetFacing = RelativeMoveInput;
		Facing = TargetFacing;

		LastDashHairColor = DashesLocal <= 0 ? Skin.HairNoDash : Skin.HairNormal;
		DashedOnGround = OnGround;
		SetDashSpeed(TargetFacing);
		AutoJump = true;

		TDash = DashTime;
		TDashResetCooldown = DashResetCooldown;
		TNoDashJump = .1f;
		DashTrailsCreated = 0;

		World.HitStun = .02f;

		if (DashesLocal <= 1)
			Audio.Play(Sfx.sfx_dash_red, Position);
		else
			Audio.Play(Sfx.sfx_dash_pink, Position);

		//CancelGroundSnap();
	}

	public virtual void StDashingExit()
	{
		TDashCooldown = DashCooldown;
		CreateDashtTrail();
	}

	public virtual void StDashingUpdate()
	{
		Model.Play("Dash");

		TDash -= Time.Delta;
		if (TDash <= 0)
		{
			if (!OnGround)
				velocity *= DashEndSpeedMult;
			StateMachine.State = States.Normal;
			return;
		}

		if (DashTrailsCreated <= 0 || (DashTrailsCreated == 1 && TDash <= DashTime * .5f))
		{
			DashTrailsCreated++;
			CreateDashtTrail();
		}

		if (Controls.Move.Value != Vec2.Zero && Vec2.Dot(Controls.Move.Value, TargetFacing) >= -.2f)
		{
			TargetFacing = Calc.RotateToward(TargetFacing, RelativeMoveInput, DashRotateSpeed * Time.Delta, 0);
			SetDashSpeed(TargetFacing);
		}

		if (TNoDashJump > 0)
			TNoDashJump -= Time.Delta;

		// dash jump
		if (DashedOnGround && TCoyote > 0 && TNoDashJump <= 0 && Controls.Jump.ConsumePress())
		{
			StateMachine.State = States.Normal;
			DashJump();
			return;
		}
	}

	public virtual void CreateDashtTrail()
	{
		Trail? trail = null;
		foreach (var it in Trails)
			if (it.Percent >= 1)
			{
				trail = it;
				break;
			}
		if (trail == null)
			Trails.Add(trail = new(Skin.Model));

		trail.Model.SetBlendedWeights(Model.GetBlendedWeights());
		trail.Hair.CopyState(Hair);
		trail.Percent = 0.0f;
		trail.Transform = Model.Transform * Matrix;
		trail.Color = LastDashHairColor;
	}

	public virtual bool RefillDash(int amount = 1)
	{
		if (DashesLocal < amount)
		{
			DashesLocal = amount;
			TDashResetFlash = .05f;
			return true;
		}
		else
			return false;
	}

	public virtual void SetDashSpeed(in Vec2 dir)
	{
		if (DashedOnGround)
			velocity = new Vec3(dir, 0) * DashSpeed;
		else
			velocity = new Vec3(dir, .4f).Normalized() * DashSpeed;

	}

	#endregion

	#region Skidding State

	public float TNoSkidJump;

	public virtual void StSkiddingEnter()
	{
		TNoSkidJump = .1f;
		Model.Play("Skid", true);
		Audio.Play(Sfx.sfx_skid, Position);

		for (int i = 0; i < 5; i++)
			World.Request<Dust>().Init(Position + new Vec3(TargetFacing, 0) * i, new Vec3(-TargetFacing, 0.0f).Normalized() * 50, 0x666666);
	}

	public virtual void StSkiddingExit()
	{
		Model.Play("Idle", true);
	}

	public virtual void StSkiddingUpdate()
	{
		if (TNoSkidJump > 0)
			TNoSkidJump -= Time.Delta;

		if (TryDash())
			return;

		if (RelativeMoveInput.LengthSquared() < .2f * .2f || Vec2.Dot(RelativeMoveInput, TargetFacing) < .7f || !OnGround)
		{
			//cancelling
			StateMachine.State = States.Normal;
			return;
		}
		else
		{
			var velXY = velocity.XY();

			// skid jump
			if (TNoSkidJump <= 0 && Controls.Jump.ConsumePress())
			{
				StateMachine.State = States.Normal;
				SkidJump();
				return;
			}

			bool dotMatches = Vec2.Dot(velXY.Normalized(), TargetFacing) >= .7f;

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

	public float ClimbCornerEase = 0;
	public Vec3 ClimbCornerFrom;
	public Vec3 ClimbCornerTo;
	public Vec2 ClimbCornerFacingFrom;
	public Vec2 ClimbCornerFacingTo;
	public Vec2? ClimbCornerCameraFrom;
	public Vec2? ClimbCornerCameraTo;
	public int ClimbInputSign = 1;
	public float TClimbCooldown = 0;

	public virtual void StClimbingEnter()
	{
		Model.Play("Climb.Idle", true);
		Model.Rate = 1.8f;
		velocity = Vec3.Zero;
		ClimbCornerEase = 0;
		ClimbInputSign = 1;
		Audio.Play(Sfx.sfx_grab, Position);
	}

	public virtual void StClimbingExit()
	{
		Model.Play("Idle");
		Model.Rate = 1.0f;
		ClimbingWallActor = default;
		SfxWallSlide?.Stop();
	}

	public virtual void StClimbingUpdate()
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
			TargetFacing = -TargetFacing;
			WallJump();
			return;
		}

		if (DashesLocal > 0 && TDashCooldown <= 0 && Controls.Dash.ConsumePress())
		{
			StateMachine.State = States.Dashing;
			DashesLocal--;
			return;
		}

		CancelGroundSnap();

		var forward = new Vec3(TargetFacing, 0);
		var wallUp = ClimbingWallNormal.UpwardPerpendicularNormal();
		var wallRight = Vec3.TransformNormal(wallUp, Matrix.CreateFromAxisAngle(ClimbingWallNormal, -MathF.PI / 2));
		var forceCorner = false;
		var wallSlideSoundEnabled = false;

		// only change the input direction based on the camera when we stop moving
		// so if we keep holding a direction, we keep moving the same way (even if it's flipped in the perspective)
		if (MathF.Abs(Controls.Move.Value.X) < .5f)
			ClimbInputSign = (Vec2.Dot(TargetFacing, CameraTargetForward.XY().Normalized()) < -.4f) ? -1 : 1;

		var inputTranslated = Controls.Move.Value;
		inputTranslated.X *= ClimbInputSign;

		// move around
		if (ClimbCornerEase <= 0)
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
				if (inputTranslated.Y > 0 && !OnGround)
				{
					if (Time.OnInterval(0.05f))
					{
						var at = Position + wallUp * 5 + new Vec3(Facing, 0) * 2;
						var vel = TPlatformVelocityStorage > 0 ? PlatformVelocity : Vec3.Zero;
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
			var ease = 1.0f - ClimbCornerEase;

			velocity = Vec3.Zero;
			Position = Vec3.Lerp(ClimbCornerFrom, ClimbCornerTo, ease);
			TargetFacing = Calc.AngleToVector(Calc.AngleLerp(ClimbCornerFacingFrom.Angle(), ClimbCornerFacingTo.Angle(), ease));

			if (ClimbCornerCameraFrom.HasValue && ClimbCornerCameraTo.HasValue)
			{
				var angle = Calc.AngleLerp(ClimbCornerCameraFrom.Value.Angle(), ClimbCornerCameraTo.Value.Angle(), ease * 0.50f);
				CameraTargetForward = new Vec3(Calc.AngleToVector(angle), CameraTargetForward.Z);
			}

			Calc.Approach(ref ClimbCornerEase, 0, Time.Delta / 0.20f);
			return;
		}

		// reset corner lerp data in case we use it
		ClimbCornerFrom = Position;
		ClimbCornerFacingFrom = TargetFacing;
		ClimbCornerCameraFrom = null;
		ClimbCornerCameraTo = null;

		// move around inner corners
		if (inputTranslated.X != 0 && World.SolidRayCast(SolidWaistTestPos, wallRight * inputTranslated.X, ClimbCheckDist, out var hit))
		{
			Position = hit.Point + (Position - SolidWaistTestPos) + hit.Normal * WallPushoutDist;
			TargetFacing = -hit.Normal.XY();
			ClimbingWallNormal = hit.Normal;
			ClimbingWallActor = hit.Actor;
		}

		// snap to walls that slope away from us
		else if (World.SolidRayCast(SolidWaistTestPos, -ClimbingWallNormal, ClimbCheckDist + 2, out hit) && ClimbNormalCheck(hit.Normal))
		{
			Position = hit.Point + (Position - SolidWaistTestPos) + hit.Normal * WallPushoutDist;
			TargetFacing = -hit.Normal.XY();
			ClimbingWallNormal = hit.Normal;
			ClimbingWallActor = hit.Actor;
		}

		// rotate around corners due to input
		else if (
			inputTranslated.X != 0 &&
			World.SolidRayCast(SolidWaistTestPos + forward * (ClimbCheckDist + 1) + wallRight * inputTranslated.X, wallRight * -inputTranslated.X, ClimbCheckDist * 2, out hit) &&
			ClimbNormalCheck(hit.Normal))
		{
			Position = hit.Point + (Position - SolidWaistTestPos) + hit.Normal * WallPushoutDist;
			TargetFacing = -hit.Normal.XY();
			ClimbingWallNormal = hit.Normal;
			ClimbingWallActor = hit.Actor;

			//if (Vec2.Dot(targetFacing, CameraForward.XY().Normalized()) < -.3f)
			{
				ClimbCornerCameraFrom = CameraTargetForward.XY();
				ClimbCornerCameraTo = TargetFacing;
			}

			Model.Play("Climb.Idle");
			forceCorner = true;
		}
		// hops over tops
		else if (inputTranslated.Y < 0 && !ClimbCheckAt(Vec3.UnitZ, out _))
		{
			Audio.Play(Sfx.sfx_climb_ledge, Position);
			StateMachine.State = States.Normal;
			velocity = new(TargetFacing * ClimbHopForwardSpeed, ClimbHopUpSpeed);
			TNoMove = ClimbHopNoMoveTime;
			TClimbCooldown = 0.3f;
			AutoJump = false;
			AddPlatformVelocity(false);
			return;
		}
		// fall off
		else if (!TryClimb())
		{
			StateMachine.State = States.Normal;
			return;
		}

		if (ClimbingWallActor is Solid { IsClimbable: false })
		{
			StateMachine.State = States.Normal;
			return;
		}

		// update wall slide sfx
		if (wallSlideSoundEnabled)
			SfxWallSlide?.Resume();
		else
			SfxWallSlide?.Stop();

		// rotate around corners nicely
		if (forceCorner || (Position - ClimbCornerFrom).Length() > 2)
		{
			ClimbCornerEase = 1.0f;
			ClimbCornerTo = Position;
			ClimbCornerFacingTo = TargetFacing;
			Position = ClimbCornerFrom;
			TargetFacing = ClimbCornerFacingFrom;
		}
	}

	#endregion

	#region StrawbGet State

	public Strawberry? LastStrawb;
	public Vec2 StrawbGetForward;

	public virtual void StStrawbGetEnter()
	{
		Model.Play("StrawberryGrab");
		Model.Flags = ModelFlags.StrawberryGetEffect;
		Hair.Flags = ModelFlags.StrawberryGetEffect;
		if (LastStrawb is { } strawb)
			strawb.Model.Flags = ModelFlags.StrawberryGetEffect;
		velocity = Vec3.Zero;
		StrawbGetForward = (World.Camera.Position - Position).XY().Normalized();
		CameraOverride = new(World.Camera.Position, World.Camera.LookAt);
	}

	public virtual void StStrawbGetExit()
	{
		CameraOverride = null;

		Model.Flags = ModelFlags.Default | ModelFlags.Silhouette;
		Hair.Flags = ModelFlags.Default | ModelFlags.Silhouette;

		if (LastStrawb is { BubbleTo: not null })
		{
			BubbleTo(LastStrawb.BubbleTo.Value);
		}

		if (LastStrawb != null)
			World.Destroy(LastStrawb);
	}

	public virtual void StStrawbGetUpdate()
	{
		Facing = TargetFacing = Calc.AngleToVector(StrawbGetForward.Angle() - MathF.PI / 7);
		CameraOverride = new CameraOverrideStruct(Position + new Vec3(StrawbGetForward * 50, 40), Position + Vec3.UnitZ * 6);
	}

	public virtual CoEnumerator StStrawbGetRoutine()
	{
		yield return 2.0f;

		if (LastStrawb != null)
			Save.CurrentRecord.Strawberries.Add(LastStrawb.ID);

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
			LastStrawb = strawb;
			StateMachine.State = States.StrawbGet;
			Position = strawb.Position + Vec3.UnitZ * -3;
			LastStrawb.Position = Position + Vec3.UnitZ * 12;
		}
	}

	#endregion

	#region FeatherStart State

	public float tFeatherStart;

	public virtual void StFeatherStartEnter()
	{
		tFeatherStart = FeatherStartTime;
	}

	public virtual void StFeatherStartExit()
	{
	}

	public virtual void StFeatherStartUpdate()
	{
		var input = RelativeMoveInput;
		if (input != Vec2.Zero)
			TargetFacing = input.Normalized();

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
		if (DashesLocal > 0 && TDashCooldown <= 0 && Controls.Dash.ConsumePress())
		{
			StateMachine.State = States.Dashing;
			DashesLocal--;
			return;
		}
	}

	public virtual void FeatherGet(Feather feather)
	{
		Audio.Play(Sfx.sfx_dashcrystal, Position);
		World.HitStun = 0.05f;

		if (StateMachine.State == States.Feather)
		{
			TFeather = FeatherDuration;
			FeatherZ = feather.Position.Z - 2;
			Audio.Play(Sfx.sfx_feather_renew, Position);
		}
		else
		{
			StateMachine.State = States.FeatherStart;
			FeatherZ = feather.Position.Z - 2;
			DashesLocal = Math.Max(DashesLocal, 1);
			Audio.Play(Sfx.sfx_feather_get, Position);
		}
	}

	public virtual void HandleFeatherZ()
		=> Calc.Approach(ref velocity.Z, (FeatherZ - Position.Z) * 40, 600 * Time.Delta);

	#endregion

	#region Feather State

	public float FeatherZ;
	public float TFeather;
	public float TFeatherWallBumpCooldown;
	public bool FeatherPlayedEndWarn = false;

	public virtual void StFeatherEnter()
	{
		velocity = velocity.WithXY(TargetFacing * FeatherStartSpeed);
		TFeather = FeatherDuration;
		Hair.Roundness = 1;
		DrawModel = false;
		FeatherPlayedEndWarn = false;
		TFeatherWallBumpCooldown = 0;
		SfxFeather?.Resume();
	}

	public virtual void StFeatherExit()
	{
		Hair.Roundness = 0;
		DrawModel = true;
		SfxFeather?.Stop();
	}

	public virtual void StFeatherUpdate()
	{
		const float EndWarningTime = 0.8f;

		if (TFeather > EndWarningTime || Time.BetweenInterval(.1f))
			SetHairColor(Skin.HairFeather);
		else if (DashesLocal == 2)
			SetHairColor(Skin.HairTwoDash);
		else
			SetHairColor(Skin.HairNormal);

		HandleFeatherZ();

		var velXY = velocity.XY();

		var input = RelativeMoveInput;
		if (input != Vec2.Zero)
			input = input.Normalized();
		else
			input = TargetFacing;

		velXY = Calc.RotateToward(velXY, input * FeatherFlySpeed, FeatherTurnSpeed * Time.Delta, FeatherAccel * Time.Delta);
		TargetFacing = velXY.Normalized();
		velocity = velocity.WithXY(velXY);

		TFeather -= Time.Delta;
		TFeatherWallBumpCooldown -= Time.Delta;

		if (TFeather <= EndWarningTime && !FeatherPlayedEndWarn)
		{
			FeatherPlayedEndWarn = true;
			Audio.Play(Sfx.sfx_feather_state_end_warning, Position);
		}

		if (TFeather <= 0)
		{
			StateMachine.State = States.Normal;

			velocity.X *= FeatherExitXYMult;
			velocity.Y *= FeatherExitXYMult;
			HoldJumpSpeed = velocity.Z = FeatherExitZSpeed;
			THoldJump = .1f;
			AutoJump = true;
			Audio.Play(Sfx.sfx_feather_state_end, Position);

			return;
		}

		// dashing
		if (DashesLocal > 0 && TDashCooldown <= 0 && Controls.Dash.ConsumePress())
		{
			StateMachine.State = States.Dashing;
			DashesLocal--;
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

	public virtual void StRespawnEnter()
	{
		DrawModel = DrawHair = false;
		DrawOrbs = true;
		DrawOrbsEase = 1;
		PointShadowAlpha = 0;
		Audio.Play(Sfx.sfx_revive, Position);
	}

	public virtual void StRespawnUpdate()
	{
		DrawOrbsEase -= Time.Delta * 2;
		if (DrawOrbsEase <= 0)
			StateMachine.State = States.Normal;
	}

	public virtual void StRespawnExit()
	{
		PointShadowAlpha = 1;
		DrawModel = DrawHair = true;
		DrawOrbs = false;
	}

	#endregion

	#region B-Side Strawb Reveal

	// TODO: should maybe be a cutscene object? idk

	public Actor? enterLookAt;

	public virtual void StStrawbRevealEnter()
	{
	}

	public virtual CoEnumerator StStrawbRevealRoutine()
	{
		yield return Co.SingleFrame;

		enterLookAt = World.Get<Strawberry>();

		if (enterLookAt != null)
		{
			TargetFacing = (enterLookAt.Position - Position).XY().Normalized();

			var lookAt = enterLookAt.Position + new Vec3(0, 0, 3);
			var normal = (Position - lookAt).Normalized();
			var fromPos = lookAt + normal * 40 + Vec3.UnitZ * 20;
			var toPos = Position + new Vec3(0, 0, 16) + normal * 40;
			var control = (fromPos + toPos) * .5f + Vec3.UnitZ * 40;

			CameraOverride = new(fromPos, lookAt);
			World.Camera.Position = CameraOverride.Value.Position;
			World.Camera.LookAt = CameraOverride.Value.LookAt;

			yield return 1f;

			for (float p = 0; p < 1.0f; p += Time.Delta / 3)
			{
				CameraOverride = new(Utils.Bezier(fromPos, control, toPos, Ease.Sine.In(p)), lookAt);
				yield return Co.SingleFrame;
			}

			for (float p = 0; p < 1.0f; p += Time.Delta / 1f)
			{
				GetCameraTarget(out var lookAtTo, out var posTo, out _);

				var t = Ease.Sine.Out(p);
				CameraOverride = new(Vec3.Lerp(toPos, posTo, t), Vec3.Lerp(lookAt, lookAtTo, t));
				yield return Co.SingleFrame;
			}

			yield return .02f;
		}

		StateMachine.State = States.Normal;
	}

	public virtual void StStrawbRevealExit()
	{
		CameraOverride = null;
	}

	#endregion

	#region Dead State

	public virtual void StDeadEnter()
	{
		DrawModel = DrawHair = false;
		DrawOrbs = true;
		DrawOrbsEase = 0;
		PointShadowAlpha = 0;
		Audio.Play(Sfx.sfx_death, Position);
	}

	public virtual void StDeadUpdate()
	{
		if (DrawOrbsEase < 1.0f)
			DrawOrbsEase += Time.Delta * 2.0f;

		if (!Game.Instance.IsMidTransition && DrawOrbsEase > 0.30f)
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

	public virtual void StCutsceneEnter()
	{
		Model.Play("Idle");
		// Fix white hair in cutscene bug
		if (TDashResetFlash > 0)
		{
			TDashResetFlash = 0;
			SetHairColor(CNormal);
		}
	}

	public virtual void StCutsceneUpdate()
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

	public virtual CoEnumerator StBubbleRoutine()
	{
		var bubbleFrom = Position;
		var control = (bubbleTo + bubbleFrom) * .5f + Vec3.UnitZ * 40;
		float duration = (bubbleTo - bubbleFrom).Length() / 220;
		float ease = 0.0f;

		yield return .2f;

		SfxBubble?.Resume();
		while (ease < 1.0f)
		{
			Calc.Approach(ref ease, 1.0f, Time.Delta / duration);
			Position = Utils.Bezier(bubbleFrom, control, bubbleTo, Utils.SineInOut(ease));
			yield return Co.SingleFrame;
		}

		yield return .2f;
		StateMachine.State = States.Normal;
	}

	public virtual void StBubbleExit()
	{
		Audio.Play(Sfx.sfx_bubble_out, Position);
		SfxBubble?.Stop();
		PointShadowAlpha = 1;
	}

	#endregion

	#region Cassette State

	public Cassette? cassette;

	public virtual void EnterCassette(Cassette it)
	{
		if (StateMachine.State != States.Cassette)
		{
			cassette = it;
			StateMachine.State = States.Cassette;
			Position = it.Position - Vec3.UnitZ * 3;
			DrawModel = DrawHair = false;
			PointShadowAlpha = 0;
			CameraOverride = new(World.Camera.Position, it.Position);
			Game.Instance.Ambience.Stop();
			Audio.StopBus(Sfx.bus_gameplay_world, false);
			Audio.Play(Sfx.sfx_cassette_enter, Position);
		}
	}

	public virtual CoEnumerator StCassetteRoutine()
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
		HoldJumpSpeed = velocity.Z;
		THoldJump = .1f;
		AutoJump = true;

	}

	public virtual void StCassetteExit()
	{
		cassette?.SetCooldown();
		cassette = null;
		DrawModel = DrawHair = true;
		CameraOverride = null;
		PointShadowAlpha = 1;
	}

	#endregion

	#region Normal State

	public virtual void StDebugFlyEnter()
	{
		THoldJump = 0;
		TFootstep = FootstepInterval;
		Model.Rate = 0;
		Velocity = Vec3.Zero;
	}

	public virtual void StDebugFlyExit()
	{
		THoldJump = 0;
		TNoMove = 0;
		AutoJump = false;
		Model.Rate = 1;
	}

	public virtual void StDebugFlyUpdate()
	{
		// movement
		{
			var velXY = Velocity.XY();
			if (Controls.Move.Value == Vec2.Zero || TNoMove > 0)
			{
				// friction
				Calc.Approach(ref velXY, Vec2.Zero, Friction * Time.Delta);
			}
			else
			{
				float accel;
				if (velXY.LengthSquared() >= MaxSpeed * MaxSpeed && Vec2.Dot(RelativeMoveInput.Normalized(), velXY.Normalized()) >= .7f)
				{
					accel = PastMaxDeccel;

					var dot = Vec2.Dot(RelativeMoveInput.Normalized(), TargetFacing);
					accel *= Calc.ClampedMap(dot, -1, 1, AirAccelMultMax, AirAccelMultMin);
				}
				else
				{
					accel = Acceleration;

					var dot = Vec2.Dot(RelativeMoveInput.Normalized(), TargetFacing);
					accel *= Calc.ClampedMap(dot, -1, 1, AirAccelMultMin, AirAccelMultMax);
				}

				Calc.Approach(ref velXY, RelativeMoveInput * MaxSpeed, accel * Time.Delta);
			}

			Velocity = Velocity.WithXY(velXY);
		}

		if (Controls.Jump.Down)
		{
			Position = new Vector3(Position.X, Position.Y, Position.Z + 1.5f);
		}
		if (Controls.Dash.Down)
		{
			Position = new Vector3(Position.X, Position.Y, Position.Z - 1.5f);
		}

		if (Controls.Move.Value != Vec2.Zero && TNoMove <= 0)
		{
			TargetFacing = Calc.RotateToward(TargetFacing, RelativeMoveInput, RotateSpeed * Time.Delta, 0);
			Facing = Calc.AngleToVector(Calc.AngleApproach(Facing.Angle(), TargetFacing.Angle(), MathF.Tau * 2 * Time.Delta));
		}

		Position += velocity * Time.Delta;
	}

	#endregion

	#region Graphics

	public virtual void CollectSprites(List<Sprite> populate)
	{
		// debug: draw camera origin pos
		if (World.DebugDraw)
		{
			populate.Add(Sprite.CreateBillboard(World, CameraOriginPos, "circle", 1, Color.Red));
		}

		// debug: draw wall up-normal
		if (World.DebugDraw)
		{
			if (StateMachine.State == States.Climbing)
			{
				var up = ClimbingWallNormal.UpwardPerpendicularNormal();

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

		if (DrawOrbs && DrawOrbsEase > 0)
		{
			var ease = DrawOrbsEase;
			var col = Math.Floor(ease * 10) % 2 == 0 ? Hair.Color : Color.White;
			var s = (ease < 0.5f) ? (0.5f + ease) : (Ease.Cube.Out(1 - (ease - 0.5f) * 2));
			for (int i = 0; i < 8; i++)
			{
				var rot = (i / 8f + ease * 0.25f) * MathF.Tau;
				var rad = Ease.Cube.Out(ease) * 16;
				var pos = SolidWaistTestPos + World.Camera.Left * MathF.Cos(rot) * rad + World.Camera.Up * MathF.Sin(rot) * rad;
				var size = 3 * s;
				populate.Add(Sprite.CreateBillboard(World, pos, "circle", size + 0.5f, Color.Black) with { Post = true });
				populate.Add(Sprite.CreateBillboard(World, pos, "circle", size, col) with { Post = true });
			}
		}

		if (!OnGround && !Dead && PointShadowAlpha > 0 && !InBubble && Settings.ZGuide)
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
			if ((!Skin.HideHair || InFeatherState) && DrawHair)
				populate.Add((this, Hair));

			if (DrawModel)
				populate.Add((this, Model));
		}

		foreach (var trail in Trails)
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

		if (TPlatformVelocityStorage < 0 || value.Z >= velocity.Z
		|| value.XY().LengthSquared() + .1f >= velocity.XY().LengthSquared()
		|| (value.XY() != Vec2.Zero && Vec2.Dot(value.XY().Normalized(), velocity.XY().Normalized()) < .5f))
		{
			PlatformVelocity = value;
			TPlatformVelocityStorage = .1f;
		}
	}

	public virtual bool RidingPlatformCheck(Actor platform)
	{
		// check if we're climbing this thing
		if (platform == ClimbingWallActor)
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
		ClimbCornerFrom += newDelta;
		ClimbCornerTo += newDelta;
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
