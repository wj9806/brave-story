using Godot;

namespace bravestory.scripts;

public partial class SaveStone : Interactable
{
	
	private AnimationPlayer _animationPlayer;
	
	public override void _Ready()
	{
		base._Ready();
		_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	public override void _Process(double delta)
	{
	}

	public override void InteractHandler()
	{
		base.InteractHandler();
		_animationPlayer.Play("activated");
		var game = GetTree().GetRoot().GetNode<Game>("/root/Game");
		game.SaveGame();
	}
}