
namespace Celeste64;

public class Badeline : NPC
{
	public virtual string TALK_FLAG => "BADELINE";

	public readonly Hair Hair;
	public virtual Color HairColor => 0x9B3FB5;

	public Badeline() : base(Assets.Models["badeline"])
	{
		Model.Play("Bad.Idle");

		foreach (var mat in Model.Materials)
		{
			if (mat.Name == "Hair")
			{
				mat.Color = HairColor;
				mat.Effects = 0;
			}
			mat.SilhouetteColor = HairColor;
		}

		Hair = new()
		{
			Color = HairColor,
			ForwardOffsetPerNode = 0,
			Nodes = 10
		};

		InteractHoverOffset = new Vec3(0, -2, 16);
		InteractRadius = 32;
		CheckForDialog();
	}

	public override void Update()
	{
		base.Update();

		// update model
		Model.Transform =
			Matrix.CreateScale(3) *
			Matrix.CreateTranslation(0, 0, MathF.Sin(World.GeneralTimer * 2) * 1.0f - 1.5f);

		// update hair
		{
			var hairMatrix = Matrix.Identity;
			foreach (var it in Model.Instance.Armature.LogicalNodes)
				if (it.Name == "Head")
					hairMatrix = it.ModelMatrix * SkinnedModel.BaseTranslation * Model.Transform * Matrix;
			Hair.Flags = Model.Flags;
			Hair.Forward = -new Vec3(Facing, 0);
			Hair.Materials[0].Effects = 0;
			Hair.Update(hairMatrix);
		}

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
		yield return Co.Run(cs.Say(Loc.Lines($"Baddy{index}")));
		Save.CurrentRecord.IncFlag(TALK_FLAG);
		CheckForDialog();
	}

	public override void CollectModels(List<(Actor Actor, Model Model)> populate)
	{
		populate.Add((this, Hair));
		base.CollectModels(populate);
	}

	public virtual void CheckForDialog()
	{
		InteractEnabled = Loc.HasLines($"Baddy{Save.CurrentRecord.GetFlag(TALK_FLAG) + 1}");
	}
}

