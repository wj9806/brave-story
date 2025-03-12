using Godot;
using System;

namespace bravestory.scripts;
public partial class Camera2d : Camera2D
{
	
	[Export]
	private double _strength = 4f;
	
	//震动恢复速度
	[Export]
	public float RecoverySpeed = 16.0f;

	public override void _Ready()
	{
		var game = GetTree().GetRoot().GetNode<Game>("/root/Game");

		game.Connect(Game.SignalName.CameraShouldShake, new Callable(this, "UpdateStrength"));
	}

	private void UpdateStrength(int amount)
	{
		_strength += amount;
	}

	public override void _Process(double delta)
	{
		Offset = new()
		{
			X = (float)GD.RandRange(-_strength, _strength),
			Y = (float)GD.RandRange(-_strength, _strength)
		};
		
		_strength = Mathf.MoveToward(_strength, 0, delta * RecoverySpeed);
	}
}
