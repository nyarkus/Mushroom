using Godot;
using System;
using Mushroom;

public partial class GridMouseHandler : Node
{
	[Signal]
	public delegate void GridClickEventHandler(Vector2I position);

	private Vector2I _mousePos;
	private bool _isPressed => Input.IsMouseButtonPressed(MouseButton.Left);
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } button && button.IsPressed())
		{
			Vector2 localPos = button.Position - Render.Instance.RenderSprite.Position;
                
			int gridX = Mathf.FloorToInt(localPos.X / Render.Instance.RenderSprite.Scale.X);
			int gridY = Mathf.FloorToInt(localPos.Y / Render.Instance.RenderSprite.Scale.Y);

			if (Grid.IsInBounds(gridX, gridY))
				_mousePos = new Vector2I(gridX, gridY);
		}
		else if (@event is InputEventMouseMotion motion && _isPressed)
		{
			Vector2 localPos = motion.Position - Render.Instance.RenderSprite.Position;
                
			int gridX = Mathf.FloorToInt(localPos.X / Render.Instance.RenderSprite.Scale.X);
			int gridY = Mathf.FloorToInt(localPos.Y / Render.Instance.RenderSprite.Scale.Y);

			if (Grid.IsInBounds(gridX, gridY))
				_mousePos = new Vector2I(gridX, gridY);
		}
	}

	public override void _Process(double delta)
	{
		if(!_isPressed)
			return;
		
		EmitSignalGridClick(_mousePos);
	}
}
