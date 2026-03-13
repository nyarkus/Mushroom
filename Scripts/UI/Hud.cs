using Godot;
using System;

public partial class Hud : Control
{
    public RichTextLabel Info { get; set; }
    public Label SimulationFPSLabel { get; set; }

    public bool WeatherEnabled { get; set; } = true;
    public double SimulationFPS { get; set; } = 5;

    public override void _Ready()
    {
        Info = GetNode<RichTextLabel>("VBoxContainer/Panel/TabContainer/Simulation/HBoxContainer/Info");
        SimulationFPSLabel = GetNode<Label>("VBoxContainer/Panel/TabContainer/Simulation/HBoxContainer/VBoxContainer/FPS");
    }

    public void OnWeatherToggled(bool toggle)
        => WeatherEnabled = toggle;

    public void SimulationFPSChanged(float value)
    {
        SimulationFPS = (int)value;
        SimulationFPSLabel.Text = $"Simulation FPS: {SimulationFPS}";
    }
}
