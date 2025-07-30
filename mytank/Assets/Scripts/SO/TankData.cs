using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TankData", menuName = "Tank Game/TankData")]
public class TankData : ScriptableObject
{
     public int moveIntervalFrame ;
     public int shootIntervalFrame ;
    public int HP;
    public int orignalHP;
   
    public void InitializeTankData(TankData orignalData)
    {
        moveIntervalFrame = orignalData.moveIntervalFrame;
        shootIntervalFrame = orignalData.shootIntervalFrame;
        HP = orignalData.HP;
        orignalHP = orignalData.orignalHP;
    }
}