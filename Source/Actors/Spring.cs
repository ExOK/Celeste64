
namespace Celeste64;

public class Spring : Attacher, IHaveModels, IPickup
{
	public SkinnedModel Model;

	public virtual float PickupRadius => 16;

	public float TCooldown = 0;

	public Spring()
	{
		Model = new SkinnedModel(Assets.Models["spring_board"]);
		Model.Transform = Matrix.CreateScale(8.0f);
		Model.SetLooping("Spring", false);
		Model.Play("Idle");

		LocalBounds = new(Position + Vec3.UnitZ * 4, 16);
	}

	public override void Update()
	{
		Model.Update();

		if (TCooldown > 0)
		{
			TCooldown -= Time.Delta;
			if (TCooldown <= 0)
				UpdateOffScreen = false;
		}
	}

	public virtual void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		populate.Add((this, Model));
	}

	public virtual void Pickup(Player player)
	{
		if (TCooldown <= 0)
		{
			UpdateOffScreen = true;
			Audio.Play(Sfx.sfx_springboard, Position);
			TCooldown = 1.0f;
			Model.Play("Spring", true);
			player.Spring(this);

			if (AttachedTo is FallingBlock fallingBlock)
				fallingBlock.Trigger();
		}
	}
}
