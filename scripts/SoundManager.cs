using Godot;

namespace bravestory.scripts;

public enum Bus
{
	Master = 0, Sfx, Bgm
}

public partial class SoundManager : Node
{

	private Node _sfx;
	private AudioStreamPlayer _bgmPlayer;

	public static SoundManager Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
		_sfx = GetNode<Node>("SFX");
		_bgmPlayer = GetNode<AudioStreamPlayer>("BGMPlayer");
	}

	public void PlaySfx(string name)
	{
		var player = _sfx.GetNode<AudioStreamPlayer>(name);
		if (player == null)
			return;
		player.Play();
	}

	public void PlayBgm(AudioStream stream)
	{
		if (_bgmPlayer.Stream == stream && _bgmPlayer.IsPlaying())
		{
			return;
		}
		
		_bgmPlayer.Stream = stream;
		_bgmPlayer.Play();
	}

	public void SetUpUiSounds(Node node)
	{
		if (node is Button button)
		{
			button.Connect(BaseButton.SignalName.Pressed, Callable.From(() => PlaySfx("UIPress")));
			button.Connect(Control.SignalName.FocusEntered, Callable.From(() => PlaySfx("UIFocus")));
			button.Connect(Control.SignalName.MouseEntered, Callable.From(() => button.GrabFocus()));
		}

		if (node is Slider slider)
		{
			slider.Connect(Range.SignalName.ValueChanged, Callable.From<float>((v) => PlaySfx("UIPress")));
			slider.Connect(Control.SignalName.FocusEntered, Callable.From(() => PlaySfx("UIFocus")));
			slider.Connect(Control.SignalName.MouseEntered, Callable.From(() => slider.GrabFocus()));
		}

		foreach (var child in node.GetChildren())
		{
			SetUpUiSounds(child);
		}
	}

	public float GetVolume(int busIndex)
	{
		var db = AudioServer.GetBusVolumeDb(busIndex);
		//单位转换
		return Mathf.DbToLinear(db);
	}

	public void SetVolume(int busIndex, float volume)
	{
		AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(volume));
	}
}