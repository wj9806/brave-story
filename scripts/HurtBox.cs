using Godot;
using System;

namespace bravestory.scripts;

[GlobalClass]
public partial class HurtBox : Area2D
{
	//自定义hurt信号
	[Signal]
	public delegate void HurtEventHandler(HitBox hitBox);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
