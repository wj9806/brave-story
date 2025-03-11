using Godot;

namespace bravestory.scripts;

/// <summary>
/// 传送器:用于切换场景
/// </summary>
[GlobalClass]
public partial class Teleporter : Interactable
{
    [Export(PropertyHint.File, "*.tscn")]
    public string Path { get; set; }
    
    [Export]
    public string EntryPoint { get; set; }
    
    public override void InteractHandler()
    {
        base.InteractHandler();
        //切换场景
        var game = GetNode<Game>("/root/Game");
        game.ChangeScene(Path, EntryPoint);
    }
}