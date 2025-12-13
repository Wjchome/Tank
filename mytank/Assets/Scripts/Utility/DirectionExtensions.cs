// Direction枚举的扩展方法

using FixMath.NET;
using Tankgame;
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
    
    public static (FixVector2,Fix64) ToFixVector2(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return (FixVector2.Up, Fix64.Zero);
            case Direction.Down: return (FixVector2.Down,(Fix64)180);
            case Direction.Left: return (FixVector2.Left,(Fix64)90);
            case Direction.Right: return (FixVector2.Right,(Fix64)(-90));
            case Direction.LeftUp: return (new FixVector2(-Fix64.One, Fix64.One).Normalized(),(Fix64)45);
            case Direction.LeftDown: return (new FixVector2(-Fix64.One, -Fix64.One).Normalized(),(Fix64)135);
            case Direction.RightUp: return (new FixVector2(Fix64.One, Fix64.One).Normalized(),(Fix64)(-45));
            case Direction.RightDown: return (new FixVector2(Fix64.One, -Fix64.One).Normalized(),(Fix64)(-135));
            default: return (FixVector2.Zero, Fix64.One);
        }
    }

    public static Direction ToDirection(this Vector2Int vec)
    {
        // 根据Vector2Int的x、y分量判断方向
        if (vec == new Vector2Int(0, 1))
        {
            return Direction.Up;
        }
        else if (vec == new Vector2Int(0, -1))
        {
            return Direction.Down;
        }
        else if (vec == new Vector2Int(-1, 0))
        {
            return Direction.Left;
        }
        else if (vec == new Vector2Int(1, 0))
        {
            return Direction.Right;
        }
        else if (vec == new Vector2Int(-1, 1))
        {
            return Direction.LeftUp;
        }
        else if (vec == new Vector2Int(-1, -1))
        {
            return Direction.LeftDown;
        }
        else if (vec == new Vector2Int(1, 1))
        {
            return Direction.RightUp;
        }
        else if (vec == new Vector2Int(1, -1))
        {
            return Direction.RightDown;
        }
        else
        {
            // 无效向量返回None（或根据你的需求返回默认值，比如Up）
            return Direction.Up;
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
            case Direction.LeftUp: return Direction.RightDown;
            case Direction.RightDown: return Direction.LeftUp;
            case Direction.LeftDown: return Direction.RightUp;
            case Direction.RightUp: return Direction.LeftDown;
            default: return Direction.Up;
        }
    }

    /// <summary>
    /// 将 Direction 转换为 InputType（用于网络协议）
    /// </summary>
    public static InputType ToInputType(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up: return InputType.InputMoveUp;
            case Direction.Down: return InputType.InputMoveDown;
            case Direction.Left: return InputType.InputMoveLeft;
            case Direction.Right: return InputType.InputMoveRight;
            case Direction.LeftUp: return InputType.InputMoveUpLeft;
            case Direction.RightUp: return InputType.InputMoveUpRight;
            case Direction.LeftDown: return InputType.InputMoveDownLeft;
            case Direction.RightDown: return InputType.InputMoveDownRight;
            default: return InputType.InputNone;
        }
    }
}