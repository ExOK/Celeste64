
namespace Celeste64;

public abstract class NPC : Actor, IHaveModels, IHaveSprites, IHavePushout, ICastPointShadow
{
	public SkinnedModel Model;

	public bool InteractEnabled = true;
	public float InteractRadius = 16;
	public Vec3 InteractHoverOffset;
	public bool IsPlayerOver;

	private bool showHover = false;
	private float showTimer = 0;

	public float PushoutHeight { get; set; } = 12;
	public float PushoutRadius { get; set; } = 8;
	public float PointShadowAlpha { get; set; }

	public NPC(SkinnedTemplate model)
	{
		Model = new SkinnedModel(model);
		Model.Play("idle");

		foreach (var mat in Model.Materials)
			mat.Effects = 0.70f;

		LocalBounds = new BoundingBox(Vec3.Zero + Vec3.UnitZ * 4, 8);
		PointShadowAlpha = 1;
	}

	public abstract void Interact(Player player);

	public override void Update()
	{
		if (World.Camera.Frustum.Contains(WorldBounds))
			Model.Update();
		showTimer += Time.Delta;
	}

	public override void LateUpdate()
	{
		if (!showHover && IsPlayerOver)
			Audio.Play(Sfx.ui_npc_popup);
		showHover = IsPlayerOver;
		IsPlayerOver = false;
	}

	public virtual void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		populate.Add((this, Model));
	}

	public void CollectSprites(List<Sprite> populate)
	{
		if (showHover)
		{
			var at = Vec3.Transform(InteractHoverOffset, Matrix);
			at += Vec3.UnitZ * MathF.Sin(showTimer * 8);
			populate.Add(Sprite.CreateBillboard(World, at, "interact", 2, Color.White) with { Post = true });
		}
	}
}
