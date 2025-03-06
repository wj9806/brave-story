using Godot;

namespace bravestory.scripts;

public partial class StateMachine : Node
{

	private int _currentState = -1;

	public int CurrentState
	{
		get => _currentState;
		set
		{
			//调用父节点的TransitionState函数
			_currentState = value;
			GetParent().Call("TransitionState", _currentState, value);
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
		// 启动异步任务
		InitializeStateAsync();
	}
	
	private async void InitializeStateAsync()
	{
		// 等待 owner 节点就绪
		await ToSignal(GetParent(), "ready");
		// 初始化当前状态
		CurrentState = 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		while (true)
		{
			int next =  GetParent().Call("GetNextState", _currentState).AsInt32();
			
			if (_currentState == next) break;
			
			CurrentState = next;
		}
		
		GetParent().Call("TickPhysics", _currentState, delta);
	}
}