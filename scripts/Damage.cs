using Godot;

namespace bravestory.scripts;

public partial class Damage : RefCounted
{

    private int _amount;
    
    private Node2D _source;

    public int Amount { get; set; }
    
    public Node2D Source { get; set; }

}