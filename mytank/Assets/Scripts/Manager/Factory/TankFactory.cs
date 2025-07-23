using System.Collections.Generic;
using UnityEngine;

public class TankFactory : SingletonMono<TankFactory>
{
    public TankController tankPrefab; // 改为 TankController 类型
    public ObjectPool<TankController> TankPool { get; private set; }

    public List<TankController> allTanks { get; private set; }
    
    
    public List<TankData> orignalDatas;
    private void Awake()
    {
        TankPool = new ObjectPool<TankController>(
            prefab: tankPrefab,
            onSpawn: CreateTank,
            onDespawn: KillTank
        );
        allTanks = new List<TankController>();
    }

    private void CreateTank(TankController tank)
    {
        tank.isDead=false;
        tank.lastMoveTime=0;
        tank.lastShootTime = 0;
        tank.lastAnimStartTime = 0;
        allTanks.Add(tank);
        tank.gameObject.SetActive(true);
        
        
    }

    private void KillTank(TankController tank)
    {
        tank.gameObject.SetActive(false);
        tank.playerPanelUI = null;
        var a = tank.GetComponent<Special>();
        if (a != null)
        {
            Destroy(a);
            FoodFactory.Instance.Initialize();
        }
        
        allTanks.Remove(tank);
    }


    public TankController Initialize(string playerID, string playerName, int x, int y, Color color, bool isPlayer,
        int dataIndex)
    {
        TankController temp=TankPool.GetObject();


        temp. PlayerID = playerID;
        temp. TankDirection = Direction.Up;
        temp. IsLocalPlayer = playerID == NetworkManager.Instance.playerID;
        temp. Pos = new Vector2Int(x, y); // 左下角坐标
        
        temp. isPlayer = isPlayer;
        temp. playerName = playerName;
        
        // 设置坦克中心位置
        temp.  transform.position = temp. GetCenter();
        temp. playerColor= color;
        temp.  GetComponent<SpriteRenderer>().material.color = color;
        int seed =
            (int)(NetworkManager.Instance.seed +
            NetworkManager.Instance.currentFrame);
        
        temp.random = new System.Random( seed );
        
        GameStateManager.Instance.allTanks[playerID] = temp;
        
        temp. currentData=ScriptableObject.CreateInstance<TankData>();
        temp. currentData.InitializeTankData(orignalDatas[dataIndex]);
        
        if (isPlayer)
        {
            
            temp. playerPanelUI= Instantiate(GameUIManager.Instance.playerPanelPrefab, GameUIManager.Instance.playerPanelParent).GetComponent<PlayerPanelUI>();
            temp. playerPanelUI.UpdateUI(temp);
            temp. playerPanelUI.SetPos(PlayerManager.Instance.activePlayers.Count);   
        }
        else
        {
            temp.animType = dataIndex;

        }


        return temp;
    }


    public TankController InitializeSpecial(string playerID, string playerName, int x, int y, 
        int dataIndex)
    {
        TankController temp=TankPool.GetObject();


        temp. PlayerID = playerID;
        temp. TankDirection = Direction.Up;
        temp. IsLocalPlayer = playerID == NetworkManager.Instance.playerID;
        temp. Pos = new Vector2Int(x, y); // 左下角坐标
        
        temp. isPlayer = false;
        temp. playerName = playerName;
        
        // 设置坦克中心位置
        temp.  transform.position = temp. GetCenter();
        
        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);
        
        temp.random = new System.Random( seed );
        
        GameStateManager.Instance.allTanks[playerID] = temp;
        
        temp. currentData=ScriptableObject.CreateInstance<TankData>();
        temp. currentData.InitializeTankData(orignalDatas[dataIndex]);
        

        temp.animType = dataIndex;

        temp.gameObject.AddComponent<Special>();


        return temp;
    }
    
        
}