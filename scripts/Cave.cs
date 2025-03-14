using Godot;
using System;
using System.Threading.Tasks;
using bravestory.scripts;

namespace bravestory.scripts;

public partial class Cave : World
{
	
	private Game _game;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		_game = GetTree().GetRoot().GetNode<Game>("/root/Game");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	private async void OnBoarDied()
	{
		await WaitTime();
		
	}
	
	private async Task WaitTime()
	{
		await ToSignal(GetTree().CreateTimer(0.5), SceneTreeTimer.SignalName.Timeout);
		
		_game.ChangeScene("res://game_end_screen.tscn", null, null);
	}
}
