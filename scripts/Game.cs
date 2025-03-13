using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;

namespace bravestory.scripts;

public partial class Game : CanvasLayer
{
	[Signal]
	public delegate void CameraShouldShakeEventHandler(int amount);
	
	private Stats _playerStats;
	private ColorRect _colorRect;

	private Dictionary<string, Hashtable> _worldStates = new();

	public Stats PlayerStats
	{
		get;
		set;
	}

	public Node CurrentScene { get; set; }

	public override void _Ready()
	{
		_colorRect = GetNode<ColorRect>("ColorRect");
		_colorRect.Color = new Color(_colorRect.Color, 0f);
			
		Viewport root = GetTree().Root;
		// 根节点的最后一个子节点始终是加载的场景。
		CurrentScene = root.GetChild(-1);

		PlayerStats = GetNode<Stats>("PlayerStats");
	}
	
	public async void ChangeScene(string path, string entryPoint, string gameDataJson)
	{
		var tree = GetTree();
		tree.Paused = true;

		Tween tween = CreateTween();
		tween.SetPauseMode(Tween.TweenPauseMode.Process);
		tween.TweenProperty(_colorRect, "color:a", 1, 0.2);
		await WaitTween(tween);
		
		if (gameDataJson != null)
		{
			//加载用户状态
			var gameData = JsonSerializer.Deserialize<GameData>(gameDataJson);
			PlayerStats.FromDict(gameData.Stats);
			CallDeferred(MethodName.DeferredGotoScene, path, entryPoint, JsonSerializer.Serialize(gameData.Player));
		}
		else if (entryPoint != null)
		{
			CallDeferred(MethodName.DeferredGotoScene, path, entryPoint, "");
		}
		else
		{
			//回到标题
			CallDeferred(MethodName.DeferredGotoScene, path, "", "");
		}

		tree.Paused = false;
	
		tween = CreateTween();
		tween.SetPauseMode(Tween.TweenPauseMode.Process);
		tween.TweenProperty(_colorRect, "color:a", 0, 0.2);
		await WaitTween(tween);
	}

	private async Task WaitTween(Tween tween)
	{
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	public void DeferredGotoScene(string path, string entryPoint, string playerJson)
	{
		//保存当前场景数据
		var baseName = CurrentScene.SceneFilePath.GetFile().GetBaseName();
		if (baseName != "title_screen")
		{
			_worldStates[baseName] = ((World)CurrentScene).ToDict();
		}
		
		//销毁当前场景
		CurrentScene.Free();

		//加载新的场景
		var nextScene = GD.Load<PackedScene>(path);

		//实例化
		CurrentScene = nextScene.Instantiate();

		//添加到节点树
		GetTree().Root.AddChild(CurrentScene);
		GetTree().CurrentScene = CurrentScene;
		
		//加载场景数据
		baseName = CurrentScene.SceneFilePath.GetFile().GetBaseName();
		if (_worldStates.TryGetValue(baseName, out var state))
		{
			((World)CurrentScene).FromDict(state);
		}

		var tree = GetTree();
		if (playerJson is { Length: > 0 })
		{
			GameData.PlayerData player = JsonSerializer.Deserialize<GameData.PlayerData>(playerJson);
			((World)tree.CurrentScene).UpdatePlayer(new Vector2(player.PositionX, player.PositionY), (Direction)player.Direction);
		}
		else
		{
			//设置Player的落地位置
			foreach  (Node node in tree.GetNodesInGroup("entry_points"))
			{
				if (node.Name == entryPoint)
				{
					if (node is EntryPoint ep)
					{
						((World)tree.CurrentScene).UpdatePlayer(ep.GetGlobalPosition(), ep.Direction);
					}
					break;
				}
			}
		}
	}

	public void ShakeCamera(int amount)
	{
		EmitSignal(SignalName.CameraShouldShake, amount);
	}

	public void SaveGame()
	{
		var baseName = CurrentScene.SceneFilePath.GetFile().GetBaseName();
		_worldStates[baseName] = ((World)CurrentScene).ToDict();
		
		var player = CurrentScene.GetNode<Player>("Player");

		GameData pd = new()
		{
			WorldStates = _worldStates,
			Stats = PlayerStats.ToDict(),
			Scene = CurrentScene.SceneFilePath,
			Player = new GameData.PlayerData
			{
				Direction = (int)player.Direction,
				PositionX = player.Position.X,
				PositionY = player.Position.Y
			}
		};

		String json = JsonSerializer.Serialize(pd);
		GD.Print("Saving game: " + json);
		var file = FileAccess.Open(Constants.SavePath, FileAccess.ModeFlags.Write);
		if (file != null)
		{
			file.StoreString(json);
			file.Flush();
			file.Close();
		}
	}

	//读取存档代码写的有点沙雕,等待重构
	public void LoadGame()
	{
		var file = FileAccess.Open(Constants.SavePath, FileAccess.ModeFlags.Read);
		string txt = file.GetAsText();
		GD.Print("Loading game data from " + txt);
		
		var gameData = JsonSerializer.Deserialize<GameData>(txt);
		_worldStates = gameData.WorldStates;
		ChangeScene(gameData.Scene, null, txt);
	}

	public void NewGame()
	{
		ChangeScene("res://world.tscn", "BornEntry", null);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			BackToTitle();
		}
	}

	//是否有存档
	public bool HasSaveFile()
	{
		return FileAccess.FileExists(Constants.SavePath);
	}

	private void BackToTitle()
	{
		ChangeScene("res://title_screen.tscn", null, null);
	}

	//游戏序列化数据
	public class GameData
	{
		public Dictionary<string, Hashtable> WorldStates { get; set; }
		
		public Dictionary<string, object> Stats { get; set; }

		public string Scene { get; set; }
		
		public PlayerData Player { get; set; }
		
		public class PlayerData
		{
			public int Direction { get; set; }
			
			public float PositionX { get; set; }
			
			public float PositionY { get; set; }
		}
	}
}