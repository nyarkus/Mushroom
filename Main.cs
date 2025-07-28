using Godot;
using System;
using System.Diagnostics;
using Mushroom;
using Mushroom.Data;
using Mushroom.Generation;
namespace Mushroom;
public partial class Main : Node
{
	[Export] public int Seed = 5;
	[Export] public int Width = 240;
	[Export] public int Height = 40;
	[Export] public int GroundLevel = 0;

	public long Tick = 0;
	public long WeatherTick = 0;
	public long WeatherTicks = 0;
	
	private Stopwatch _stopwatch = new();
	private TimeSpan _logicTime = TimeSpan.Zero;
	private TimeSpan _renderTime = TimeSpan.Zero;
	private Random _rand = new();
	
	private Render _render;
	private Hud _hud;
	public override void _Ready()
	{
		GD.Print("Setup render...");
		_render = GetNode<Render>("Render");
		_render.Initialize();
		
		GD.Print("Setup hud...");
		_hud = GetNode<Hud>("Hud");
	}

	private double oldTimer = 0;
	private double timer = 0;
	private double targetTimer;
	public override void _Process(double delta)
	{
		targetTimer = 1 / _hud.SimulationFPS;
		timer += delta;
		if (timer < targetTimer)
			return;
		oldTimer = timer;
		timer = 0;
		
		_stopwatch.Restart();
		
		Tick++;
		Logic.Calculate();
		if (_hud.WeatherEnabled && Tick >= WeatherTick)
		{
			if (Tick % 6 == 0)
			{
				Weather.Rain();
				WeatherTicks++;
			}

			if (WeatherTicks >= 10)
			{
				WeatherTick = Tick + _rand.Next(150, 1000);
				WeatherTicks = 0;
			}
		}

		_logicTime = _stopwatch.Elapsed;
		_stopwatch.Restart();
		
		_render.Call("RenderFrame");
		_renderTime = _stopwatch.Elapsed;
		
		_hud.Info.Text = $"Logic Time: {_logicTime.TotalMilliseconds:F1} ms" +
		                 $"\nRender Time: {_renderTime.TotalMilliseconds:F1} ms" +
		                 $"\nSimulation FPS: {1000 / (_logicTime.TotalMilliseconds + _renderTime.TotalMilliseconds + oldTimer * 1000):F0}" +
		                 $"\nTick: {Tick}" +
		                 $"\nNext rain in " + (_hud.WeatherEnabled ? Math.Clamp(WeatherTick - Tick, 0, int.MaxValue) : "Never") + " ticks";
	}
}
