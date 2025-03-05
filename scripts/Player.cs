using Godot;
using System;

public partial class Player : CharacterBody2D
{
    private Sprite2D sprite2D;
    private AnimationPlayer animationPlayer;
    private Timer coyoteTimer;
    private Timer jumpRequestTimer;

    public override void _Ready()
    {
        sprite2D = GetNode<Sprite2D>("Sprite2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        coyoteTimer = GetParent().GetNode<Timer>("CoyoteTimer");
        jumpRequestTimer = GetParent().GetNode<Timer>("JumpRequestTimer");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("jump"))
        {
            jumpRequestTimer.Start();
        }

        
        if (@event.IsActionReleased("jump") && Velocity.Y < Constants.JUMP_VELOCITY / 2)
        {
            var vec = Velocity;
            vec.Y = Constants.JUMP_VELOCITY / 2;
            Velocity = vec;
        }

        
    }

    public override void _PhysicsProcess(double delta)
    {
        var direction = Input.GetAxis("move_left", "move_right");

        float acc = IsOnFloor() ? Constants.FLOOR_ACCELERATION : Constants.AIR_ACCELERATION;
        var vec = new Vector2(Mathf.MoveToward(Velocity.X, direction * Constants.RUN_SPEED, 
            (float)delta * acc), Velocity.Y + (float)(Constants.GRAVITY * delta));
        
        Velocity = vec;

        //是否能跳跃：在地板上或者timer剩余时间大于0
        var canJump = IsOnFloor() || coyoteTimer.TimeLeft > 0;
        var shouldJump = canJump && jumpRequestTimer.TimeLeft > 0;
        if (shouldJump)
        {
            vec.Y = Constants.JUMP_VELOCITY;
            Velocity = vec;
            coyoteTimer.Stop();
            jumpRequestTimer.Stop();
        }
        
        if (IsOnFloor())
        {
            if (Mathf.IsZeroApprox(direction) && Mathf.IsZeroApprox(Velocity.X))
            {
                animationPlayer.Play("idle");
            }
            else
            {
                animationPlayer.Play("running");
            }
        }
        else
        {
            animationPlayer.Play("jump");
        }

        if (!Mathf.IsZeroApprox(direction))
        {
            sprite2D.FlipH = direction < 0;
        }

        var isOnFloor = IsOnFloor();
        MoveAndSlide();

        if (isOnFloor != IsOnFloor())
        {
            //已经离开地面并且不是因为跳跃而离开地面
            if (isOnFloor && !shouldJump)
            {
                coyoteTimer.Start();
            }
            else
            {
                coyoteTimer.Stop();
            }
        }
    }
}
