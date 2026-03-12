using System.Threading.Tasks;
using Godot;
using Mushroom.Data;

namespace Mushroom;

public partial class Render : Node2D
{
    [Export] public float SpriteScale = 8f;
    
    private Sprite2D _sprite;
    private Image _image;
    private ImageTexture _texture;
    private byte[] _pixelData;
    
    private int _gridWidth;
    private int _gridHeight;
    private int _totalCells;

    public void Initialize()
    {
        _gridWidth = Grid.Size.X;
        _gridHeight = Grid.Size.Y;
        _totalCells = _gridWidth * _gridHeight;
        
        _pixelData = new byte[_totalCells * 4]; // 4 bytes per pixel
        
        _image = Image.CreateEmpty(_gridWidth, _gridHeight, false, Image.Format.Rgba8);
        _texture = ImageTexture.CreateFromImage(_image);
        
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _sprite.Texture = _texture;
        
        _sprite.Centered = false;
        _sprite.TextureFilter = TextureFilterEnum.Nearest;

        ResetPosition();
    }
    
    public void RenderFrame()
    {
        Parallel.For(0, _totalCells, i =>
        {
            int x = i % _gridWidth;
            int y = i / _gridWidth;
            
            var cell = Grid.Get(x, y);
            Color c = cell.CachedColor;
            
            int byteIdx = i * 4;
            _pixelData[byteIdx] = (byte)(c.R * 255f); // R
            _pixelData[byteIdx + 1] = (byte)(c.G * 255f); // G
            _pixelData[byteIdx + 2] = (byte)(c.B * 255f); // B
            _pixelData[byteIdx + 3] = (byte)(c.A * 255f); // A
        });
        
        _image.SetData(_gridWidth, _gridHeight, false, Image.Format.Rgba8, _pixelData);
        _texture.Update(_image);
    }

    private void ResetPosition()
    {
        _sprite.Scale = new Vector2(SpriteScale, SpriteScale);

        var windowSize = GetWindow().Size;
        
        float actualGridWidth = _gridWidth * SpriteScale;
        float actualGridHeight = _gridHeight * SpriteScale;

        float centerX = (windowSize.X - actualGridWidth) / 2;
        float centerY = (windowSize.Y - actualGridHeight) / 2;

        _sprite.Position = new Vector2(centerX, centerY);
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
                float scaleFactor = 1f;

                if (mouseButtonEvent.ButtonIndex == MouseButton.WheelUp)
                    scaleFactor = 1.1f;
                else if (mouseButtonEvent.ButtonIndex == MouseButton.WheelDown)
                    scaleFactor = 0.9f;

                Vector2 mousePos = mouseButtonEvent.Position;
                _sprite.Position = mousePos - (mousePos - _sprite.Position) * scaleFactor;
                _sprite.Scale *= scaleFactor;
            }
        }
        else if (@event is InputEventMouseMotion mouseMotionEvent)
        {
            if (isDragging)
                _sprite.Position += mouseMotionEvent.Relative;
        }
    }
}