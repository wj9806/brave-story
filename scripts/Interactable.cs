using Godot;
using System;

namespace bravestory.scripts; 

[GlobalClass]
public partial class Interactable : Area2D
{
	
	[Signal]
	public delegate void InteractEventHandler();

	public override void _Ready()
	{
		CollisionLayer = 0;
		CollisionMask = 0;
		//设置打开与2 Layer的碰撞
		SetCollisionMaskValue(2, true);
		
		//连接信号
		Connect(Area2D.SignalName.BodyEntered, new Callable(this, "OnBodyEntered"));
		Connect(Area2D.SignalName.BodyExited, new Callable(this, "OnBodyExited"));
	}

	private void OnBodyEntered(Player player)
	{
		player.AddInteractable(this);
	}
	
	private void OnBodyExited(Player player)
	{
		player.RemoveInteractable(this);
	}

	public virtual void InteractHandler()
	{
		EmitSignal(SignalName.Interact);
	}

}
