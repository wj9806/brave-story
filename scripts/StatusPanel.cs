using Godot;

namespace bravestory.scripts;

public partial class StatusPanel : Health
{
    
    private TextureProgressBar _energyBar;

    public override void _Ready()
    {
        VBoxContainer v = GetNode<VBoxContainer>("V");
        _healthBar = v.GetNode<TextureProgressBar>("HealthBar");
        _energyBar = v.GetNode<TextureProgressBar>("EnergyBar");
        _easedHealthBar = _healthBar.GetNode<TextureProgressBar>("EasedHealthBar");
        //连接信号
        Stats.Connect(Stats.SignalName.HealthChanged, new Callable(this, "UpdateHeath"));
        Stats.Connect(Stats.SignalName.EnergyChanged, new Callable(this, "UpdateEnergy"));
        //初始化血条和能量
        UpdateHeath();
        UpdateEnergy();
    }
    
    private void UpdateEnergy()
    {
        var percentage = (float)Stats.Energy / Stats.MaxEnergy;
        _energyBar.Value = percentage;
    }
}