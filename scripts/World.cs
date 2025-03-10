using System;
using Godot;

namespace bravestory.scripts;

public partial class World : Node2D
{
    
    private TileMapLayer _tileMapLayer;
    private Camera2D _camera2D;

    public override void _Ready()
    {
        _tileMapLayer = GetNode<TileMapLayer>("TileMapLayer");      
        _camera2D = GetNode<Camera2D>("Player/Camera2D");

        //设置相机边界
        var usedRect = _tileMapLayer.GetUsedRect().Grow(-1);
        var tileSize = _tileMapLayer.TileSet.TileSize;
        _camera2D.LimitTop = usedRect.Position.Y * tileSize.Y + (int)Math.Floor(_tileMapLayer.Position.Y);
        _camera2D.LimitBottom = usedRect.End.Y * tileSize.Y + (int)Math.Floor(_tileMapLayer.Position.Y);
        _camera2D.LimitLeft = usedRect.Position.X * tileSize.X + (int)Math.Floor(_tileMapLayer.Position.X);
        _camera2D.LimitRight = usedRect.End.X * tileSize.X + (int)Math.Floor(_tileMapLayer.Position.X);
        _camera2D.ResetSmoothing();
    }

}