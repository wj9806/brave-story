using System;
using System.Collections.Generic;
using Godot;
using static bravestory.scripts.Constants;

namespace bravestory.scripts;

public enum State
{
    KeepCurrent = Constants.KeepCurrent,
    Idle,
    Running,
    Jump,
    Fall,
    Landing,
    WallSliding,
    WallJump,
    Attack1,
    Attack2,
    Attack3,
    Hurt,
    Dying
}

public partial class Player : CharacterBody2D
{
    private Node2D _graphics;
    private AnimationPlayer _animationPlayer;
    private Timer _coyoteTimer;
    private Timer _jumpRequestTimer;
    private StateMachine _stateMachine;
    private HurtBox _hurtBox;
    private RayCast2D _handChecker;
    private RayCast2D _footChecker;
    private Damage _pendingDamage; //待处理伤害
    private Stats _stats;
    private Timer _invincibleTimer; //受击后的无敌计时器
    
    private bool _isFirstTick; //是否是状态改变的第一帧
    private bool _isComboRequested = false; //是否发生combo
    
    private static readonly List<State> GroundStates = new();

    private bool _canCombo;

    [Export]
    public bool CanCombo
    {
        get => _canCombo;
        set => _canCombo = value;
    }

    public override void _Ready()
    {
        _graphics = GetNode<Node2D>("Graphics");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _coyoteTimer = GetParent().GetNode<Timer>("CoyoteTimer");
        _jumpRequestTimer = GetParent().GetNode<Timer>("JumpRequestTimer");
        _handChecker = _graphics.GetNode<RayCast2D>("HandChecker");
        _footChecker = _graphics.GetNode<RayCast2D>("FootChecker");
        _stats = GetNode<Stats>("Stats");
        _invincibleTimer = GetNode<Timer>("InvincibleTimer");

        _stateMachine = new StateMachine();
        AddChild(_stateMachine);

        _hurtBox = _graphics.GetNode<HurtBox>("HurtBox");

        State[] states = [State.Idle, State.Running, State.Landing, State.Attack1, State.Attack2, State.Attack3];
        GroundStates.AddRange(states);
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

        if (@event.IsActionPressed("attack") && _canCombo)
        {
            _isComboRequested = true;
        }
    }

    private void TickPhysics(State state, double delta)
    {
        if (_invincibleTimer.TimeLeft > 0)
        {
            //玩家无敌
            Color c = new(_graphics.Modulate)
            {
                //值域 0-1
                A = (float)(Math.Sin(Time.GetTicksMsec() / 20) * 0.5) + 0.5f
            };
            _graphics.SetModulate(c);
        }
        else
        {
            Color c = new(_graphics.Modulate)
            {
                A = 1
            };
            _graphics.SetModulate(c);
        }

        switch (state)
        {
            case State.Fall:
            case State.Running:
            case State.Idle:
                Move(Gravity, delta);
                break;
            case State.WallJump:
                if (_stateMachine.StateTime < 0.1)
                {
                    Stand(_isFirstTick ? 0 : Gravity, delta);
                    _graphics.SetScale(new Vector2(GetWallNormal().X, _graphics.Scale.Y));
                }
                else
                {
                    Move(Gravity, delta);
                }
                break;
            case State.Jump:
                Move(_isFirstTick ? 0 : Gravity, delta);
                break;
            case State.Landing:
                Stand(Gravity, delta);
                break;
            case State.WallSliding:
                if (Velocity.Y > 0)
                    Move(Gravity / 4, delta);
                else
                    Move(Gravity, delta);
                _graphics.SetScale(new Vector2(GetWallNormal().X, _graphics.Scale.Y));
                break;
            case State.Attack1:
            case State.Attack2:
            case State.Attack3:
            case State.Hurt:
            case State.Dying:
                Stand(Gravity, delta);
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
            _graphics.SetScale(new Vector2(direction < 0 ? -1.0f : 1.0f, _graphics.Scale.Y));
        }

        MoveAndSlide();
    }

    private void Stand(double gravity, double delta)
    {
        float acc = IsOnFloor() ? FloorAcceleration : AirAcceleration;
        var vec = new Vector2(Mathf.MoveToward(Velocity.X, 0, 
            (float)delta * acc), Velocity.Y + (float)(gravity * delta));
        Velocity = vec;
        MoveAndSlide();
    }

