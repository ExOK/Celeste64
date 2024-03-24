using ImGuiNET;
using System.Diagnostics;
using System.Net;
using System.Reflection;
namespace Celeste64.Mod;

internal class DebugActor : ImGuiHandler
{
	// Actor windows
	bool actorListWindowVisible = false;
	bool actorTypeListWindowVisible = false;
	bool actorPropertiesWindowVisible = false;

	// Selected actor
	Type? selectedActorType;
	Actor? selectedActor;
	Actor? unModifiedSelectedActor;

	public override void Update()
	{
		if (Input.Keyboard.Pressed(Keys.F6))
		{
			actorListWindowVisible = !actorListWindowVisible;
			if (actorTypeListWindowVisible) actorTypeListWindowVisible = false;
			if(actorPropertiesWindowVisible) actorPropertiesWindowVisible = false;
		}
	}

	PropertyInfo[] GetProperties(Type classType)
	{
		var props = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		props.OrderBy(obj => obj.GetType().Name);
		return props;
	}

	FieldInfo[] GetFields(Type classType)
	{
		var fields = classType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
		fields.OrderBy(obj => obj.GetType().Name);
		return fields;
	}

	List<Type> GetActorTypesInWorld(World world)
	{
		List<Type> actorTypes = new List<Type>();

		foreach (Actor actor in world.All<Actor>())
			if (!actorTypes.Contains(actor.GetType()))
				actorTypes.Add(actor.GetType());

		actorTypes.OrderBy(obj => obj.GetType().Name);
		return actorTypes;
	}

	void RenderActorListWindow(List<Type> actorTypes)
	{
		if (!actorListWindowVisible) return;

		ImGui.Begin("Actors in World");
		foreach (Type actorType in actorTypes)
			if (ImGui.Button(actorType.Name))
			{
				actorPropertiesWindowVisible = false;
				selectedActor = null;
				selectedActorType = actorType;
				actorTypeListWindowVisible = true;
				unModifiedSelectedActor = null;
			}
		
		ImGui.End();
	}

	void RenderActorTypeListWindow(World world)
	{
		if (!actorTypeListWindowVisible) return;
		int i = 1;

		ImGui.Begin($"Actors Type {selectedActorType.Name}");
		foreach (Actor actor in world.Actors)
			if (actor.GetType() == selectedActorType)
			{
				if (ImGui.Button($"{i}. {selectedActorType.Name}"))
				{
					selectedActor = actor;
					unModifiedSelectedActor = actor;
					actorPropertiesWindowVisible = true;
				}
				i++;
			}
		if (ImGui.Button("Close Window"))
		{
			actorTypeListWindowVisible = false;
			selectedActorType = null;
			actorPropertiesWindowVisible = false;
			selectedActor = null;
			unModifiedSelectedActor = null;
		}

		ImGui.End();
	}

