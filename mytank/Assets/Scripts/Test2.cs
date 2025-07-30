using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerRole
{
    
    Original,
    Tanker,
    Priest
}
/*
public class Test2 : SingletonMono<Test2>
{
    public ToggleGroup toggleGroup;

    public Image myTank;
    
    public Sprite[] sprites;

    public Dictionary<PlayerRole,Sprite> spriteDict;
    public Dictionary<string, PlayerRole> playerRoleDict;

    
    private void Start()
    {
        spriteDict = new Dictionary<PlayerRole, Sprite>
        {
            { PlayerRole.Original, sprites[0] },
            { PlayerRole.Tanker, sprites[1] },
            { PlayerRole.Priest, sprites[2] },
        };
        playerRoleDict = new Dictionary<string, PlayerRole>
        {
            { "Original", PlayerRole.Original },
            { "Tanker", PlayerRole.Tanker },
            { "Priest", PlayerRole.Priest },
        };
    }

    // 获取当前选中的 Toggle
    public void GetSelectedToggle()
    {
        Toggle selectedToggle = toggleGroup.GetFirstActiveToggle();
        if (selectedToggle != null)
        {
            RoomManager.Instance.playerRole=playerRoleDict[selectedToggle.name];
            myTank.sprite = spriteDict[playerRoleDict[selectedToggle.name]];
            
        }
    }
}*/