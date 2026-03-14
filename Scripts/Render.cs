using System.Threading.Tasks;
using Godot;
using Mushroom.Ceils;
using Mushroom.Data;

namespace Mushroom;

public partial class Render : Node2D
{
    public static Render Instance { get; private set; }
    
    [Export] public Sprite2D RenderSprite;
    
    private Image _image;
    private ImageTexture _texture;
    private byte[] _pixelData;
    
    private int _gridWidth;
    private int _gridHeight;
    private int _totalCells;

    public override void _Ready()
    {
        _gridWidth = Grid.Size.X;
        _gridHeight = Grid.Size.Y;
        _totalCells = _gridWidth * _gridHeight;
        
        _pixelData = new byte[_totalCells * 4]; // 4 bytes per pixel
        
        _image = Image.CreateEmpty(_gridWidth, _gridHeight, false, Image.Format.Rgba8);
        _texture = ImageTexture.CreateFromImage(_image);
        
        RenderSprite.Texture = _texture;
        
        RenderSprite.Centered = false;
        RenderSprite.TextureFilter = TextureFilterEnum.Nearest;

        Instance = this;
    }
    
    public void RenderFrame()
    {
        Parallel.For(0, _totalCells, i =>
        {
            var cell = Grid.GetRaw(i);
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

    
}