using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TankData", menuName = "Tank Game/TankData")]
public class TankData : ScriptableObject
{
    public float moveInterval ;
    public float shootInterval ;
    public int HP;
   
    public void InitializeTankData(TankData orignalData)
    {
        moveInterval = orignalData.moveInterval;
        shootInterval = orignalData.shootInterval;
        HP = orignalData.HP;
    }
}