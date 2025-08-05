using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Level", menuName = "Tank Game/Level")]
public class Level : ScriptableObject
{
    public string levelName;
    public int width;
    public int height;

    [TextArea] public string mapData;
    public List<Vector2Int> enemyTankPawns; //敌对坦克生成点
    public List<Vector2Int> playerTankPawns; //坦克生成点
    public int enemySum;//敌人总数
    public int enemySpawnFrame;// 敌人生成间隔
    public int maxEnemies; // 场上最大敌人数量
    public int specialRate;//特殊敌人比例 1/n
    public List<LevelEnemy>  enemiesUp;
    [System.Serializable]
    public class LevelEnemy
    {
        public int numEnemies;//在产生这么多怪的情况下做什么
        public EnemyEvent enemyEvent;
    }
    

}

public enum EnemyEvent
{
    HP_Add_20,
    Shoot_Interval_1,
    Move_Interval_1
}