using Godot;

namespace bravestory.scripts;

public abstract class Constants
{
    //重力
    public static readonly double Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsDouble();

    //水平移动速度
    public const int RunSpeed = 160;

    //跳跃速度
    public const int JumpVelocity = -320;

    //角色地面加速度
    public const float FloorAcceleration = RunSpeed / 0.2f;

    //角色空中加速度
    public const float AirAcceleration = RunSpeed / 0.05f;
    
    //蹬墙跳速度
    public static Vector2 WallJumpVelocity = new(500, -300);

    //怪物受击后被击飞的速度
    public const float KnockBackAmount = 512;
    
    //保持当前状态
    public const int KeepCurrent = -1;

    //滑铲时间
    public const double SlidingDuration = 0.3d;

    //滑铲速度
    public const float SlidingSpeed = 200;
    
    //高度
    public const float LandingHeight = 100.0f;

    //滑铲所需能量
    public const double SlidingEnergy = 4.0d;

    public const string SavePath = "user://data.sav";
    public const string ConfigPath = "user://config.ini";
}