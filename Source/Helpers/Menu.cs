
namespace Celeste64;

public class Menu
{
	public static float Spacing => 4 * Game.RelativeScale;
	public static float SpacerHeight => 12 * Game.RelativeScale;
	public const float TitleScale = 0.75f;

	public abstract class Item
	{
		public virtual string Label { get; } = string.Empty;
		public virtual bool Selectable { get; } = true;
		public virtual bool Pressed() => false;
		public virtual void Slide(int dir) {}

		public string Description { get; set; } = "";

		public Item Describe(string str)
		{
			this.Description = str;

			return this;
		}
	}

	public class Submenu(string label, Menu? rootMenu, Menu? submenu = null) : Item 
	{
		private readonly string label = label;
		public override string Label => label;
		public override bool Pressed() 
		{
			if (submenu != null) 
			{
				Audio.Play(Sfx.ui_select);
				submenu.Index = 0;
				rootMenu?.PushSubMenu(submenu);
				submenu.Initialized();
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
		private readonly List<string> labels = [];
		private readonly int min;
		private readonly int max;
		private readonly Func<int> get;
		private readonly Action<int> set;
	
		public Slider(string label, int min, int max, Func<int> get, Action<int> set)
		{
			for (int i = 0, n = (max - min); i <= n; i ++)
				labels.Add($"{label} [{new string('|', i)}{new string('.', n - i)}]");
			this.min = min;
			this.max = max;
			this.get = get;
			this.set = set;
		}

        public override string Label => labels[get() - min];
        public override void Slide(int dir) => set(Calc.Clamp(get() + dir, min, max));
    }

	public class OptionList : Item
	{
		private readonly string label;
		private readonly int min;
		private readonly Func<string> get;
		private readonly Func<int> getMax;
		private readonly Func<List<string>> getLabels;
		private readonly Action<string> set;

		public OptionList(string label, Func<List<string>> getLabels, Func<string> get, Action<string> set)
		{
			this.label = label;
			this.getLabels = getLabels;
			this.min = 0;
			this.getMax = () => getLabels().Count;
			this.get = get;
			this.set = set;
		}

		public OptionList(string label, Func<List<string>> getLabels, int min, Func<int> getMax, Func<string> get, Action<string> set)
		{
			this.label = label;
			this.getLabels = getLabels;
			this.min = min;
			this.getMax = getMax;
			this.get = get;
			this.set = set;
		}

		public override string Label => $"{label} : {getLabels()[getId() - min]}";
		public override void Slide(int dir) 
		{
			if(getLabels().Count > 1)
			{
				set(getLabels()[(getMax() + getId() + dir) % getMax()]);
			}
		}

		private int getId()
		{
			int id = getLabels().IndexOf(get());
			return id > -1 ? id : 0;
		}
    }

    public class Option(string label, Action? action = null) : Item
	{
		private readonly string label = label;
		private readonly Action? action = action;
        public override string Label => label;
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

	public class Toggle(string label, Action action, Func<bool> get)  : Item
	{
		private readonly string labelOff = $"{label} : OFF";
		private readonly string labelOn  = $"{label} :  ON";
		private readonly Action action = action;
        public override string Label => get() ? labelOn : labelOff;
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

	public class MultiSelect(string label, List<string> options, Func<int> get, Action<int> set) : Item
	{
		private readonly List<string> options = options;
		private readonly Action<int> set = set;
		public override string Label => $"{label} : {options[get()]}";

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
	}

	public int Index;
	public string Title = string.Empty;
	public bool Focused = true;

	protected readonly List<Item> items = [];
	protected readonly Stack<Menu> submenus = [];

	public string UpSound = Sfx.ui_move;
	public string DownSound = Sfx.ui_move;

	public bool IsInMainMenu => submenus.Count <= 0;
	protected Menu CurrentMenu => GetDeepestActiveSubmenu(this);

	protected virtual int maxItemsCount { get; set; } = 12;
	protected int scrolledAmount = 0;
	protected bool showScrollbar = true;

	public Menu GetDeepestActiveSubmenu(Menu target)
	{
		if (target.submenus.Count <= 0)
		{
			return target;
		} else {
			return GetDeepestActiveSubmenu(target.submenus.Peek());
		}
	}

	public Menu GetSecondDeepestMenu(Menu target)
	{
		if (target.submenus.Peek() != null && target.submenus.Peek().submenus.Count <= 0)
		{
			return target;
		} else {
			return GetSecondDeepestMenu(target.submenus.Peek());
		}
	}
	
	public Vec2 Size
	{
		get
		{
			var size = Vec2.Zero;
			var font = Language.Current.SpriteFont;
	
			if (!string.IsNullOrEmpty(Title))
			{
				size.X = font.WidthOf(Title) * TitleScale;
				size.Y += font.HeightOf(Title) * TitleScale;
				size.Y += SpacerHeight + Spacing;
			}
	
			for (int i = scrolledAmount; i < items.Count && i < scrolledAmount + maxItemsCount; i++)
			{
				if (string.IsNullOrEmpty(items[i].Label))
				{
					size.Y += SpacerHeight;
				}
				else
				{
					size.X = MathF.Max(size.X, font.WidthOf(items[i].Label));
					size.Y += font.LineHeight;
				}
				size.Y += Spacing;
			}
	
			if (items.Count > 0)
				size.Y -= Spacing;
	
			return size;
		}
	}

	public virtual void Initialized()
	{

	}
	
	public Menu Add(Item item)
	{
		items.Add(item);
		return this;
	}
	
	internal void PushSubMenu(Menu menu) 
	{
		submenus.Push(menu);
	}

	internal void PopSubMenu()
	{
		submenus.Pop();
	}

	public void CloseSubMenus() 
	{
		submenus.Clear();
	}

	protected virtual void HandleInput()
	{
		if (items.Count > 0)
		{
			var was = Index;
			var step = 0;

			if (Controls.Menu.Vertical.Positive.Pressed)
				step = 1;
			if (Controls.Menu.Vertical.Negative.Pressed)
				step = -1;
	
			Index += step;
			while (!items[(items.Count + Index) % items.Count].Selectable)
				Index += step;
			Index = (items.Count + Index) % items.Count;

			if(items.Count > maxItemsCount)
			{
				if (Index >= scrolledAmount + (maxItemsCount - 3))
				{
					scrolledAmount = Index - (maxItemsCount - 3);
				}
				else if (Index < scrolledAmount + 2)
				{
					scrolledAmount = Index - 2;
				}
				scrolledAmount = Math.Clamp(scrolledAmount, 0, items.Count - maxItemsCount);
			}

			if (was != Index)
				Audio.Play(step < 0 ? UpSound : DownSound);
	
			if (Controls.Menu.Horizontal.Negative.Pressed)
				items[Index].Slide(-1);
			if (Controls.Menu.Horizontal.Positive.Pressed)
				items[Index].Slide(1);
	
			if (Controls.Confirm.Pressed && items[Index].Pressed())
				Controls.Consume();
		}
	}

	public void Update()
	{
		if (Focused)
		{
			CurrentMenu.HandleInput();

	        if (!IsInMainMenu && Controls.Cancel.ConsumePress()) 
			{
				Audio.Play(Sfx.main_menu_toggle_off);
				GetSecondDeepestMenu(this).submenus.Pop();
			}
	    }
	}

	protected virtual void RenderItems(Batcher batch)
	{
		var font = Language.Current.SpriteFont;
		var size = Size;
		var position = Vec2.Zero;
		batch.PushMatrix(new Vec2(0, -size.Y / 2));
	
		if(!string.IsNullOrEmpty(Title)) 
		{
			var text = Title;
			var justify = new Vec2(0.5f, 0);
			var color = new Color(8421504);

			batch.PushMatrix(
				Matrix3x2.CreateScale(TitleScale) * 
				Matrix3x2.CreateTranslation(position));
			UI.Text(batch, text, Vec2.Zero, justify, color);
			batch.PopMatrix();

			position.Y += font.HeightOf(Title) * TitleScale;
			position.Y += SpacerHeight + Spacing;
		}
	
		for (int i = scrolledAmount; i < items.Count && i < scrolledAmount + maxItemsCount; i ++)
		{
			if (string.IsNullOrEmpty(items[i].Label))
			{
				position.Y += SpacerHeight;
				continue;
			}
	
			var text = items[i].Label;
			var justify = new Vec2(0.5f, 0);
			var color = Index == i && Focused ? (Time.BetweenInterval(0.1f) ? 0x84FF54 : 0xFCFF59) : Color.White;
			
			UI.Text(batch, text, position, justify, color);
	
			position.Y += font.LineHeight;
			position.Y += Spacing;    
	    }
		batch.PopMatrix();

		// Render a scrolbar if there are too many items to show on screen at once
		if(showScrollbar && items.Count > maxItemsCount)
		{
			// TODO: This will need to be redone if we implement mouse support and want it to interact with menus.
			int padding = 4;
			int scrollSize = 16;
			int xPos = Game.Width - scrollSize - padding;
			int scrollBarHeight = Game.Height - (scrollSize * 2) - padding * 4;
			int scrollStartPos = padding * 2 + scrollSize;
			batch.PushMatrix(Vec2.Zero, false);
			batch.Rect(new Rect(xPos, padding, scrollSize, scrollSize), Color.White);
			batch.Rect(new Rect(xPos, scrollStartPos, scrollSize, scrollBarHeight), Color.Gray);
			int scrollYPos = (int)MathF.Ceiling(scrollStartPos + ((float)scrolledAmount * scrollBarHeight / items.Count));
			int scrollYHeight = scrollBarHeight * maxItemsCount / items.Count;
			batch.Rect(new Rect(xPos, scrollYPos, scrollSize, scrollYHeight), Color.White);
			batch.Rect(new Rect(xPos, Game.Height - scrollSize - padding, scrollSize, scrollSize), Color.White);
			batch.PopMatrix();
		}
	}

	public virtual void Render(Batcher batch, Vec2 position)
	{
		batch.PushMatrix(position);
		CurrentMenu.RenderItems(batch);
		batch.PopMatrix();

		// let's not crash if the menu has no items
		// such as the modselectionmenu
		if(CurrentMenu.items.Count > 0)
		{
			var currentItem = CurrentMenu.items[CurrentMenu.Index];

			var text = currentItem.Description;
			var justify = new Vec2(0.5f, -8f);
			var color = Color.Gray;
				
			UI.Text(batch, text, position, justify, color);
		}
	}
}