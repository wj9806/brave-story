using Godot;

public class Constants
{
    //重力
    public static double GRAVITY = ProjectSettings.GetSetting("physics/2d/default_gravity").AsDouble();

    //水平移动速度
    public static int RUN_SPEED = 150;
    
    //跳跃速度
    public static int JUMP_VELOCITY = -320;
    
    //角色地面加速度
    public static float FLOOR_ACCELERATION = RUN_SPEED / 0.2f;
    
    //角色空中加速度
    public static float AIR_ACCELERATION = RUN_SPEED / 0.02f;
}