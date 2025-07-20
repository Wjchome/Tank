using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Tank Game/Level")]
public class Level : ScriptableObject
{
    
    public string levelName;
    public int width;
    public int height;
    public string mapData;
    public List<Vector2Int> tankPawns ;//敌对坦克生成点
    public int enemyNum;
} 