using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Humanizer;
using Mushroom.Ceils;
using Mushroom.Data;

public partial class CellList : ItemList
{
	[Export] public int IconSize = 8;

	[Signal] public delegate void CellSelectedEventHandler(int index);
	
	private Dictionary<string, CellInfo> _cells = new();

	public static Dictionary<int, Type> Cells = new();
	
	public override void _Ready()
	{
		var assembly = typeof(Air).Assembly;
		var cells = assembly
			.GetTypes()
			.Where(x => x.IsClass 
			            && !x.IsAbstract 
			            && x.IsSubclassOf(typeof(CellBase))
			            && x.GetConstructor(Type.EmptyTypes) != null)
			.ToList();

		foreach (var cell in cells)
		{
			object inst = Activator.CreateInstance(cell);
			if (inst is not CellBase cellBase)
			{
				GD.PushWarning($"{cell.Name} from {cell.Namespace} is not inhered by CellBase");
				continue;
			}

			var color = cellBase.GetUiColor();
			var icon = CreateIcon(color);
			_cells[cell.Name.Humanize(LetterCasing.Title)] = new CellInfo(icon, cell);
			
			int index = AddItem(cell.Name.Humanize(LetterCasing.Title), icon);

			Cells[index] = cell;
		}
	}
	
	// It's kinda wierd but maybe in the future godot api changes something idk
	public void _OnItemSelected(int index)
	{
		GD.Print("Selected " + index);
		EmitSignalCellSelected(index);
	}

	private ImageTexture CreateIcon(Color color)
	{
		var img = Image.CreateEmpty(IconSize, IconSize, false, Image.Format.Rgb8);
		img.Fill(color);
		
		return ImageTexture.CreateFromImage(img);
	}
	
	private record CellInfo(ImageTexture Icon, Type Type);
}


