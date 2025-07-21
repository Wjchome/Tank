// Direction枚举的扩展方法

using UnityEngine;

public static class DirectionExtensions
{
    public static Vector2Int ToVector2Int(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Vector2Int.up;
            case Direction.Down: return Vector2Int.down;
            case Direction.Left: return Vector2Int.left;
            case Direction.Right: return Vector2Int.right;
            default: return Vector2Int.zero;
        }
    }
    
    public static Direction Opposite(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return Direction.Down;
            case Direction.Down: return Direction.Up;
            case Direction.Left: return Direction.Right;
            case Direction.Right: return Direction.Left;
            default: return Direction.Up;
        }
    }
}