using Godot;

namespace bravestory.scripts;

[GlobalClass]
public partial class EntryPoint : Marker2D
{
    public override void _Ready()
    {
        AddToGroup("entry_points");
    }
}