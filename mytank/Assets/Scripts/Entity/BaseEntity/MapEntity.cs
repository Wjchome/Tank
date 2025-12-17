// 地图实体基类（有位置和渲染器）

using Physics2D;
using UnityEngine;

public abstract class MapEntity : MonoBehaviour, IEntity
{
    public TankController tank;
    public FixRect myFixRect;
    public SpriteRenderer  spriteRenderer;
    

    public virtual void Init(PlayerTankController _tank, FixRect fixRect)
    {
        tank = _tank;
        myFixRect = fixRect;
        spriteRenderer.material.color = tank.playerColor;
        transform.position = (Vector2)fixRect.Center;
    }

    public abstract void UpdateFrame();
    public virtual void Destroy()
    {
        EntityManager.Instance.RemoveEntity(this);
        Destroy(gameObject);
    }
}