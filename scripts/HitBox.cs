using Godot;
using System;

namespace bravestory.scripts;

[GlobalClass]
public partial class HitBox : Area2D
{
	
	//自定义hit信号
	[Signal]
	public delegate void HitEventHandler(HurtBox hurtbox);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Connect(Area2D.SignalName.AreaEntered, new Callable(this, "OnAreaEntered"));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnAreaEntered(HurtBox hurtBox)
	{
		GD.Print($"[hit] {Owner.Name} ==> {hurtBox.Owner.Name}");
		//触发HitBox的hit信号，入参是hurtBox
		EmitSignal(SignalName.Hit, hurtBox);
		//触发hurtBox的hurt信号，入参是hitBox
		hurtBox.EmitSignal(HurtBox.SignalName.Hurt, this);
	}
}
