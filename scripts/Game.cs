using Godot;

namespace bravestory.scripts;

public partial class Game : Node
{
	private Stats _playerStats;

	public Stats PlayerStats
	{
		get;
		set;
	}

	public Node CurrentScene { get; set; }

	public override void _Ready()
	{
		Viewport root = GetTree().Root;
		// 根节点的最后一个子节点始终是加载的场景。
		CurrentScene = root.GetChild(-1);

		PlayerStats = GetNode<Stats>("PlayerStats");
	}
	
	public void ChangeScene(string path, string entryPoint)
	{
		CallDeferred(MethodName.DeferredGotoScene, path, entryPoint);
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
}