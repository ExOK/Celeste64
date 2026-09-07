
namespace Celeste64;

public class Menu(Controls controls)
{
	public const float Spacing = 4 * Game.RelativeScale;
	public const float SpacerHeight = 12 * Game.RelativeScale;
	public const float TitleScale = 0.75f;

	public abstract class Item
	{
		public virtual string Label { get; } = string.Empty;
		public virtual bool Selectable { get; } = true;
		public virtual bool Pressed() => false;
		public virtual void Slide(int dir) {}
	}

	public class Submenu(Func<string> label, Menu? rootMenu, Menu? submenu = null) : Item 
	{
		public Submenu(string label, Menu? rootMenu, Menu? submenu = null) : this(() => label, rootMenu, submenu) {}
		public override string Label => label();
		public override bool Pressed() 
		{
			if (submenu != null) 
			{
				Audio.Play(Sfx.ui_select);
				submenu.Index = 0;
				rootMenu?.PushSubMenu(submenu);
				return true;
			}
			
			return false;
		}
	}

    public class Spacer : Item
	{
        public override bool Selectable => false;
    }
	
	public class Slider: Item
	{
		private readonly Func<string> label;
		private readonly int min;
		private readonly int max;
		private readonly Func<int> get;
		private readonly Action<int> set;
	
		public Slider(Func<string> label, int min, int max, Func<int> get, Action<int> set)
		{
			this.label = label;
			this.min = min;
			this.max = max;
			this.get = get;
			this.set = set;
		}
		public Slider(string label, int min, int max, Func<int> get, Action<int> set) : this(() => label, min, max, get, set) {}
	
		public override string Label => $"{label()} [{new string('|', get() - min)}{new string('.', max - get())}]";
        public override void Slide(int dir) => set(Calc.Clamp(get() + dir, min, max));
    }

	public class Option(Func<string> label, Action? action = null) : Item
	{
		private readonly Action? action = action;
		public Option(string label, Action? action = null) : this(() => label, action) {}
		public override string Label => label();
        public override bool Pressed()
		{
			if (action != null)
			{
				Audio.Play(Sfx.ui_select);
				action();
				return true;
			}
			return false;
		}
    }

	public class Toggle(Func<string> label, Action action, Func<bool> get)  : Item
	{
		private readonly Action action = action;
		public Toggle(string label, Action action, Func<bool> get) : this(() => label, action, get) {}
		public override string Label => $"{label()} : {(get() ? " ON" : "OFF")}";
        public override bool Pressed()
		{
			action();
			if (get())
				Audio.Play(Sfx.main_menu_toggle_on);
			else
				Audio.Play(Sfx.main_menu_toggle_off);
			return true;
		}
	}

	public class MultiSelect(Func<string> label, List<string> options, Func<int> get, Action<int> set) : Item
	{
		private readonly List<string> options = options;
		private readonly Action<int> set = set;
		public MultiSelect(string label, List<string> options, Func<int> get, Action<int> set) : this(() => label, options, get, set) {}
		public override string Label => $"{label()} : {options[get()]}";

		public override void Slide(int dir) 
		{
			Audio.Play(Sfx.ui_select);

			int index = get();
			if (index < options.Count() - 1 && dir == 1)
				index++;
			if (index > 0 && dir == -1)
				index--;
			set(index);
		}
	}

	public class MultiSelect<T> : MultiSelect where T : struct, Enum
	{
		private static List<string> GetEnumOptions()
		{
			var list = new List<string>();
			foreach (var it in Enum.GetNames<T>())
				list.Add(it);
			return list;
		}

		public MultiSelect(string label, Action<T> set, Func<T> get)
			: base(label, GetEnumOptions(), () => (int)(object)get(), (i) => set((T)(object)i))
		{

		}

		public MultiSelect(Func<string> label, Action<T> set, Func<T> get)
			: base(label, GetEnumOptions(), () => (int)(object)get(), (i) => set((T)(object)i))
		{

		}
	}

	public class LocalizedMultiSelect(
		Func<string> label,
		int optionCount,
		Func<int, string> optionLabel,
		Func<int> get,
		Action<int> set) : Item
	{
		public override string Label => $"{label()} : {optionLabel(get())}";

		public override void Slide(int dir)
		{
			Audio.Play(Sfx.ui_select);

			int index = get();
			if (index < optionCount - 1 && dir == 1)
				index++;
			if (index > 0 && dir == -1)
				index--;
			set(index);
		}
	}

