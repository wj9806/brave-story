using Godot;

namespace bravestory.scripts;

public partial class Health : HBoxContainer
{
 
    protected TextureProgressBar _healthBar;
    protected TextureProgressBar _easedHealthBar;

    [Export]
    public Stats Stats { get; set; }

    protected void UpdateHeath(bool skipAnimation)
    {
        var percentage = (float)Stats.Health / Stats.MaxHealth;
        _healthBar.Value = percentage;

        if (skipAnimation)
            _easedHealthBar.Value = percentage;
        else
            CreateTween().TweenProperty(_easedHealthBar, "value", percentage, 0.5);
    }

    public override void _Ready()
    {
        _healthBar = GetNode<TextureProgressBar>("HealthBar");
        _easedHealthBar = _healthBar.GetNode<TextureProgressBar>("EasedHealthBar");
        Stats.Connect(Stats.SignalName.HealthChanged, Callable.From(() => UpdateHeath(false)));
        UpdateHeath(true);
    }
}