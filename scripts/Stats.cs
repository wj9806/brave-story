using System;
using Godot;

namespace bravestory.scripts;

[GlobalClass]
public partial class Stats : Node
{

	private int _maxHealth;

	[Export]
	public int MaxHealth
	{
		get;
		set;
	} = 5;

	private int _health;
	[Export]
	public int Health
	{
		get => _health;
		set => _health = Math.Clamp(value, 0, MaxHealth);
	}

	public override void _Ready()
	{
		Health = MaxHealth;
	}
}