	public readonly Controls Controls = controls;
	public int Index;
	public Func<string> Title = () => string.Empty;
	public bool Focused = true;

	private readonly List<Item> items = [];
	private readonly Stack<Menu> submenus = [];
	private Time time;

	public string UpSound = Sfx.ui_move;
	public string DownSound = Sfx.ui_move;

	public bool IsInMainMenu => submenus.Count <= 0;
	private Menu CurrentMenu => submenus.Count > 0 ? submenus.Peek() : this;
	
	public Vec2 Size
	{
		get
		{
			var size = Vec2.Zero;
			var font = Language.Current.SpriteFont;
	
			var title = Title();
			if (!string.IsNullOrEmpty(title))
			{
				size.X = font.WidthOf(title) * TitleScale;
				size.Y += font.LineHeight * TitleScale;
				size.Y += SpacerHeight + Spacing;
			}
	
			foreach (var item in items)
			{
				if (string.IsNullOrEmpty(item.Label))
				{
					size.Y += SpacerHeight;
				}
				else
				{
					size.X = MathF.Max(size.X, font.WidthOf(item.Label));
					size.Y += font.LineHeight;
				}
				size.Y += Spacing;
			}
	
			if (items.Count > 0)
				size.Y -= Spacing;
	
			return size;
		}
	}

    public Menu Add(Item item)
	{
		items.Add(item);
		return this;
	}
	
	protected void PushSubMenu(Menu menu) 
	{
		submenus.Push(menu);
	}
	
	public void CloseSubMenus() 
	{
		submenus.Clear();
	}

	private void HandleInput()
	{
		if (items.Count > 0)
		{
			var was = Index;
			var step = 0;

			if (Controls.Menu.Down.Pressed)
				step = 1;
			if (Controls.Menu.Up.Pressed)
				step = -1;
	
			Index += step;
			while (!items[(items.Count + Index) % items.Count].Selectable)
				Index += step;
			Index = (items.Count + Index) % items.Count;
	
			if (was != Index)
				Audio.Play(step < 0 ? UpSound : DownSound);
	
			if (Controls.Menu.Left.Pressed)
				items[Index].Slide(-1);
			if (Controls.Menu.Right.Pressed)
				items[Index].Slide(1);
	
			if (Controls.Confirm.Pressed && items[Index].Pressed())
				Controls.Consume();
		}
	}

	public void Update(in Time time)
	{
		this.time = time;

		if (Focused)
		{
			CurrentMenu.HandleInput();

	        if (!IsInMainMenu && Controls.Cancel.ConsumePress()) 
			{
				Audio.Play(Sfx.main_menu_toggle_off);
				submenus.Pop();
			}
	    }
	}

	private void RenderItems(Batcher batch)
	{
		var font = Language.Current.SpriteFont;
		var size = Size;
		var position = Vec2.Zero;
		batch.PushMatrix(new Vec2(0, -size.Y / 2));
	
		var title = Title();
		if(!string.IsNullOrEmpty(title)) 
		{
			var text = title;
			var justify = new Vec2(0.5f, 0);
			var color = new Color(8421504);

			batch.PushMatrix(
				Matrix3x2.CreateScale(TitleScale) * 
				Matrix3x2.CreateTranslation(position));
			UI.Text(batch, text, Vec2.Zero, justify, color);
			batch.PopMatrix();

			position.Y += font.LineHeight * TitleScale;
			position.Y += SpacerHeight + Spacing;
		}
	
		for (int i = 0; i < items.Count; i ++)
		{
			if (string.IsNullOrEmpty(items[i].Label))
			{
				position.Y += SpacerHeight;
				continue;
			}
	
			var text = items[i].Label;
			var justify = new Vec2(0.5f, 0);
			var color = Index == i && Focused ? (time.BetweenInterval(0.1f) ? 0x84FF54 : 0xFCFF59) : Color.White;
			
			UI.Text(batch, text, position, justify, color);
	
			position.Y += font.LineHeight;
			position.Y += Spacing;    
	    }
		batch.PopMatrix();
	}
	
	public void Render(Batcher batch, Vec2 position)
	{
		batch.PushMatrix(position);
		CurrentMenu.RenderItems(batch);
		batch.PopMatrix();
	}
}
