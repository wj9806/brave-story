using Godot;

namespace bravestory.scripts;

public partial class PauseScreen : Control
{
	private Button _resume;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
		SoundManager.Instance.SetUpUiSounds(this);
		
		_resume = GetNode<Button>("V/Actions/H/Resume");
		
		Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(() =>
		{
			GetTree().Paused = Visible;
		}));
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("pause"))
		{
			OnResumePressed();
			GetWindow().SetInputAsHandled();
		}
	}

	public void ShowPause()
	{
		Show();
		_resume.GrabFocus();
	}

	private void OnResumePressed()
	{
		Hide();
	}

	private void OnQuitPressed()
	{
		Game.Instance.BackToTitle();
	}
}