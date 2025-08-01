// 地图实体基类（有位置和渲染器）

using UnityEngine;

public abstract class MapEntity : MonoBehaviour, IEntity
{
    public PlayerTankController tank;
    public Vector2Int Pos;

    public abstract void UpdateFrame();

    public virtual void Init(PlayerTankController _tank, Vector2Int pos)
    {
        tank = _tank;
        Pos = pos;
        GetComponent<SpriteRenderer>().material.color = tank.playerColor;
    }

    public virtual void Destroy()
    {
        EntityManager.Instance.RemoveEntity(this);
        Destroy(gameObject);
    }
}