    private State GetNextState(State state)
    {
        if (_stats.Health == 0)
            if (state == State.Dying)
                return State.KeepCurrent;
            else
                return State.Dying;

        if (_pendingDamage != null)
            return State.Hurt;
        
        //是否能跳跃：在地板上或者timer剩余时间大于0
        var canJump = IsOnFloor() || _coyoteTimer.TimeLeft > 0;
        var shouldJump = canJump && _jumpRequestTimer.TimeLeft > 0;
        if (shouldJump)
            return State.Jump;

        //当前状态是地面状态，但是玩家不在地板上
        if (GroundStates.Contains(state) && !IsOnFloor())
        {
            //发生坠落
            return State.Fall;
        }
        
        var direction = Input.GetAxis("move_left", "move_right");
        //是否站立不动
        bool isStill = Mathf.IsZeroApprox(direction) && Mathf.IsZeroApprox(Velocity.X);
        
        switch (state)
        {
            case State.Idle:
                if (Input.IsActionJustPressed("attack"))
                    return State.Attack1;
                if (!isStill)
                    return State.Running;
                break;
            case State.Fall:
                if (IsOnFloor())
                    if (isStill)
                        return State.Landing;
                    else
                        return State.Running;
                if (CanWallSlide())
                    return State.WallSliding;
                break;
            case State.Running:
                if (Input.IsActionJustPressed("attack"))
                    return State.Attack1;
                if (isStill)
                    return State.Idle;
                break;
            case State.Jump:
                if (Velocity.Y >= 0)
                    return State.Fall;
                break;
            case State.Landing:
                if(!isStill)
                    return State.Running;
                if (!_animationPlayer.IsPlaying())
                    return State.Idle;
                break;
            case State.WallSliding:
                if (_jumpRequestTimer.TimeLeft > 0)
                {
                    return State.WallJump;
                }
                if (IsOnFloor())
                    return State.Idle;
                if (!IsOnWall())
                    return State.Fall;
                break;
            case State.WallJump:
                if (CanWallSlide())
                    return State.WallSliding;
                if (Velocity.Y >= 0)
                    return State.Fall;
                break;
            case State.Attack1:
                if (!_animationPlayer.IsPlaying())
                    if (_isComboRequested)
                        return State.Attack2;
                    else
                        return State.Idle;
                break;
            case State.Attack2:
                if (!_animationPlayer.IsPlaying())
                    if (_isComboRequested)
                        return State.Attack3;
                    else
                        return State.Idle;
                break;
            case State.Attack3:
                if (!_animationPlayer.IsPlaying())
                    return State.Idle;
                break;
            case State.Hurt:
                if (!_animationPlayer.IsPlaying())
                    return State.Idle;
                break;
        }

        return State.KeepCurrent;
    }

    /**
     * 转换状态执行函数
     */
    private void TransitionState(State from, State to)
    {
        //GD.Print($"{Engine.GetPhysicsFrames()} {from} => {to}");
        
        if (!GroundStates.Contains(from) && GroundStates.Contains(to))
        {
            _coyoteTimer.Stop();
        }
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
            case State.WallJump:
                _animationPlayer.Play("jump");
                
                Velocity = new Vector2(WallJumpVelocity.X * GetWallNormal().X, WallJumpVelocity.Y);
                _jumpRequestTimer.Stop();
                break;
            case State.Jump:
                _animationPlayer.Play("jump");
                
                Velocity = new Vector2(Velocity.X, JumpVelocity);
                _coyoteTimer.Stop();
                _jumpRequestTimer.Stop();
                break;
            case State.Landing:
                _animationPlayer.Play("landing");
                break;
            case State.WallSliding:
                _animationPlayer.Play("wall_sliding");
                break;
            case State.Attack1:
                _animationPlayer.Play("attack_1");
                _isComboRequested = false;
                break;
            case State.Attack2:
                _animationPlayer.Play("attack_2");
                _isComboRequested = false;
                break;
            case State.Attack3:
                _animationPlayer.Play("attack_3");
                _isComboRequested = false;
                break;
            case State.Hurt:
                _animationPlayer.Play("hurt");
                //扣血
                _stats.Health -= _pendingDamage.Amount;
                //计算相对方向
                var dir = _pendingDamage.Source.GlobalPosition.DirectionTo(GlobalPosition);
                //击退
                Velocity = dir * KnockBackAmount;
                //重置伤害
                _pendingDamage = null;
                _invincibleTimer.Start();
                break;
            case State.Dying:
                _animationPlayer.Play("die");
                _invincibleTimer.Stop();
                break;
        }

        if (to == State.WallJump)
        {
            Engine.TimeScale = 0.3d;
        }

        if (from == State.WallJump)
        {
            Engine.TimeScale = 1.0;
        }

        _isFirstTick = true;
    }

    private bool CanWallSlide()
    {
        return IsOnWall() && _handChecker.IsColliding()
                          && _footChecker.IsColliding();
    }

    /**
    * 玩家受到攻击回调
     */
    private void OnHurtBoxHurt(HitBox hitBox)
    {
        if (_invincibleTimer.TimeLeft > 0)
            return;
        _pendingDamage = new();
        _pendingDamage.Amount = 1;
        _pendingDamage.Source = hitBox.Owner as Node2D;
    }
    
    private void Die()
    {
        QueueFree();
        //重新加载场景
        GetTree().ReloadCurrentScene();
    }
}
