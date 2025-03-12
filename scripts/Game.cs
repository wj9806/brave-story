using System.Threading.Tasks;
using Godot;

namespace bravestory.scripts;

public partial class Game : CanvasLayer
{
	[Signal]
	public delegate void CameraShouldShakeEventHandler(int amount);
	
	private Stats _playerStats;
	private ColorRect _colorRect;

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
	
	public async void ChangeScene(string path, string entryPoint)
	{
		var tree = GetTree();
		tree.Paused = true;

		Tween tween = CreateTween();
		tween.SetPauseMode(Tween.TweenPauseMode.Process);
		tween.TweenProperty(_colorRect, "color:a", 1, 0.2);
		await WaitTween(tween);
		
		CallDeferred(MethodName.DeferredGotoScene, path, entryPoint);
		
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

	public void DeferredGotoScene(string path, string entryPoint)
	{
		//销毁当前场景
		CurrentScene.Free();

		//加载新的场景
		var nextScene = GD.Load<PackedScene>(path);

		//实例化
		CurrentScene = nextScene.Instantiate();

		//添加到节点树
		GetTree().Root.AddChild(CurrentScene);

		// Optionally, to make it compatible with the SceneTree.change_scene_to_file() API.
		GetTree().CurrentScene = CurrentScene;

		
		//设置Player的落地位置
		var tree = GetTree();
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

	public void ShakeCamera(int amount)
	{
		EmitSignal(SignalName.CameraShouldShake, amount);
	}
}