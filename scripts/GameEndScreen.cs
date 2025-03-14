using Godot;
using System;
using System.Collections.Generic;

namespace bravestory.scripts;

public partial class GameEndScreen : Control
{
	private List<string> _lines = [
		"Niubility，通关啦~",
		"大魔王终于被打败了..",
		"森林又恢复了往日的宁静...",
		"但是这一切都是值得吗？"
	];
	
	private int _currentLine = -1;
	
	private Tween _tween;
	
	private Label _label;
	private SoundManager _soundManager;
	
	public override void _Ready()
	{
		_soundManager = GetTree().GetRoot().GetNode<SoundManager>("SoundManager");
		var stream = AudioStreamOggVorbis.LoadFromFile("res://assets/music/15 game over LOOP.ogg");
		_soundManager.PlayBgm(stream);
		
		_label = GetNode<Label>("Label");
		ShowLine(0);
	}
	
	public override void _Input(InputEvent @event)
	{
		if (_tween.IsRunning())
		{
			return;
		}

		if (@event is InputEventKey
		    || @event is InputEventMouseButton
		    || @event is InputEventJoypadButton)
		{
			if (@event.IsPressed() && !@event.IsEcho())
			{
				if (_currentLine + 1 < _lines.Count)
				{
					ShowLine(_currentLine + 1);
				}
				else
				{
					Game game = GetTree().GetRoot().GetNode<Game>("/root/Game");
					game.BackToTitle();
				}
			}
		}
	}

	private void ShowLine(int line)
	{
		_currentLine = line;
		_tween = CreateTween();
		//淡入淡出效果设置
		_tween.SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Sine);
		if (line > 0)
		{
			_tween.TweenProperty(_label, "modulate:a", 0, 1);
		}
		else
		{
			_label.Modulate = new Color(_label.Modulate, 0);
		}

		_tween.TweenCallback(Callable.From(()=> _label.SetText(_lines[line])));
		_tween.TweenProperty(_label, "modulate:a", 1, 1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
