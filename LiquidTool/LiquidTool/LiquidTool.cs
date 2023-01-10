using UnityEngine;
using VoxelTycoon;
using VoxelTycoon.Buildings;
using VoxelTycoon.Tools;
using VoxelTycoon.UI;

public class LiquidTool : ITool
{
	private struct State
	{
		public Xyz Origin;

		public bool Preview;

		public Xyz Target;

		public State(Xyz origin, Xyz target, bool preview)
		{
			Origin = origin;
			Target = target;
			Preview = preview;
		}
	}

	public Color color;

	private const int MaxSize = 30;

	private State? _state;

	private Xyz? _origin;

	private Vector3 _originOffset;

	private AreaSelection _selection;

	private Xyz[] _floodPositions = new Xyz[0];

	public LiquidTool (Color setColor)
    {
		color = setColor;
    }

	public void Activate()
	{
		_selection = new AreaSelection();
	}

	public bool Deactivate(bool soft)
	{
		if (soft && _origin.HasValue)
		{
			_origin = null;
			_state = null;
			return false;
		}
		_origin = null;
		_state = null;
		_selection.Destroy();
		LazyManager<ToolHintManager>.Current.Hide();
		Manager<WorldManager>.Current.ResetPreview();
		return true;
	}

	public bool OnUpdate()
	{
		if (_origin.HasValue)
		{
			Xyz target = Xyz.Round(ToolHelper.GetCursorPosition(_origin.Value.Y) - _originOffset);
			Flood(_origin.Value, target, !InputHelper.MouseUp);
		}
		else
		{
			Xyz voxelPosition = VoxelRaycaster.GetVoxelPosition();
			Flood(voxelPosition, voxelPosition, true);
			if (InputHelper.WorldMouseDown)
			{
				_origin = voxelPosition;
				_originOffset = ToolHelper.GetCursorPosition(voxelPosition.Y) - voxelPosition;
			}
		}
		return false;
	}

	private int Clamp(int value, int maxSize)
	{
		return (int)(Mathf.Min(Mathf.Abs(value), maxSize) * Mathf.Sign(value));
	}
	private int ClampHeight(int x, int z, int height, int targetHeight)
	{
		Debug.Log("1");
		if (targetHeight > height)
		{
			for (int i = height; i <= targetHeight; i++)
			{
				if (ContainsBuildingsNotPlants(new Xyz(x, i + 1, z)))
				{
					return Mathf.Min(targetHeight, i);
				}
			}
		}
		else if (targetHeight < height)
		{
			if (ContainsBuildingsNotPlants(new Xyz(x, height + 1, z)))
			{
				return height;
			}
			for (int i = height; i > targetHeight; i--)
			{
				if (ContainsBuildingsNotPlants(new Xyz(x, i, z)))
				{
					return Mathf.Max(targetHeight, i);
				}
			}
		}
		return targetHeight;
	}

	private bool ContainsBuildingsNotPlants(Xyz xyz)
    {
		int num = Mathf.Max(xyz.Y, 31);
		for (int j = xyz.Y + 1; j <= num; j++)
		{
			if (WorldManager.Current.GetBuildings(xyz) != null)
			{
				return WorldManager.Current.GetBuildings(xyz).GetType() != typeof(Plant);
			}
		}
		return false;
	}

	private void Flood(Xyz origin, Xyz target, bool preview)
	{
		State value = new State(origin, target, preview);
		if (value.Equals(_state))
		{
			return;
		}
		_state = value;
		Xz xz = (Xz)origin;
		Xz xz2 = (Xz)target;
		Xz xz3 = xz2 - xz;
		int num = MaxSize;

		xz2 = xz + new Xz(Clamp(xz3.X, num), Clamp(xz3.Z, num));
		Xz xz4 = Xz.Min(xz, xz2);
		Xz xz5 = Xz.Max(xz, xz2);
		_selection.Clear();
		Manager<WorldManager>.Current.ResetPreview();
		double num2 = 0.0;
		_floodPositions = new Xyz[0];
		Xyz xyz = target;
		for (int i = xz4.X; i <= xz5.X; i++)
		{
			for (int j = xz4.Z; j <= xz5.Z; j++)
			{
				Xz xz6 = new Xz(i, j);
				int height = Manager<WorldManager>.Current.GetHeight(xz6);
				int num3 = ClampHeight(i, j, height, origin.Y-1);
				if (xz6 == xz2)
				{
					xyz = new Xyz(i, num3, j);
				}
				bool num4 = height != num3 && Manager<RegionManager>.Current.IsUnlocked(xz6) && !WorldManager.Current.GetVoxel(new Xyz(xz6.X, WorldManager.Current.GetHeight(xz6), xz6.Z)).IsLiquid;
				Color color = R.Colors.FlattenToolNeutral.Value;
				if (num4)
				{
					num2 += ToolHelper.GetFlattenPrice(new Xyz(i, height, j), num3);
					color = ((num3 > height) ? R.Colors.FlattenToolUp.Value : R.Colors.FlattenToolDown.Value);
				}
				_selection.Select(xz6, color);
				if (num4)
				{
					_floodPositions = AddToArray(_floodPositions, new Xyz(i, num3, j));
				}
			}
		}
		Vector3 position = xyz + new Vector3(0.5f, 2f, 0.5f);
		if (preview && num2 > 0.0)
		{
			LazyManager<ToolHintManager>.Current.ShowLegacy(position, num2);
		}
		else
		{
			LazyManager<ToolHintManager>.Current.Hide();
		}
		bool flag = Company.Current.HasEnoughMoney(num2);
		bool preview2 = preview || !flag;
		foreach (Xyz flattentPosition in _floodPositions)
		{
			if (preview2) {
				Manager<WorldManager>.Current.FlattenHeight(flattentPosition, emitDust: true, preview2);
			} else {
				Xz xz6 = new Xz(flattentPosition.X, flattentPosition.Z);
				int height = Manager<WorldManager>.Current.GetHeight(xz6);
				int num3 = ClampHeight(flattentPosition.X, flattentPosition.Z, height, origin.Y);

				num = Mathf.Max(flattentPosition.Y, 31);
				for (int j = height + 1; j <= num; j++)
				{
					BuildingHelper.RemoveAllBuildings(new Xyz(flattentPosition.X, j, flattentPosition.Z));
				}

				Manager<WorldManager>.Current.FlattenHeight(new Xyz(flattentPosition.X, num3, flattentPosition.Z), emitDust: true, preview2);
				Voxel fluid = new Voxel();
				fluid.Color = color;
				fluid.IsLiquid = true;
				WorldManager.Current.SetVoxel(new Xyz(flattentPosition.X, num3, flattentPosition.Z), fluid);
            }
		}
		_selection.Rebuild();
		if (!preview)
		{
			if (num2 > 0.0)
			{
				ToolHelper.Execute(num2, BudgetItem.Buildings, position, R.Audio.Dig, R.Audio.BuildBlocked);
			}
			_origin = null;
			_state = null;
		}
	}

	public T[] AddToArray<T>(T[] array, T item)
	{
		T[] newArray = new T[array.Length + 1];
		array.CopyTo(newArray, 0);
		newArray[array.Length] = item;
		return newArray;
	}
}
