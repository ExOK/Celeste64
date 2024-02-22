
namespace Celeste64;

public class Theo : NPC
{
	public virtual string TALK_FLAG => "THEO";

	public Theo() : base(Assets.Models["theo"])
	{
		Model.Transform = Matrix.CreateScale(3) * Matrix.CreateTranslation(0, 0, -1.5f);
		InteractHoverOffset = new Vec3(0, -2, 16);
		InteractRadius = 32;
		CheckForDialog();
	}

	public override void Interact(Player player)
	{
		World.Add(new Cutscene(Conversation));
	}

	public virtual CoEnumerator Conversation(Cutscene cs)
	{
		yield return Co.Run(cs.MoveToDistance(World.Get<Player>(), Position.XY(), 16));
		yield return Co.Run(cs.FaceEachOther(World.Get<Player>(), this));

		int index = Save.CurrentRecord.GetFlag(TALK_FLAG) + 1;
		yield return Co.Run(cs.Say(Loc.Lines($"Theo{index}")));
		Save.CurrentRecord.IncFlag(TALK_FLAG);
		CheckForDialog();
	}

	public virtual void CheckForDialog()
	{ 
		InteractEnabled = Loc.HasLines($"Theo{Save.CurrentRecord.GetFlag(TALK_FLAG) + 1}");
	}
}

