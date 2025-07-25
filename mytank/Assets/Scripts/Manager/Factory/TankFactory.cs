using System.Collections.Generic;
using UnityEngine;

public class TankFactory : SingletonMono<TankFactory>
{
    public TankController tankPrefab; // 改为 TankController 类型
    public ObjectPool<TankController> TankPool { get; private set; }

    public List<TankController> activeTanks;
    
    
    public List<TankData> orignalDatas;
    
    
    private void Awake()
    {
        TankPool = new ObjectPool<TankController>(
            prefab: tankPrefab,
            onSpawn: CreateTank,
            onDespawn: KillTank
        );
        activeTanks = new List<TankController>();
    }

    private void CreateTank(TankController tank)
    {
        tank.isDead=false;
        tank.lastMoveTime=0;
        tank.lastShootTime = 0;
        tank.lastAnimStartTime = 0;
        tank.isCanBreakWall = false;
        tank.isCanShootTwice = false;
        tank.invincibleFrame = 0;
        tank.killNum = 0;
        activeTanks.Add(tank);
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
           
        }
        
        activeTanks.Remove(tank);
    }


  
    
    public TankController InitialPlayer(string tankID, string tankName, int x, int y, Color color)
    {
        TankController temp=TankPool.GetObject();


        temp. tankID = tankID;
        temp. tankDirection = Direction.Up;
        temp. Pos = new Vector2Int(x, y); // 左下角坐标

        if (tankID == NetworkManager.Instance.playerID)
        {
            temp.identity=Identity.Myself;
        }
        else
        {
            temp.identity=Identity.OtherPlayer;
            
        }
        temp. playerName = tankName;
        
        // 设置坦克中心位置
        temp.  transform.position = temp. GetCenter();
        temp. playerColor= color;
        temp.  GetComponent<SpriteRenderer>().material.color = color;
        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);
        
        temp.random = new System.Random( seed );
        
   
        
        temp. currentData=ScriptableObject.CreateInstance<TankData>();
        temp. currentData.InitializeTankData(orignalDatas[0]);
        
     
            
        temp. playerPanelUI= Instantiate(GameUIManager.Instance.playerPanelPrefab, GameUIManager.Instance.playerPanelParent).GetComponent<PlayerPanelUI>();
        temp. playerPanelUI.UpdateUI(temp);
        temp. playerPanelUI.SetPos(PlayerManager.Instance.activePlayers.Count);  
        temp.animType = 1;
            
        PlayerManager.Instance.activePlayers.Add(temp);
       


        return temp;
    }


    public TankController InitialEnemy(string tankID, string tankName, int x, int y, Color color, int dataIndex)
    {
        TankController temp=TankPool.GetObject();

       

        temp. tankID = tankID;
        temp. tankDirection = Direction.Up;

        temp. Pos = new Vector2Int(x, y); // 左下角坐标

        temp.identity = Identity.Enemy;
        temp. playerName = tankName;
        
        // 设置坦克中心位置
        temp.  transform.position = temp. GetCenter();
        temp. playerColor= color;
        temp.  GetComponent<SpriteRenderer>().material.color = color;
        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);
        
        temp.random = new System.Random( seed );

        
        temp. currentData=ScriptableObject.CreateInstance<TankData>();
        temp. currentData.InitializeTankData(orignalDatas[dataIndex]);
        
      
            temp.animType = dataIndex;

        
        EnemyManager.Instance.activeEnemies.Add(temp);


        return temp;
    }
    public TankController InitialSpecialEnemy(string playerID, string playerName, int x, int y, 
        int dataIndex)
    {
        TankController temp=TankPool.GetObject();
        

        temp. tankID = playerID;
        temp. tankDirection = Direction.Up;
        temp. Pos = new Vector2Int(x, y); // 左下角坐标
        
        temp.identity = Identity.Enemy;

        temp. playerName = playerName;
        
        // 设置坦克中心位置
        temp.  transform.position = temp. GetCenter();
        
        int seed =
            (int)(NetworkManager.Instance.seed +
                  NetworkManager.Instance.currentFrame);
        
        temp.random = new System.Random( seed );
        
   
        
        temp. currentData=ScriptableObject.CreateInstance<TankData>();
        temp. currentData.InitializeTankData(orignalDatas[dataIndex]);
        

        temp.animType = dataIndex;

        temp.gameObject.AddComponent<Special>();

        EnemyManager.Instance.activeEnemies.Add(temp);
        return temp;
    }
    
    public void ClearAllTank()
    {
        // 创建副本，避免遍历时修改集合
        var tanks = new List<TankController>(activeTanks);
        foreach (var tank in tanks)
        {
           TankPool.ReturnObject(tank);
        }
 
    }
    
        
}