	void RenderActorPropertiesWindow(World world, Player player)
	{
		if (!actorPropertiesWindowVisible) return;
		PropertyInfo[] properties = GetProperties(selectedActorType);
		FieldInfo[] fields = GetFields(selectedActorType);

		ImGui.Begin($"{selectedActorType.Name}");
		if (ImGui.Button("TELEPORT"))
			player.Position = selectedActor.Position;
		ImGui.TextColored(new Vec4(0.592f, 0.843f, 0.988f, 1f), "PROPERTIES"); 
		foreach (PropertyInfo property in properties)
		{
			// If the property is a Vec3
			if (property.PropertyType == typeof(Vec3))
			{
				Vec3 newVar = (Vec3)property.GetValue(selectedActor);
				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.DragFloat3(property.Name, ref newVar);
				if (property.CanWrite)
					property.SetValue(selectedActor, newVar);
				else
					ImGui.EndDisabled();
			}

			// If the property is a bool
			if (property.PropertyType == typeof(bool))
			{
				bool newVar = (bool)property.GetValue(selectedActor);
				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.Checkbox(property.Name, ref newVar);
				if (property.CanWrite)
					property.SetValue(selectedActor, newVar);
				else
					ImGui.EndDisabled();
			}

			// If the property is a int
			if (property.PropertyType == typeof(int))
			{
				int newVar = (int)property.GetValue(selectedActor);
				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.InputInt(property.Name, ref newVar);
				if (property.CanWrite)
					property.SetValue(selectedActor, newVar);
				else
					ImGui.EndDisabled();
			}

			// If the property is a float
			if (property.PropertyType == typeof(float))
			{
				float newVar = (float)property.GetValue(selectedActor);
				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.InputFloat(property.Name, ref newVar);
				if (property.CanWrite)
					property.SetValue(selectedActor, newVar);
				else
					ImGui.EndDisabled();
			}

			// If the property is a Color
			if (property.PropertyType == typeof(Color))
			{
				Color newVar = (Color)property.GetValue(selectedActor);
				Vec4 colAsVec = new Vec4(newVar.R, newVar.G, newVar.B, newVar.A);

				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.ColorPicker4(property.Name, ref colAsVec);
				if (property.CanWrite)
					property.SetValue(selectedActor, new Color(colAsVec.X, colAsVec.Y, colAsVec.Z, colAsVec.Z));
				else
					ImGui.EndDisabled();
			}
			
			// If the property is a string
			if (property.PropertyType == typeof(string))
			{
				string newVar = (string)property.GetValue(selectedActor);

				if (!property.CanWrite) ImGui.BeginDisabled(true);
				ImGui.InputText(property.Name, ref newVar, 256);
				if (property.CanWrite)
					property.SetValue(selectedActor, newVar);
				else 
					ImGui.EndDisabled();
			}
		}

		ImGui.TextColored(new Vec4(0.592f, 0.843f, 0.988f, 1f), "FIELDS");
		foreach (FieldInfo field in fields)
		{
			// If the field is a Vec3
			if (field.FieldType == typeof(Vec3))
			{
				Vec3 newVar = (Vec3)field.GetValue(selectedActor);
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.DragFloat3(field.Name, ref newVar);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, newVar);
				else
					ImGui.EndDisabled();
			}

			// If the field is a bool
			if (field.FieldType == typeof(bool))
			{
				bool newVar = (bool)field.GetValue(selectedActor);
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.Checkbox(field.Name, ref newVar);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, newVar);
				else
					ImGui.EndDisabled();
			}

			// If the field is a int
			if (field.FieldType == typeof(int))
			{
				int newVar = (int)field.GetValue(selectedActor);
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.InputInt(field.Name, ref newVar);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, newVar);
				else
					ImGui.EndDisabled();
			}

			// If the field is a float
			if (field.FieldType == typeof(float))
			{
				float newVar = (float)field.GetValue(selectedActor);
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.InputFloat(field.Name, ref newVar);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, newVar);
				else
					ImGui.EndDisabled();
			}

			// If the field is a Color
			if (field.FieldType == typeof(Color))
			{
				Color newVar = (Color)field.GetValue(selectedActor);
				Vec4 colAsVec = new Vec4(newVar.R, newVar.G, newVar.B, newVar.A);
				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.ColorPicker4(field.Name, ref colAsVec);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, new Color(colAsVec.X * 255, colAsVec.Y * 255, colAsVec.Z * 255, colAsVec.Z * 255));
				else
					ImGui.EndDisabled();
			}
			
			// If the field is a string
			if (field.FieldType == typeof(string))
			{
				string newVar = (string)field.GetValue(selectedActor);

				if (field.IsInitOnly) ImGui.BeginDisabled(true);
				ImGui.InputText(field.Name, ref newVar, 256);
				if (!field.IsInitOnly)
					field.SetValue(selectedActor, newVar);
				else
					ImGui.EndDisabled();
			}
		}

		if (ImGui.Button("Close window") || !selectedActor.Alive || selectedActor == null)
		{
			actorPropertiesWindowVisible = false;
			selectedActor = null;
		}
		ImGui.End();
	}

	public override void Render()
	{
		if (Game.Instance.Scene is World world && world.Get<Player>() is { } player)
		{
			RenderActorListWindow(GetActorTypesInWorld(world));
			RenderActorTypeListWindow(world);
			RenderActorPropertiesWindow(world, player);
		}
	}
}
