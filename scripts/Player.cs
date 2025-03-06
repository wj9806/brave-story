using System.Collections.Generic;
using Godot;
using static bravestory.scripts.Constants;

namespace bravestory.scripts;

public enum State
{
    Idle,
    Running,
    Jump,
    Fall,
    Landing
}

public partial class Player : CharacterBody2D
{
    private Sprite2D _sprite2D;
    private AnimationPlayer _animationPlayer;
    private Timer _coyoteTimer;
    private Timer _jumpRequestTimer;
    private StateMachine _stateMachine;
    private bool _isFirstTick; //是否是状态改变的第一帧
    
    private static readonly List<State> GroundStates = new();

    public override void _Ready()
    {
        _sprite2D = GetNode<Sprite2D>("Sprite2D");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _coyoteTimer = GetParent().GetNode<Timer>("CoyoteTimer");
        _jumpRequestTimer = GetParent().GetNode<Timer>("JumpRequestTimer");

        _stateMachine = new StateMachine();
        AddChild(_stateMachine);

        GroundStates.Add(State.Idle);
        GroundStates.Add(State.Running);
        GroundStates.Add(State.Landing);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("jump"))
        {
            _jumpRequestTimer.Start();
        }

        
        if (@event.IsActionReleased("jump"))
        {
            _jumpRequestTimer.Stop();
            if (Velocity.Y < JumpVelocity / 2)
            {
                var vec = Velocity;
                vec.Y = JumpVelocity / 2;
                Velocity = vec;
            }
        }
    }

    private void TickPhysics(State state, double delta)
    {
        switch (state)
        {
            case State.Fall:
            case State.Running:
            case State.Idle:
                Move(Gravity, delta);
                break;
            case State.Jump:
                Move(_isFirstTick ? 0 : Gravity, delta);
                break;
            case State.Landing:
                Stand(delta);
                break;
        }

        _isFirstTick = false;
    }

    private void Move(double gravity, double delta)
    {
        var direction = Input.GetAxis("move_left", "move_right");

        float acc = IsOnFloor() ? FloorAcceleration : AirAcceleration;
        var vec = new Vector2(Mathf.MoveToward(Velocity.X, direction * RunSpeed, 
            (float)delta * acc), Velocity.Y + (float)(gravity * delta));
        Velocity = vec;

        if (!Mathf.IsZeroApprox(direction))
        {
            _sprite2D.FlipH = direction < 0;
        }

        MoveAndSlide();
    }

    private void Stand(double delta)
    {
        float acc = IsOnFloor() ? FloorAcceleration : AirAcceleration;
        var vec = new Vector2(Mathf.MoveToward(Velocity.X, 0, 
            (float)delta * acc), Velocity.Y + (float)(Gravity * delta));
        Velocity = vec;
        MoveAndSlide();
    }

    private State GetNextState(State state)
    {
        //是否能跳跃：在地板上或者timer剩余时间大于0
        var canJump = IsOnFloor() || _coyoteTimer.TimeLeft > 0;
        var shouldJump = canJump && _jumpRequestTimer.TimeLeft > 0;
        if (shouldJump)
            return State.Jump;
        
        var direction = Input.GetAxis("move_left", "move_right");
        //是否站立不动
        bool isStill = Mathf.IsZeroApprox(direction) && Mathf.IsZeroApprox(Velocity.X);
        
        switch (state)
        {
            case State.Idle:
                if (!IsOnFloor())
                    return State.Fall;
                if (!isStill)
                    return State.Running;
                break;
            case State.Fall:
                if (IsOnFloor())
                    if (isStill)
                        return State.Landing;
                    else
                        return State.Running;
                break;
            case State.Running:
                if (!IsOnFloor())
                    return State.Fall;
                if (isStill)
                    return State.Idle;
                break;
            case State.Jump:
                if (Velocity.Y >= 0)
                    return State.Fall;
                break;
            case State.Landing:
                if (!_animationPlayer.IsPlaying())
                    return State.Idle;
                break;
            default:
                return state;
        }

        return state;
    }

    /**
     * 转换状态执行函数
     */
    private void TransitionState(State from, State to)
    {
        if (!GroundStates.Contains(from) && GroundStates.Contains(to))
        {
            _coyoteTimer.Stop();
        }
        var vec = Velocity;
        
        switch (to)
        {
            case State.Idle:
                _animationPlayer.Play("idle");
                break;
            case State.Fall:
                _animationPlayer.Play("fall");
                if (GroundStates.Contains(from))
                {
                    _coyoteTimer.Start();
                }
                break;
            case State.Running:
                _animationPlayer.Play("running");
                break;
            case State.Jump:
                _animationPlayer.Play("jump");
                
                vec.Y = JumpVelocity;
                Velocity = vec;
                _coyoteTimer.Stop();
                _jumpRequestTimer.Stop();
                break;
            case State.Landing:
                _animationPlayer.Play("landing");
                break;
        }

        _isFirstTick = true;
    }
}
