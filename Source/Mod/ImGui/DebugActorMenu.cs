using ImGuiNET;
using System.Reflection;

namespace Celeste64.Mod;

internal class DebugActorMenu : ImGuiHandler
{
	public override bool Visible { get; set; } = false;
	// Actor windows
	private bool actorListWindowVisible = false;
	private bool actorPropertiesWindowVisible = false;

	// Selected actor
	private Type? selectedActorType;
	private Actor? selectedActor;

	public override void Update()
	{
		if (Input.Keyboard.Pressed(Keys.F6))
		{
			if (Visible) Visible = false;
			if (actorListWindowVisible) actorListWindowVisible = false;
			if (actorPropertiesWindowVisible) actorPropertiesWindowVisible = false;
		}
	}

	public override void Render()
	{
		if (Visible && Game.Instance.Scene is World world && world.Get<Player>() is { } player)
		{
			if (actorPropertiesWindowVisible)
			{
				RenderActorPropertiesWindow(world, player);
			}
			else if (actorListWindowVisible)
			{
				RenderActorListWindow(world);
			}
			else
			{
				RenderActorTypeListWindow(GetActorTypesInWorld(world));
			}
		}
	}

	private void RenderActorTypeListWindow(List<Type> actorTypes)
	{
		if (ImGui.Button("Back"))
		{
			Visible = false;
		}

		ImGui.TextColored(new Vec4(0.592f, 0.843f, 0.988f, 1f), "ACTOR TYPES");
		foreach (var actorType in actorTypes)
		{
			if (ImGui.Button(actorType.Name))
			{
				actorPropertiesWindowVisible = false;
				selectedActor = null;
				selectedActorType = actorType;
				actorListWindowVisible = true;
			}
		}
	}

	private void RenderActorListWindow(World world)
	{
		if (ImGui.Button("Back"))
		{
			actorListWindowVisible = false;
			selectedActorType = null;
			actorPropertiesWindowVisible = false;
			selectedActor = null;
		}

		ImGui.TextColored(new Vec4(0.592f, 0.843f, 0.988f, 1f), "ACTORS");
		int i = 1;
		foreach (var actor in world.Actors)
		{
			if (actor.GetType() == selectedActorType)
			{
				if (ImGui.Button($"{i}. {selectedActorType.Name}"))
				{
					selectedActor = actor;
					actorPropertiesWindowVisible = true;
				}
				i++;
			}
		}
	}

