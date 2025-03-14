using Godot;
using System;

namespace bravestory.scripts;

public partial class TitleScreen : Control
{
	private Button _newGame;
	private Button _loadGame;
	private VBoxContainer _v;
	private Game _game;
	private SoundManager _soundManager;
	
	public override void _Ready()
	{
		_game = GetTree().GetRoot().GetNode<Game>("/root/Game");
		_soundManager = GetTree().GetRoot().GetNode<SoundManager>("SoundManager");
		
		_v = GetNode<VBoxContainer>("V");
		_newGame = _v.GetNode<Button>("NewGame");
		//获取键盘焦点
		_newGame.GrabFocus();
		
		_loadGame = _v.GetNode<Button>("LoadGame");
		_loadGame.Disabled = !_game.HasSaveFile();
		
		foreach (var node in _v.GetChildren())
		{
			var button = node as Button;
			button.Connect(Button.SignalName.MouseEntered, Callable.From(() => button.GrabFocus()));
		}
		
		_soundManager.SetUpUiSounds(this);
		
		//可以从资源路径中加载音乐
		var stream = AudioStreamOggVorbis.LoadFromFile("res://assets/music/1 titles LOOP.ogg");
		_soundManager.PlayBgm(stream);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnNewGamePressed()
	{
		_game.NewGame();
	}

	private void OnLoadGamePressed()
	{
		
		_game.LoadGame();
	}

	private void OnExitGamePressed()
	{
		GetTree().Quit();
	}
}
