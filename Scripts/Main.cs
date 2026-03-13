using Godot;
using System;
using System.Diagnostics;

namespace Mushroom;

public partial class Main : Node
{
	[Export] public int Seed = 5;
	[Export] public int Width = 240;
	[Export] public int Height = 40;
	[Export] public int GroundLevel = 0;

	public long Tick = 0;
	public long WeatherTick = 0;
	public long RainTicks = 0;
	
	private Stopwatch _stopwatch = new();
	private Random _rand = new();
	
	private Render _render;
	private Hud _hud;
	
	private double timer = 0;
	private double targetTimer;
	
	private double _fpsTimer = 0;
	private int _ticksThisSecond = 0;
	private double _logicTimeAccumulator = 0;
	private double _renderTimeAccumulator = 0;
	
	private int _displayFps = 0;
	private double _displayLogicTime = 0;
	private double _displayRenderTime = 0;
	private string _displayNextRain = "Never";

	public override void _Ready()
	{
		GD.Print("Setup render...");
		_render = GetNode<Render>("Render");
		
		GD.Print("Setup hud...");
		_hud = GetNode<Hud>("CanvasLayer/Hud");
	}

	public override void _Process(double delta)
	{
		_fpsTimer += delta;
		if (_fpsTimer >= 1.0)
		{
			_displayFps = _ticksThisSecond;
			_displayLogicTime = _ticksThisSecond > 0 ? _logicTimeAccumulator / _ticksThisSecond : 0;
			_displayRenderTime = _ticksThisSecond > 0 ? _renderTimeAccumulator / _ticksThisSecond : 0;
			_displayNextRain = _hud.WeatherEnabled ? Math.Clamp(WeatherTick - Tick, 0, int.MaxValue).ToString() : "Never";
			
			_ticksThisSecond = 0;
			_logicTimeAccumulator = 0;
			_renderTimeAccumulator = 0;
			_fpsTimer -= 1.0;
		}
		
		targetTimer = 1.0 / _hud.SimulationFPS;
		timer += delta;
		
		if (timer < targetTimer)
			return;

		timer -= targetTimer;
		
		Tick++;
		_ticksThisSecond++;
		
		_stopwatch.Restart();
		Logic.Calculate();
		
		if (_hud.WeatherEnabled && Tick >= WeatherTick)
		{
			if (Tick % 6 == 0)
			{
				Weather.Rain();
				RainTicks++;
			}

			if (RainTicks >= 100)
			{
				WeatherTick = Tick + _rand.Next(150, 5000);
				RainTicks = 0;
			}
		}
		_logicTimeAccumulator += _stopwatch.Elapsed.TotalMilliseconds;
		
		_stopwatch.Restart();
		_render.Call("RenderFrame");
		_renderTimeAccumulator += _stopwatch.Elapsed.TotalMilliseconds;
		
		_hud.Info.Text = $"Logic Time: {_displayLogicTime:F1} ms\n" +
						 $"Render Time: {_displayRenderTime:F1} ms\n" +
						 $"Simulation FPS: {_displayFps}\n" +
						 $"Tick: {Tick}\n" +
						 $"Next rain in {_displayNextRain} ticks";
	}
}
