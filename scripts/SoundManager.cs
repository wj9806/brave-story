using Godot;
using System;

public partial class SoundManager : Node
{

	private Node _sfx;
	private AudioStreamPlayer _bgmPlayer;
	
	public override void _Ready()
	{
		_sfx = GetNode<Node>("SFX");
		_bgmPlayer = GetNode<AudioStreamPlayer>("BGMPlayer");
	}

	public void PlaySfx(string name)
	{
		var player = _sfx.GetNode<AudioStreamPlayer>(name);
		if (player == null)
			return;
		player.Play();
	}

	public void PlayBgm(AudioStream stream)
	{
		if (_bgmPlayer.Stream == stream && _bgmPlayer.IsPlaying())
		{
			return;
		}
		
		_bgmPlayer.Stream = stream;
		_bgmPlayer.Play();
	}

	public void SetUpUiSounds(Node node)
	{
		if (node is Button button)
		{
			button.Connect(BaseButton.SignalName.Pressed, Callable.From(() => PlaySfx("UIPress")));
			button.Connect(Control.SignalName.FocusEntered, Callable.From(() => PlaySfx("UIFocus")));
		}

		foreach (var child in node.GetChildren())
		{
			SetUpUiSounds(child);
		}
	}
}