	private void RenderActorPropertiesWindow(World world, Player player)
	{
		if (ImGui.Button("Back") || selectedActor == null || !selectedActor.Alive || selectedActorType == null)
		{
			actorPropertiesWindowVisible = false;
			selectedActor = null;
			return;
		}

		var properties = GetProperties(selectedActorType);
		var fields = GetFields(selectedActorType);

		ImGui.TextColored(new Vec4(0.592f, 0.843f, 0.988f, 1f), selectedActorType.Name);
		if (ImGui.Button("TELEPORT"))
			player.Position = selectedActor.Position;

		if (ImGui.Button("DESTROY"))
		{
			world.Destroy(selectedActor);
			actorPropertiesWindowVisible = false;
			selectedActor = null;
			return;
		}

		ImGui.TextColored(new Vec4(0.592f, 0.843f, 0.988f, 1f), "PROPERTIES");
		foreach (var property in properties)
		{
			// If the property is a Vec3
			if (property.PropertyType == typeof(Vec3) && property.GetValue(selectedActor) is Vec3 newVec3)
			{
				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.DragFloat3(property.Name, ref newVec3);
				if (property.CanWrite)
					property.SetValue(selectedActor, newVec3);
				else
					ImGui.EndDisabled();
			}

			// If the property is a bool
			if (property.PropertyType == typeof(bool) && property.GetValue(selectedActor) is bool newBool)
			{
				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.Checkbox(property.Name, ref newBool);
				if (property.CanWrite)
					property.SetValue(selectedActor, newBool);
				else
					ImGui.EndDisabled();
			}

			// If the property is a int
			if (property.PropertyType == typeof(int) && property.GetValue(selectedActor) is int newInt)
			{
				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.InputInt(property.Name, ref newInt);
				if (property.CanWrite)
					property.SetValue(selectedActor, newInt);
				else
					ImGui.EndDisabled();
			}

			// If the property is a float
			if (property.PropertyType == typeof(float) && property.GetValue(selectedActor) is float newFloat)
			{
				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.InputFloat(property.Name, ref newFloat);
				if (property.CanWrite)
					property.SetValue(selectedActor, newFloat);
				else
					ImGui.EndDisabled();
			}

			// If the property is a Color
			if (property.PropertyType == typeof(Color) && property.GetValue(selectedActor) is Color newColor)
			{
				var colAsVec = new Vec4(newColor.R, newColor.G, newColor.B, newColor.A);

				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.ColorPicker4(property.Name, ref colAsVec);
				if (property.CanWrite)
					property.SetValue(selectedActor, new Color(colAsVec.X, colAsVec.Y, colAsVec.Z, colAsVec.Z));
				else
					ImGui.EndDisabled();
			}

			// If the property is a string
			if (property.PropertyType == typeof(string) && property.GetValue(selectedActor) is string newString)
			{
				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.InputText(property.Name, ref newString, 256);
				if (property.CanWrite)
					property.SetValue(selectedActor, newString);
				else
					ImGui.EndDisabled();
			}
		}

		ImGui.TextColored(new Vec4(0.592f, 0.843f, 0.988f, 1f), "FIELDS");
		foreach (var field in fields)
		{
			// If the field is a Vec3
			if (field.FieldType == typeof(Vec3) && field.GetValue(selectedActor) is Vec3 newVec3)
			{
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.DragFloat3(field.Name, ref newVec3);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, newVec3);
				else
					ImGui.EndDisabled();
			}

			// If the field is a bool
			if (field.FieldType == typeof(bool) && field.GetValue(selectedActor) is bool newBool)
			{
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.Checkbox(field.Name, ref newBool);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, newBool);
				else
					ImGui.EndDisabled();
			}

			// If the field is a int
			if (field.FieldType == typeof(int) && field.GetValue(selectedActor) is int newInt)
			{
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.InputInt(field.Name, ref newInt);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, newInt);
				else
					ImGui.EndDisabled();
			}

			// If the field is a float
			if (field.FieldType == typeof(float) && field.GetValue(selectedActor) is float newFloat)
			{
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.InputFloat(field.Name, ref newFloat);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, newFloat);
				else
					ImGui.EndDisabled();
			}

			// If the field is a Color
			if (field.FieldType == typeof(Color) && field.GetValue(selectedActor) is Color newColor)
			{
				var colAsVec = new Vec4(newColor.R, newColor.G, newColor.B, newColor.A);
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.ColorPicker4(field.Name, ref colAsVec);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, new Color(colAsVec.X * 255, colAsVec.Y * 255, colAsVec.Z * 255, colAsVec.Z * 255));
				else
					ImGui.EndDisabled();
			}

			// If the field is a string
			if (field.FieldType == typeof(string) && field.GetValue(selectedActor) is string newString)
			{
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.InputText(field.Name, ref newString, 256);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, newString);
				else
					ImGui.EndDisabled();
			}
		}
	}

	private PropertyInfo[] GetProperties(Type classType)
	{
		var props = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		props.OrderBy(obj => obj.GetType().Name);
		return props;
	}

	private FieldInfo[] GetFields(Type classType)
	{
		var fields = classType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		fields.OrderBy(obj => obj.GetType().Name);
		return fields;
	}

	private List<Type> GetActorTypesInWorld(World world)
	{
		var actorTypes = new List<Type>();

		foreach (var actor in world.All<Actor>())
			if (!actorTypes.Contains(actor.GetType()))
				actorTypes.Add(actor.GetType());

		actorTypes.OrderBy(obj => obj.GetType().Name);
		return actorTypes;
	}
}
