using System.Threading.Tasks;
using Godot;
using Mushroom.Data;

namespace Mushroom;

public partial class Render : Node2D
{
    [Export]
    private float CellSize = 8.0f;
    
    private MultiMeshInstance2D multiMeshInstance;
    private MultiMesh multiMesh;
    private QuadMesh quadMesh;
    
    private Color[] _calculatedColors;
    private Color[] _lastAppliedColors;
    
    private int _gridWidth;
    private int _gridHeight;
    private int _totalCells;

    public void Initialize()
    {
        multiMeshInstance = GetNode<MultiMeshInstance2D>("MultiMeshInstance2D");
        multiMesh = multiMeshInstance.Multimesh;
        
        _gridWidth = Grid.Size.X;
        _gridHeight = Grid.Size.Y;
        _totalCells = _gridWidth * _gridHeight;

        multiMesh.InstanceCount = _totalCells;
        
        quadMesh = multiMesh.Mesh as QuadMesh;
        if (quadMesh != null)
        {
            quadMesh.Size = new Vector2(CellSize, CellSize);
        }
        
        _calculatedColors = new Color[_totalCells];
        _lastAppliedColors = new Color[_totalCells];

        ResetPosition();
        UpdateTransforms();
    }
    
    public void RenderFrame()
    {
        Parallel.For(0, _totalCells, i =>
        {
            int x = i % _gridWidth;
            int y = i / _gridWidth;
            
            var cell = Grid.Get(x, y);
            _calculatedColors[i] = //cell.GetColor(new Vector2I(x, y));
                cell.CachedColor;
        });
        
        for (int i = 0; i < _totalCells; i++)
        {
            if (_lastAppliedColors[i] != _calculatedColors[i])
            {
                multiMesh.SetInstanceColor(i, _calculatedColors[i]);
                _lastAppliedColors[i] = _calculatedColors[i];
            }
        }
    }
    
    private void UpdateTransforms()
    {
        if (quadMesh != null)
            quadMesh.Size = new Vector2(CellSize, CellSize);

        for (int i = 0; i < _totalCells; i++)
        {
            int x = i % _gridWidth;
            int y = i / _gridWidth;
            
            var transform = new Transform2D(0, new Vector2(x * CellSize, y * CellSize));
            multiMesh.SetInstanceTransform2D(i, transform);
        }
    }

    private void ResetPosition()
    {
        CellSize = 8f;
        var windowSize = GetWindow().Size;
        
        multiMeshInstance.Position = new Vector2(
            (float)windowSize.X / _gridWidth * CellSize * 2, 
            (float)windowSize.Y / _gridHeight * CellSize
        );
        UpdateTransforms();
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
                    ResetPosition();
            }
            
            if (mouseButtonEvent.IsPressed())
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp)
                {
                    multiMeshInstance.Scale *= 1.1f;
                }
                else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelDown)
                {
                    multiMeshInstance.Scale *= 0.9f;
                    multiMeshInstance.Scale.Clamp(new Vector2(0.5f, 0.5f), new Vector2(1f, 1f));
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            if (isDragging)
                multiMeshInstance.Position += mouseMotionEvent.Relative;
        }
    }
}