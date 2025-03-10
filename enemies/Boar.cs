using Godot;
using System;
using bravestory.scripts;

namespace bravestory.enemies;

public enum State
{
    Idle,
    Walk,
    Run
}

public partial class Boar : Enemy
{
    
    private RayCast2D _wallChecker;
    private RayCast2D _floorChecker;
    private RayCast2D _playerChecker;
    private Timer _calmDownTimer; //冷静计时器

    public override void _Ready()
    {
        base._Ready();
        _wallChecker = Graphics.GetNode<RayCast2D>("WallChecker");
        _floorChecker = Graphics.GetNode<RayCast2D>("FloorChecker");
        _playerChecker = Graphics.GetNode<RayCast2D>("PlayerChecker");
        _calmDownTimer = GetNode<Timer>("CalmDownTimer");//冷静计时器
    }

    private void TickPhysics(State state, double delta)
    {
        switch (state)
        {
            case State.Idle:
                Move(0, delta);
                break;
            case State.Walk:
                Move(MaxSpeed / 3, delta);
                break;
            case State.Run:
                //暴走状态,遇到墙或者悬崖，则转身
                if (_wallChecker.IsColliding() || !_floorChecker.IsColliding())
                    Direction = (Direction)(-(int)Direction);
                Move(MaxSpeed, delta);
                if (CanSeePlayer())
                    _calmDownTimer.Start();
                break;
        }
    }

    private State GetNextState(State state)
    {
        if (CanSeePlayer())
        {
            return State.Run;
        }

        switch (state)
        {
            case State.Idle:
                //idle状态大于2秒,进入Walk状态
                if (StateMachine.StateTime > 2)
                    return State.Walk;
                break;
            case State.Walk:
                //前面是墙或者是悬崖
                if (_wallChecker.IsColliding() || !_floorChecker.IsColliding())
                    return State.Idle;
                break;
            case State.Run:
                if (_calmDownTimer.IsStopped())
                    return State.Walk;
                break;
        }
        return state;
    }

    private void TransitionState(State from, State to)
    {
        GD.Print($"{Engine.GetPhysicsFrames()} {from} => {to}");

        switch (to)
        {
            case State.Idle:
                AnimationPlayer.Play("idle");
                //前面是墙，则立即转身
                if (_wallChecker.IsColliding())
                    Direction = (Direction)(-(int)Direction);
                break;
            case State.Walk:
                AnimationPlayer.Play("walk");
                //前面悬崖，立即转身
                if (!_floorChecker.IsColliding())
                {
                    Direction = (Direction)(-(int)Direction);
                    //悬崖转身后强制更新Raycast
                    _floorChecker.ForceRaycastUpdate();
                }
                break;
            case State.Run:
                AnimationPlayer.Play("run");
                break;
        }
    }

    private bool CanSeePlayer()
    {
        if (!_playerChecker.IsColliding())
        {
            return false;
        }
        //判断射线跟哪个物体发生碰撞
        return _playerChecker.GetCollider() is Player;
    }
}
