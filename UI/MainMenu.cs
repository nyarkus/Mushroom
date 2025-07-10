using Godot;
using System;
using System.Threading.Tasks;
using Mushroom.Data;
using Mushroom.Generation;

public partial class MainMenu : Control
{
    private int _worldSizeIndex;
    private ItemList _worldSize;
    private Panel _panel;
    private SpinBox _seed;
    
    private Control _loading;
    private ProgressBar _progressBar;

    public override void _Ready()
    {
        _worldSize = GetNode<ItemList>("CenterContainer/Panel/VBoxContainer/WorldSize");
        _seed = GetNode<SpinBox>("CenterContainer/Panel/VBoxContainer/Seed");
        _panel = GetNode<Panel>("CenterContainer/Panel");
        
        _loading = GetNode<Control>("CenterContainer/Loading");
        _progressBar = GetNode<ProgressBar>("CenterContainer/Loading/ProgressBar");
    }
    public void Generate()
    {
        Position worldSize = _worldSize.GetItemText(_worldSizeIndex) switch
        {
            "Small" => new Position(80, 40),
            "Medium" => new Position(160, 80),
            "Large" => new Position(240, 120),
            "Very Large" => new Position(320, 160),
            "Colossal" => new Position(480, 200),
            "Titanic" => new Position(640, 240),
            _ => throw new ArgumentOutOfRangeException()
        };

        _panel.Visible = false;
        _loading.Visible = true;
        
        var progress = new Progress<float>(f => _progressBar.SetDeferred(ProgressBar.PropertyName.Value, f));
        Generator.Progress = progress;

        _ = Task.Run(() =>
        {
            Generator.Generate(worldSize, 0, (int)_seed.Value);
            var game = ResourceLoader.Load<PackedScene>("res://Game.tscn").Instantiate();
            GetTree().Root.CallDeferred(Node.MethodName.AddChild, game);
            
            QueueFree();
        });
    }

    public void WorldSizeSelected(int index)
        => _worldSizeIndex = index;
}
