using System.Runtime.CompilerServices;

namespace Celeste64;

/// <summary>
/// 2D Grid Partition used to split up solids (or anything else)
/// Doesn't split on the Z axis - only across X/Y, since most of the terrain
/// is spread out on a flat plane.
/// </summary>
public class GridPartition<T>
{
	private readonly List<T>[] grid;
	private readonly int columns;
	private readonly int rows;
	private readonly int gridsize;

	public GridPartition(int cellSize, int gridCount)
	{
		gridsize = cellSize;
		columns = gridCount;
		rows = gridCount;
		grid = new List<T>[gridCount * gridCount];
		for (int i = 0; i < grid.Length; i ++)
			grid[i] = new();
	}

	public void Insert(in T instance, in Rect bounds)
	{
		var b = Cells(bounds);
		for (int x = b.X; x < b.X + b.Width; x ++)
		for (int y = b.Y; y < b.Y + b.Height; y ++)
			Cell(x, y).Add(instance);
	}

	public void Remove(in T instance, in Rect bounds)
	{
		var b = Cells(bounds);
		for (int x = b.X; x < b.X + b.Width; x ++)
		for (int y = b.Y; y < b.Y + b.Height; y ++)
			Cell(x, y).Remove(instance);
	}

	public void Query(List<T> populate, in Rect bounds)
	{
		var already = Pool.Get<HashSet<T>>();
		already.Clear();

		var b = Cells(bounds);
		for (int x = b.X; x < b.X + b.Width; x ++)
		for (int y = b.Y; y < b.Y + b.Height; y ++)
		{
			foreach (var it in Cell(x, y))
			{
				if (!already.Contains(it))
				{
					populate.Add(it);
					already.Add(it);
				}
			}
		}

		Pool.Return(already);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private List<T> Cell(int x, int y)
	{
		x = (columns + (x % columns)) % columns;
		y = (rows + (y % rows)) % rows;
		return grid[x + y * columns];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private RectInt Cells(Rect bounds)
	{
		bounds = bounds.Inflate(1);
		int left = (int)MathF.Floor(bounds.X / gridsize);
		int top = (int)MathF.Floor(bounds.Y / gridsize);
		int right = (int)MathF.Ceiling((bounds.X + bounds.Width) / gridsize);
		int bottom = (int)MathF.Ceiling((bounds.Y + bounds.Height) / gridsize);
		return new(left, top, right - left, bottom - top);
	}
}