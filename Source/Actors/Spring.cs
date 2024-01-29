
namespace Celeste64;

public class Spring : Attacher, IHaveModels, IPickup
{
	public SkinnedModel Model;

	public float PickupRadius => 16;

	private float tCooldown = 0;

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

		if (tCooldown > 0)
		{
			tCooldown -= Time.Delta;
			if (tCooldown <= 0)
				UpdateOffScreen = false;
		}
	}

	public void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		populate.Add((this, Model));
	}

	public void Pickup(Player player)
	{
		if (tCooldown <= 0)
		{
			UpdateOffScreen = true;
			Audio.Play(Sfx.sfx_springboard, Position);
			tCooldown = 1.0f;
			Model.Play("Spring", true);
			player.Spring(this);

			if (AttachedTo is FallingBlock fallingBlock)
				fallingBlock.Trigger();
		}
	}
}
