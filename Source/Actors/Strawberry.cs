
namespace Celeste64;

public class Strawberry : Actor, IHaveModels, IHaveSprites, IPickup, ICastPointShadow
{
	public SkinnedModel Model;
	public ParticleSystem Particles;

	public float Pulse => Calc.ClampedMap(World.GeneralTimer % 3, 0, 0.25f, 0, 1);
	public float PickupRadius => 12;
	
	public bool IsCollected => 
		!string.IsNullOrEmpty(ID) && 
		Save.CurrentRecord.Strawberries.Contains(ID);

	public Color HaloColor = 0xeed14f;
	public float PointShadowAlpha { get; set; } = 1.0f;

	public bool IsLocked { get; private set; }
	public readonly string ID;
	public readonly string UnlockConditionGroup;
	public readonly Vec3? BubbleTo;
	public readonly bool PlayUnlockSound;

	private bool isCollecting = false;
	private readonly List<Actor> unlockConditions = [];
	private float checkConditionsOffset;
	private float scaleMultiplier = 1;

	public Strawberry(string id, bool isLocked, string? unlockCondition, bool unlockSound, Vec3? bubbleTo)
	{
		ID = id;
		IsLocked = isLocked;
		UnlockConditionGroup = unlockCondition ?? string.Empty;
		PlayUnlockSound = unlockSound;
		BubbleTo = bubbleTo;
		Model = new(Assets.Models["strawberry"]);
		Model.Transform = Matrix.CreateScale(3);
		Model.Materials[0].Effects = 0;
		LocalBounds = new BoundingBox(Vec3.Zero, 10);
		Particles = new(32, new ParticleTheme()
		{
			Rate = 10.0f,
			Sprite = "particle-star",
			Life = 1.0f,
			Gravity = new Vec3(0, 0, 80),
			Size = 1.2f
		});
		
	}

	public override void Added()
	{
		if (IsCollected)
			IsLocked = false;

		if (IsLocked)
		{
			foreach (var actor in World.All<IUnlockStrawberry>())
			{
				if (actor.GroupName == UnlockConditionGroup)
					unlockConditions.Add(actor);
			}

			if (unlockConditions.Count <= 0)
				IsLocked = false;

			checkConditionsOffset = World.Rng.Float();
			PointShadowAlpha = 0;
		}

		UpdateOffScreen = IsLocked;
	}

	public override void Update()
	{
		if (IsLocked)
		{
			if (Time.OnInterval(0.1f, checkConditionsOffset))
			{
				bool ready = true;
				foreach (var it in unlockConditions)
				{
					if (it.Alive && !(it as IUnlockStrawberry)!.Satisfied)
						ready = false;
				}

				if (ready)
				{
					World.Add(new Cutscene(UnlockRoutine, true));
				}
			}

			return;
		}

		PointShadowAlpha = IsCollected ? 0.5f : 1.0f;

		if (!IsCollected || isCollecting)
		{
			Particles.SpawnParticle(
				Position + new Vec3(6 - World.Rng.Float() * 12, 6 - World.Rng.Float() * 12, 6 - World.Rng.Float() * 12),
				new Vec3(0,0,0), 1);
			Particles.Update(Time.Delta);
		}
		else
		{
			Model.MakeMaterialsUnique();
			Model.Flags = ModelFlags.Transparent;	

			foreach (var mat in Model.Materials)
			{
				mat.Texture = Assets.Textures["white"];
				mat.Color = new Color(0x99ddf4) * 0.70f;
			}
		}
	}

	public void CollectSprites(List<Sprite> populate)
	{
		if (IsCollected || IsLocked)
			return;

		var haloPos = Position + Vec3.UnitZ * 2 + Vec3.Transform(Vec3.Zero, Model.Transform);
		populate.Add(Sprite.CreateBillboard(World, haloPos, "gradient", 12, HaloColor * 0.40f));

		var pulse = Pulse;
		if (pulse < 1.0f)
		{
			var size = Ease.Cube.Out(pulse) * 20;
			var alpha = 1.0f - pulse;
			populate.Add(Sprite.CreateBillboard(World, haloPos, "gradient", size, HaloColor * 0.40f * alpha));
		}

		Particles.CollectSprites(Position, World, populate);
	}

	public void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		if (!IsLocked)
		{
			var scale = 3.0f;
			if (!IsCollected && !isCollecting)
				scale += Ease.Back.In(Ease.UpDown(Pulse)) * 0.50f;
			scale *= scaleMultiplier;

			if (isCollecting)
			{
				Model.Transform =
					Matrix.CreateScale(scale);
			}
			else
			{
				Model.Transform =
					Matrix.CreateScale(scale) *
					Matrix.CreateTranslation(Vec3.UnitZ * MathF.Sin(World.GeneralTimer * 2.0f) * 2) *
					Matrix.CreateRotationZ(World.GeneralTimer * 3.0f);
			}

			populate.Add((this, Model));
		}
	}

	public void Pickup(Player player)
	{
		if (!IsCollected && !isCollecting && !IsLocked)
		{
			Audio.Play(World.Entry.Submap ? Sfx.sfx_collect_strawb_bside : Sfx.sfx_collect_strawb, Position);
			isCollecting = true;
			player.StrawbGet(this);
		}
	}

	private CoEnumerator UnlockRoutine(Cutscene cs)
	{
		var wasLookAt = World.Camera.LookAt;
		var wasPosition = World.Camera.Position;

		var toNormal = (World.Camera.Position - Position).Normalized();
		var toPosition = Position + toNormal * 70;

		if (PlayUnlockSound)
			Audio.Play(Sfx.sfx_secret, Position);
		yield return 0.1f;

		for (float t = 0; t < 1.0f; t += Time.Delta / 0.2f)
		{
			World.Camera.LookAt = Vec3.Lerp(wasLookAt, Position, Utils.SineInOut(t));
			yield return Co.SingleFrame;
		}

		yield return 0.2f;

		for (float t = 0; t < 1.0f; t += Time.Delta)
		{
			World.Camera.Position = Vec3.Lerp(wasPosition, toPosition, Utils.SineInOut(t));
			yield return Co.SingleFrame;
		}

		yield return 0.1f;

		IsLocked = false;
		Audio.Play(Sfx.sfx_berry_appear, Position);

		for (float t = 0; t < 1; t += Time.Delta / .8f)
		{
			scaleMultiplier = 1 + MathF.Sin(t * MathF.Tau * 3) * .3f * (1 - t);
			yield return Co.SingleFrame;
		}

		scaleMultiplier = 1;
		yield return .7f;

		World.Camera.LookAt = wasLookAt;
		World.Camera.Position = wasPosition;
		UpdateOffScreen = false;
	}
}
