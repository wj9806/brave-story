using Godot;
using System;

public partial class World : Node2D
{
    
    private TileMapLayer tileMapLayer;
    private Camera2D camera2D;

    public override void _Ready()
    {
        tileMapLayer = GetNode<TileMapLayer>("TileMapLayer");      
        camera2D = GetNode<Camera2D>("Player/Camera2D");

        //设置相机边界
        var usedRect = tileMapLayer.GetUsedRect().Grow(-1);
        var tileSize = tileMapLayer.TileSet.TileSize;
        camera2D.LimitTop = usedRect.Position.Y * tileSize.Y + (int)Math.Floor(tileMapLayer.Position.Y);
        camera2D.LimitBottom = usedRect.End.Y * tileSize.Y + (int)Math.Floor(tileMapLayer.Position.Y);
        camera2D.LimitLeft = usedRect.Position.X * tileSize.X + (int)Math.Floor(tileMapLayer.Position.X);
        camera2D.LimitRight = usedRect.End.X * tileSize.X + (int)Math.Floor(tileMapLayer.Position.X);
        camera2D.ResetSmoothing();
    }

}
