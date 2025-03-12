using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace bravestory.scripts;

public partial class World : Node2D
{
    
    private TileMapLayer _tileMapLayer;
    private Camera2D _camera2D;
    private Player _player;

    public override void _Ready()
    {
        _tileMapLayer = GetNode<TileMapLayer>("TileMapLayer");      
        _camera2D = GetNode<Camera2D>("Player/Camera2D");
        _player = GetNode<Player>("Player");

        //设置相机边界
        var usedRect = _tileMapLayer.GetUsedRect().Grow(-1);
        var tileSize = _tileMapLayer.TileSet.TileSize;
        _camera2D.LimitTop = usedRect.Position.Y * tileSize.Y + (int)Math.Floor(_tileMapLayer.Position.Y);
        _camera2D.LimitBottom = usedRect.End.Y * tileSize.Y + (int)Math.Floor(_tileMapLayer.Position.Y);
        _camera2D.LimitLeft = usedRect.Position.X * tileSize.X + (int)Math.Floor(_tileMapLayer.Position.X);
        _camera2D.LimitRight = usedRect.End.X * tileSize.X + (int)Math.Floor(_tileMapLayer.Position.X);
        _camera2D.ResetSmoothing();
    }

    public void UpdatePlayer(Vector2 vector2, Direction direction)
    {
        _player.GlobalPosition = vector2;
        _player.FallFromY = vector2.Y;
        _player.Direction = direction;
        _camera2D.ResetSmoothing();
        _camera2D.ForceUpdateScroll();
    }

    public Hashtable ToDict()
    {
        var hashtable = new Hashtable();
        var nodes = GetTree().GetNodesInGroup("enemies");

        List<String> list = [];
        list.AddRange(nodes.Select(node => GetPathTo(node).ToString()));
        hashtable.Add("enemies_alive", list);
        return hashtable;
    }

    public void FromDict(Hashtable dict)
    {
        var nodePaths = dict["enemies_alive"] as List<String>;

        foreach (var node in GetTree().GetNodesInGroup("enemies"))
        {
            var path = GetPathTo(node).ToString();

            if (nodePaths != null && !nodePaths.Contains(path))
            {
                node.QueueFree();
            }
        }
    }
}