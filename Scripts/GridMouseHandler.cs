using Godot;
using System;
using Mushroom;

public partial class GridMouseHandler : Node
{
    [Signal]
    public delegate void GridClickEventHandler(Vector2I position);

    private Vector2I _mousePos;
    private bool _isPressed = false; 
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } button)
        {
            if (button.IsPressed())
            {
                _isPressed = true;
                UpdateGridPosition();
            }
        }
        else if (@event is InputEventMouseMotion && _isPressed)
            UpdateGridPosition();
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } button && !button.IsPressed())
            _isPressed = false;
    }

    public override void _Process(double delta)
    {
        if(!_isPressed)
            return;
        
        EmitSignalGridClick(_mousePos);
    }

    private void UpdateGridPosition()
    {
        Vector2 localPos = Render.Instance.RenderSprite.GetGlobalMousePosition();
        
        int gridX = Mathf.FloorToInt(localPos.X);
        int gridY = Mathf.FloorToInt(localPos.Y);

        if (Grid.IsInBounds(gridX, gridY))
        {
            _mousePos = new Vector2I(gridX, gridY);
        }
    }
}