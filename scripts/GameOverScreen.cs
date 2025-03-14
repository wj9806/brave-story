using Godot;
using System;

namespace bravestory.scripts;

public partial class GameOverScreen : Control
{

	private Game _game;
	private AnimationPlayer _animationPlayer;

	public override void _Ready()
	{
		_game = GetTree().GetRoot().GetNode<Game>("/root/Game");
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		
		Hide();
		SetProcessInput(false);
	}

	/**
	 * 输入事件处理顺序：
	 * _input => _gui_input => _shortcut_input => _unhandled_key_input => _unhandled_input
	 */
	public override void _Input(InputEvent @event)
	{
		GetWindow().SetInputAsHandled();
		if (_animationPlayer.IsPlaying())
		{
			return;
		}

		if (@event is InputEventKey
		    || @event is InputEventMouseButton
		    || @event is InputEventJoypadButton)
		{
			if (@event.IsPressed() && !@event.IsEcho())
			{
				_game.BackToTitle();
			}
		}
	}

	public void ShowGameOverScreen()
	{
		Show();
		SetProcessInput(true);
		_animationPlayer.Play("enter");
	}
}
