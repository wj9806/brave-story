using Godot;
using static bravestory.scripts.Constants;

namespace bravestory.scripts;

[GlobalClass]
public partial class Enemy : CharacterBody2D
{

    protected Node2D Graphics;
    protected AnimationPlayer AnimationPlayer;
    protected StateMachine StateMachine;
    protected Stats Stats;

    private Direction _direction = Direction.Left;

    [Export]
    public Direction Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            HandleDirectionChange(); // 触发异步处理
        }
    } 
    
    private async void HandleDirectionChange()
    {
        // 若节点未准备就绪，等待Ready信号
        if (!IsNodeReady())
        {
            await ToSignal(this, Node.SignalName.Ready);
        }

        Graphics.SetScale(new Vector2(-(int)Direction, Graphics.Scale.Y));
    }

    private float _maxSpeed;

    [Export]
    public float MaxSpeed
    {
        get; set;
    } = 180;

    private float _acceleration;

    [Export]
    public float Acceleration
    {
        get;
        set;
    } = 2000;


    public override void _Ready()
    {
        Graphics = GetNode<Node2D>("Graphics");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Stats = GetNode<Stats>("Stats");

        StateMachine = new();
        AddChild(StateMachine);
    }

    protected void Move(float speed, double delta)
    {
        var x = Mathf.MoveToward(Velocity.X, speed * (int)Direction, Acceleration * delta);
        Velocity = new Vector2((float)x, Velocity.Y + (float)(Gravity * delta));
        MoveAndSlide();
    }

    private void Die()
    {
        QueueFree();
    }
}
