using System;
using Godot;
using Mushroom.Data;

namespace Mushroom;

public partial class CellSpawner : Node
{
	private Type _selectedCell;
	public void _OnCellSelected(int index)
	{
		var cellType = UI.CellList.Cells[index];
		
		GD.Print($"{cellType.Name} selected");

		_selectedCell = cellType;
	}

	public void _OnGridClick(Vector2I position)
	{
		if (_selectedCell is null)
		{
			GD.Print("selected cell is null");
			return;
		}
		
		object obj = Activator.CreateInstance(_selectedCell);

		if (obj is not CellBase cell)
		{
			GD.Print($"obj is not CellBase. index: {_selectedCell}");
			return;
		}
		
		Grid.Set(position, cell);
	}
	
	
}