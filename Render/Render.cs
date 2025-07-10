using Godot;
using Mushroom;
using Mushroom.Data;

namespace Mushroom;

public partial class Render : Node2D
{
	[Export]
	private float CellSize = 8.0f;
	
	private MultiMeshInstance2D multiMeshInstance;
	private MultiMesh multiMesh;
	private QuadMesh quadMesh;
	public void Initialize()
	{
		multiMeshInstance = GetNode<MultiMeshInstance2D>("MultiMeshInstance2D");
		multiMesh = multiMeshInstance.Multimesh;
		
		multiMesh.InstanceCount = Grid.Size.X * Grid.Size.Y;
		
		quadMesh = multiMesh.Mesh as QuadMesh;
		if (quadMesh != null)
		{
			quadMesh.Size = new Vector2(CellSize, CellSize);
		}
	}
	
	public void RenderFrame()
	{
		for (int y = 0; y < Grid.Size.Y; y++)
		{
			for (int x = 0; x < Grid.Size.X; x++)
			{
				int index = y * Grid.Size.X + x;
				
				var transform = new Transform2D(0, new Vector2(x * CellSize, y * CellSize));
				multiMesh.SetInstanceTransform2D(index, transform);

				Color cellColor;
				var cell = Grid.Get(x, y);

				cellColor = Color.FromString(cell.GetColor(new Position(x,y)), Colors.Magenta); 
				
				multiMesh.SetInstanceColor(index, cellColor);
			}
		}
	}

	private bool isDragging;
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButtonEvent)
		{
			if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
			{
				isDragging = mouseButtonEvent.IsPressed();
				if (mouseButtonEvent.IsDoubleClick())
				{
					CellSize = 8f;
					multiMeshInstance.Position = new Vector2(0, 0);
				}
			}
				
		}
		
		if (@event is InputEventMouseMotion mouseMotionEvent)
		{
			if (isDragging)
				multiMeshInstance.Position += mouseMotionEvent.Relative;
			
		}

		if (@event is InputEventMouseButton mouseWheelEvent && mouseWheelEvent.IsPressed())
		{
			if (mouseWheelEvent.ButtonIndex == MouseButton.WheelUp)
			{
				CellSize += 0.5f;
				quadMesh.Size = new Vector2(CellSize, CellSize);
			}
			
			if (mouseWheelEvent.ButtonIndex == MouseButton.WheelDown)
			{
				CellSize -= 0.5f;
				if (CellSize < 0.5f)
					CellSize = 0.5f;
				
				quadMesh.Size = new Vector2(CellSize, CellSize);
			}
		}
	}
}
