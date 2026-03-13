using Godot;
using System;
using Mushroom;
using Mushroom.Data;

public partial class CellSpawner : Node
{
	private CellBase _selectedCell;
	public void _OnCellSelected(int index)
	{
		var cellType = CellList.Cells[index];
		
		GD.Print($"{cellType.Name} selected");
		
		object obj = Activator.CreateInstance(cellType);

		if (obj is not CellBase cell)
		{
			GD.Print($"obj is not CellBase. index: {index}");
			return;
		}

		_selectedCell = cell;
	}

	public void _OnGridClick(Vector2I position)
	{
		if (_selectedCell is null)
		{
			GD.Print("selected cell is null");
			return;
		}
		
		Grid.Set(position, _selectedCell);
	}
	
	
}
