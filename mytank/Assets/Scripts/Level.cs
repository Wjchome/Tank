using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Tank Game/Level")]
public class Level : ScriptableObject
{
    
    public string levelName;
    public int width;
    public int height;
    public string mapData;
} 