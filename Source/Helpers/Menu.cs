
using System.Reflection.Emit;
using static Celeste64.Menu;

namespace Celeste64;

public class Menu
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

	public int Index;
	public string Title = string.Empty;
	public bool Focused = true;

	private readonly SpriteFont font;
	private readonly List<Item> items = [];
	private readonly Stack<Menu> submenus = [];

	public string UpSound = Sfx.ui_move;
	public string DownSound = Sfx.ui_move;

	public bool IsInMainMenu => submenus.Count <= 0;
	private Menu CurrentMenu => submenus.Count > 0 ? submenus.Peek() : this;
	private string CurrentTitle => CurrentMenu.Title;
	private List<Item> CurrentItems => CurrentMenu.items;
	private int CurrentIndex
	{
		get => CurrentMenu.Index;
		set => CurrentMenu.Index = value;
	}
	
	public Vec2 Size
	{
		get
		{
			Vec2 size = Vec2.Zero;
	
			if (!string.IsNullOrEmpty(Title))
			{
				size.X = font.WidthOf(Title) * TitleScale;
				size.Y += font.LineHeight * TitleScale;
				size.Y += SpacerHeight + Spacing;
			}
	
			foreach (var item in CurrentItems)
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
	
			if (CurrentItems.Count > 0)
				size.Y -= Spacing;
	
			return size;
		}
	}
	
	public Menu()
	{
		font = Assets.Fonts.First().Value;
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
	    while (submenus.Count > 0)
			submenus.Pop();
	}
	
	public void Update()
	{
		if (CurrentItems.Count > 0 && Focused)
		{
			var was = CurrentIndex;
			var step = 0;

			if (Controls.Menu.Vertical.Positive.Pressed)
				step = 1;
			if (Controls.Menu.Vertical.Negative.Pressed)
				step = -1;
	
			CurrentIndex += step;
			while (!CurrentItems[(CurrentItems.Count + CurrentIndex) % CurrentItems.Count].Selectable)
				CurrentIndex += step;
			CurrentIndex = (CurrentItems.Count + CurrentIndex) % CurrentItems.Count;
	
			if (was != CurrentIndex)
				Audio.Play(step < 0 ? UpSound : DownSound);
	
			if (Controls.Menu.Horizontal.Negative.Pressed)
				CurrentItems[CurrentIndex].Slide(-1);
			if (Controls.Menu.Horizontal.Positive.Pressed)
				CurrentItems[CurrentIndex].Slide(1);
	
			if (Controls.Confirm.Pressed && CurrentItems[CurrentIndex].Pressed())
				Controls.Consume();

	        if (Controls.Cancel.Pressed && !IsInMainMenu) 
			{
				Audio.Play(Sfx.main_menu_toggle_off);
				submenus.Pop();
			}
	    }
	}
	
	public void Render(Batcher batch, Vec2 position)
	{
		var size = Size;
		batch.PushMatrix(-size / 2);
	
		if(!string.IsNullOrEmpty(CurrentTitle)) 
		{
			var at = position + new Vec2(size.X / 2, 0);
			var text = CurrentTitle;
			var justify = new Vec2(0.5f, 0);
			var color = new Color(8421504);

			batch.PushMatrix(
				Matrix3x2.CreateScale(TitleScale) * 
				Matrix3x2.CreateTranslation(at));
			UI.Text(batch, text, Vec2.Zero, justify, color);
			batch.PopMatrix();

			position.Y += font.LineHeight * TitleScale;
			position.Y += SpacerHeight + Spacing;
		}
	
		for (int i = 0; i < CurrentItems.Count; i ++)
		{
			if (string.IsNullOrEmpty(CurrentItems[i].Label))
			{
				position.Y += SpacerHeight;
				continue;
			}
	
			var at = position + new Vec2(size.X / 2, 0);
			var text = CurrentItems[i].Label;
			var justify = new Vec2(0.5f, 0);
			var color = CurrentIndex == i && Focused ? (Time.BetweenInterval(0.1f) ? 0x84FF54 : 0xFCFF59) : Color.White;
			
			UI.Text(batch, text, at, justify, color);
	
			position.Y += font.LineHeight;
			position.Y += Spacing;    
	    }
		batch.PopMatrix();
	}
}