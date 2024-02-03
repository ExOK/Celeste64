
using System.Reflection.Emit;
using static Celeste64.Menu;

namespace Celeste64;

public class Menu
{
	public const float Spacing = 4 * Game.RelativeScale;
	public const float SpacerHeight = 12 * Game.RelativeScale;

	public abstract class Item
	{
		public virtual string Label { get; } = string.Empty;
		public virtual bool Selectable { get; } = true;
        public virtual bool Pressed() => false;
		public virtual void Slide(int dir) {}
	}

	public class Submenu(string label, Menu mainMenu, Menu? submenu = null) : Item 
	{
		private readonly string label = label;
		public override string Label => label;
		public override bool Pressed() {
			if (submenu != null) {
				Audio.Play(Sfx.ui_select);
				mainMenu.Index = 0;
				mainMenu.Title = submenu.Title;
				mainMenu.addItemsToStack(submenu.getCurrentItems());
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

	public int Index
	{
		get => index;
		set => index = value;
	}

    public string Title {
        get => title;
        set => title = value;
    }

    public bool Focused = true;

	private readonly SpriteFont font;
    //Stack to keeps track of all menus before current menu
    private readonly Stack<List<Item>> menuStack = new Stack<List<Item>>();
    private int index = 0;
	private string title = string.Empty;

	public string UpSound = Sfx.ui_move;
	public string DownSound = Sfx.ui_move;

    protected List<Item> getCurrentItems() 
	{
        return menuStack.Peek();
    }

    protected void addItemsToStack(List<Item> items) 
	{
        menuStack.Push(items);
    }

    public bool isInMainMenu() 
	{
        return menuStack.Count == 1;
    }

    public void closeSubmenus() 
	{
        while (menuStack.Count > 1) { menuStack.Pop(); }
    }

    public Vec2 Size
	{
		get
		{
			Vec2 size = Vec2.Zero;

			if (!string.IsNullOrEmpty(title)) {
                size.X = font.WidthOf(title);
                size.Y += font.LineHeight;
				size.Y += SpacerHeight + Spacing;
            }

                foreach (var item in getCurrentItems())
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

			if (getCurrentItems().Count > 0)
				size.Y -= Spacing;

			return size;
		}
	}

	public Menu()
	{
		font = Assets.Fonts.First().Value;
		menuStack.Push(new List<Item>());
	}

	public Menu Add(Item item)
	{
        getCurrentItems().Add(item);
		return this;
	}

	public void Update()
	{
		if (getCurrentItems().Count > 0 && Focused)
		{
			var was = index;
			var step = 0;
			if (Controls.Menu.Vertical.Positive.Pressed)
				step = 1;
			if (Controls.Menu.Vertical.Negative.Pressed)
				step = -1;

			index += step;
			while (!getCurrentItems()[(getCurrentItems().Count + index) % getCurrentItems().Count].Selectable)
				index += step;
			index = (getCurrentItems().Count + index) % getCurrentItems().Count;

			if (was != index)
				Audio.Play(step < 0 ? UpSound : DownSound);

			if (Controls.Menu.Horizontal.Negative.Pressed)
                getCurrentItems()[index].Slide(-1);
			if (Controls.Menu.Horizontal.Positive.Pressed)
                getCurrentItems()[index].Slide(1);

			if (Controls.Confirm.Pressed && getCurrentItems()[index].Pressed())
				Controls.Consume();
            if (Controls.Cancel.Pressed && !isInMainMenu()) 
			{
                Audio.Play(Sfx.main_menu_toggle_off);
				Index = 0;
				menuStack.Pop();
			}
        }
	}

	public void Render(Batcher batch, Vec2 position)
	{
		var size = Size;
		batch.PushMatrix(-size / 2);

		if(!string.IsNullOrEmpty(title)) 
		{
            var at = position + new Vec2(size.X / 2, 0);
            var text = title;
            var justify = new Vec2(0.5f, 0);
            var color = new Color(8421504);

            UI.Text(batch, text, at, justify, color);

            position.Y += font.LineHeight;
            position.Y += SpacerHeight + Spacing;
        }

		for (int i = 0; i < getCurrentItems().Count; i ++)
		{
			if (string.IsNullOrEmpty(getCurrentItems()[i].Label))
			{
				position.Y += SpacerHeight;
				continue;
			}

			var at = position + new Vec2(size.X / 2, 0);
			var text = getCurrentItems()[i].Label;
			var justify = new Vec2(0.5f, 0);
			var color = index == i && Focused ? (Time.BetweenInterval(0.1f) ? 0x84FF54 : 0xFCFF59) : Color.White;
			
			UI.Text(batch, text, at, justify, color);

			position.Y += font.LineHeight;
			position.Y += Spacing;    
        }
		batch.PopMatrix();
	}
}