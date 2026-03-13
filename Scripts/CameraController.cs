using Godot;

namespace Mushroom.Controls;

public partial class CameraController : Camera2D
{
    [Export] public float DefaultZoom = 8f;
    private bool isDragging;

    public override void _Ready()
    {
        ResetPosition();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
            {
                isDragging = mouseButtonEvent.IsPressed();
                if (mouseButtonEvent.IsDoubleClick())
                    ResetPosition();
            }
            
            if (mouseButtonEvent.IsPressed())
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp)
                    Zoom *= 1.1f;
                else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelDown)
                    Zoom *= 0.9f;
            }
            
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.IsPressed())
            {
                Vector2 localPos = mouseButtonEvent.Position - Render.Instance.RenderSprite.Position;
                
                int gridX = Mathf.FloorToInt(localPos.X / Render.Instance.RenderSprite.Scale.X);
                int gridY = Mathf.FloorToInt(localPos.Y / Render.Instance.RenderSprite.Scale.Y);
                
                // if (Grid.IsInBounds(gridX, gridY))
                //     EmitSignalGridClick(gridX, gridY);
            }
        }
        else if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            if (isDragging)
                Position -= mouseMotionEvent.Relative * 0.1f;
            
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                Vector2 localPos = mouseMotionEvent.Position - Render.Instance.RenderSprite.Position;
                
                int gridX = Mathf.FloorToInt(localPos.X / Render.Instance.RenderSprite.Scale.X);
                int gridY = Mathf.FloorToInt(localPos.Y / Render.Instance.RenderSprite.Scale.Y);
                
                // if (Grid.IsInBounds(gridX, gridY))
                //     EmitSignalGridClick(gridX, gridY);
            }
        }
    }

    public void ResetPosition()
    {
        float x = Render.Instance.RenderSprite.Scale.X * Grid.Size.X / 2f;
        float y = Render.Instance.RenderSprite.Scale.Y * Grid.Size.Y / 2;

        Zoom = new Vector2(DefaultZoom, DefaultZoom);
        Position = new Vector2(x, y);
    }
}