using System;
using Godot;

namespace bravestory.scripts;

[GlobalClass]
public partial class Stats : Node
{

	[Export]
	public int MaxHealth = 5; //最大血量
	
	[Export]
	public float MaxEnergy = 10f; //最大能量

	[Export] 
	public double EnergyRegen = 0.8d; //每秒恢复能量

	private int _health;
	[Export]
	public int Health
	{
		get => _health;
		set
		{
			_health = Math.Clamp(value, 0, MaxHealth);
			EmitSignal(SignalName.HealthChanged);
		}
	}
	
	private double _energy;
	[Export]
	public double Energy
	{
		get => _energy;
		set
		{
			_energy = Math.Clamp(value, 0, MaxEnergy);
			EmitSignal(SignalName.EnergyChanged);
		}
	}

	public override void _Ready()
	{
		Health = MaxHealth;
		Energy = MaxEnergy;
	}
	
	[Signal]
	public delegate void HealthChangedEventHandler();
	
	[Signal]
	public delegate void EnergyChangedEventHandler();

	public override void _Process(double delta)
	{
		//回复能量
		Energy += EnergyRegen * delta;
	}
}