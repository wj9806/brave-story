using Godot;

namespace bravestory.scripts;

[GlobalClass]
public partial class EntryPoint : Marker2D
{

    [Export] public Direction Direction = Direction.Right;
    
    public override void _Ready()
    {
        AddToGroup("entry_points");
    }
}