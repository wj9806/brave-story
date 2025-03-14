using Godot;
using System;
using Range = Godot.Range;

namespace bravestory.scripts;

public partial class VolumeSlider : HSlider
{

	[Export] public StringName Bus = "Master";
	
	private int _busIndex;
	
	public override void _Ready()
	{
		_busIndex = AudioServer.GetBusIndex(Bus);
		
		Value = SoundManager.Instance.GetVolume(_busIndex);
		Connect(Range.SignalName.ValueChanged, Callable.From<float>(v =>
		{
			SoundManager.Instance.SetVolume(_busIndex, v);
			Game.Instance.SaveConfig();
		}));
	}

	public override void _Process(double delta)
	{
	}
}
