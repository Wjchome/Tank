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
   public List<Vector2Int> enemyTankPawns ;//敌对坦克生成点
    public List<Vector2Int> playerTankPawns ;//坦克生成点
    public int enemyNum;
} 