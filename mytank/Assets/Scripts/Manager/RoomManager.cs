using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tankgame;
using System.Linq;
using TMPro;
using UnityEngine.Serialization;

public class RoomManager : SingletonMono<RoomManager>
{
    [Header("UI References")]
    public GameObject mainMenuPanel;
    public GameObject roomListPanel;
    public GameObject createRoomPanel;
    public GameObject roomPanel;
    
    [Header("Main Menu")]
    public TMP_InputField myNameInput;
    public Slider myColorR; 
    public Slider myColorG; 
    public Slider myColorB;
    public Image colorShow;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button quitButton;
    
    
    [Header("Create Room")]
    public TMP_InputField roomNameInput;
    public TMP_Dropdown maxPlayersDropdown;
    public Button createButton;
    public Button cancelCreateButton;
    
    [Header("Room List")]
    public Transform roomListContent;
    public GameObject roomUIItemPrefab;
    public Button refreshRoomListButton;
    public Button backToMainButton;
    
    [Header("Room")]
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI playerCountText;
    public TextMeshProUGUI hostText;
    public Transform playerListContent;
    public GameObject playerUIItemPrefab;
    public Button leaveRoomButton;
    public Button startGameButton;
    
    private List<RoomUIItem> roomItems = new List<RoomUIItem>();
    private List<PlayerUIItem> playerUIItems = new List<PlayerUIItem>();

   
    
    void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        ShowMainMenu();
        
       
    }
    
    void InitializeUI()
    {
        // 设置最大玩家数下拉菜单
        maxPlayersDropdown.ClearOptions();
        maxPlayersDropdown.AddOptions(new List<string> { "1","2", "3", "4" });
        maxPlayersDropdown.value = 0; // 默认2人
        
        // 设置默认房间名
        roomNameInput.text = $"Room_{Random.Range(1000, 9999)}";
        
   
    }
    
    void SubscribeToEvents()
    {
        // 主菜单按钮 - 只负责UI切换
        createRoomButton.onClick.AddListener(ShowCreateRoom);    //本地切换创建页面
        joinRoomButton.onClick.AddListener(SendRoomListRequest); //仅发送房间列表请求
        quitButton.onClick.AddListener(QuitGame);                //本地退出
        myColorR.onValueChanged.AddListener(ColorShow);          
        myColorG.onValueChanged.AddListener(ColorShow);
        myColorB.onValueChanged.AddListener(ColorShow);
        
        // 创建房间按钮 - 只发送请求
        createButton.onClick.AddListener(SendCreateRoomRequest); //发出创建列表请求
        cancelCreateButton.onClick.AddListener(ShowMainMenu);    //本地切回主菜单
        
        // 房间列表按钮 - 只发送请求
        refreshRoomListButton.onClick.AddListener(SendRoomListRequest);//发出房间列表请求
        backToMainButton.onClick.AddListener(ShowMainMenu); //本地切回主页面
        
        // 房间内按钮 - 只发送请求
        leaveRoomButton.onClick.AddListener(SendLeaveRoomRequest); //发送离开请求
        startGameButton.onClick.AddListener(SendStartGameRequest); //发送游戏开始
        
        // 网络事件 - 处理UI更新
        
        NetworkManager.Instance.OnRoomListReceived += OnRoomListReceived;
        NetworkManager.Instance.OnRoomInfoUpdate += OnRoomInfoUpdate;
        NetworkManager.Instance.OnGameStart += OnGameStartRoom;
        NetworkManager.Instance.OnGameStart += OnGameStartFun;
    }
    
    void OnDestroy()
    {
        
            NetworkManager.Instance.OnRoomListReceived -=  OnRoomListReceived;
          NetworkManager.Instance.OnRoomInfoUpdate -= OnRoomInfoUpdate;
            NetworkManager.Instance.OnGameStart -= OnGameStartRoom;
            NetworkManager.Instance.OnGameStart -= OnGameStartFun;
        
    }

    void ColorShow(float colorValue)
    {
        colorShow.color=new Color(myColorR.value,myColorG.value,myColorB.value);
    }

    #region UI显示控制

    

    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        roomListPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
      
    }

    void ShowCreateRoom()
    {
        mainMenuPanel.SetActive(false);
        roomListPanel.SetActive(false);
        createRoomPanel.SetActive(true);
        roomPanel.SetActive(false);
    }
    
    void ShowRoomList()
    {
        mainMenuPanel.SetActive(false);
        roomListPanel.SetActive(true);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
        
        
    }
    
    void ShowRoom()
    {
        
        mainMenuPanel.SetActive(false);
        roomListPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(true);
        
        if (NetworkManager.Instance.isHost)
        {
            startGameButton.interactable = true;
        }
        else
        {
            startGameButton.interactable = false;
        }
    }
    
    #endregion

    #region 发送消息

    

    // 发送创建房间请求
    void SendCreateRoomRequest()
    {
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"Room_{Random.Range(1000, 9999)}";
        }
        
        int maxPlayers = maxPlayersDropdown.value + 1;
        var (playerName, colorR, colorG, colorB) = GetPlayerInfo();
        
        NetworkManager.Instance.CreateRoom(roomName, maxPlayers, playerName, colorR, colorG, colorB);
    }
    
    // 发送房间列表请求
    void SendRoomListRequest()
    {
        NetworkManager.Instance.RequestRoomList();
    }
    //发送离开房间请求
    void SendLeaveRoomRequest()
    {
        
        NetworkManager.Instance.LeaveRoom(NetworkManager.Instance.currentRoom.RoomId);
        
    }
    //发送开始游戏请求
    void SendStartGameRequest()
    {
        if (NetworkManager.Instance.currentRoom != null)
        {
            NetworkManager.Instance.GameStartRequest(NetworkManager.Instance.currentRoom.RoomId);
        }
    }
    //发送加入房间请求
    void SendJoinRoom(string roomId)
    {
        var (playerName, colorR, colorG, colorB) = GetPlayerInfo();
        NetworkManager.Instance.JoinRoom(roomId, playerName, colorR, colorG, colorB);
    }
    #endregion
    
    // 获取玩家信息的辅助方法
    (string playerName, int colorR, int colorG, int colorB) GetPlayerInfo()
    {
        string playerName = myNameInput.text;
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player";
        }
        
        int colorR = (int)(myColorR.value * 255);
        int colorG = (int)(myColorG.value * 255);
        int colorB = (int)(myColorB.value * 255);
        
        return (playerName, colorR, colorG, colorB);
    }
    

    
    void CreateRoomUIItem(RoomInfo room,int index)
    {
        
        var roomUIItem = Instantiate( roomUIItemPrefab, roomListContent).GetComponent<RoomUIItem>();
        roomItems.Add(roomUIItem);
        
        // 设置房间信息
    
        roomUIItem.roomNameText.text = room.RoomName;
        roomUIItem.playerCountText.text = $"{room.PlayerIds.Count}/{room.MaxPlayers}";
        
        // 使用HostName，如果没有则使用截断的HostId
        roomUIItem.hostText.text = $"Host: {room.HostName}";
        
        // 如果房间满了，禁用加入按钮
        if (room.PlayerIds.Count >= room.MaxPlayers)
        {
            roomUIItem.joinButton.interactable = false;
            roomUIItem.joinButton.GetComponentInChildren<TextMeshProUGUI>().text = "Full";
        }
        else
        {
            roomUIItem.joinButton.onClick.AddListener(() => SendJoinRoom(room.RoomId));
        }
        //设置房间位置
        RectTransform rectTransform = roomUIItem.transform.GetComponent<RectTransform>();
        rectTransform.anchoredPosition=new Vector2(0,-100*index);
        
    }
    
    
    void UpdatePlayerList()
    {
        // 清空现有玩家列表
        foreach (var item in playerUIItems)
        {
            Destroy(item.gameObject);
        }
        playerUIItems.Clear();
        
     
        //
        int index = 0;
        foreach (var playerInfo in NetworkManager.Instance.currentRoom.PlayerInfos)
        {
            PlayerUIItem playerUIItem =  Instantiate(playerUIItemPrefab, playerListContent).GetComponent<PlayerUIItem>();
            playerUIItems.Add(playerUIItem);
        
            // 设置基本信息
            playerUIItem.playerNameText.text = playerInfo.PlayerName;
            SetPlayerStatus(playerUIItem);
            SetupKickButton(playerUIItem, playerInfo.PlayerId);
        
            // 设置位置
            RectTransform rectTransform = playerUIItem.transform.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, -100 * index++);
            // 设置玩家颜色
            Color playerColor = new Color(
                playerInfo.ColorR / 255f,
                playerInfo.ColorG / 255f,
                playerInfo.ColorB / 255f
            );
            playerUIItem.playerColor.color = playerColor;
        }
        
       
    }
    

    

   
    
    void SetPlayerStatus(PlayerUIItem playerUIItem)
    {
        if (NetworkManager.Instance.isHost)
        {
            playerUIItem.playerStatusText.text = "Host";
            playerUIItem.playerStatusText.color = Color.yellow;
        }
        else
        {
            playerUIItem.playerStatusText.text = "Player";
            playerUIItem.playerStatusText.color = Color.white;
        }
    }
    
    void SetupKickButton(PlayerUIItem playerUIItem, string playerId)
    {
        // 设置踢人按钮（只有房主可以看到，且不能踢自己）
        bool isHost = NetworkManager.Instance.isHost;
        bool isSelf = playerId == NetworkManager.Instance.playerID;
        bool canKick = isHost && !isSelf && playerId != NetworkManager.Instance.currentRoom.HostId;
        
        playerUIItem.kickButton.gameObject.SetActive(canKick);
        if (canKick)
        {
            // 清除之前的监听器，避免重复添加
            playerUIItem.kickButton.onClick.RemoveAllListeners();
            // 添加踢人监听器
            string targetPlayerId = playerId; // 捕获变量
            playerUIItem.kickButton.onClick.AddListener(() => {
                NetworkManager.Instance.KickPlayerRequest(NetworkManager.Instance.currentRoom.RoomId, targetPlayerId);
            });
        }
    }

    #region Event

    

    // 事件处理
    void OnRoomListReceived(List<RoomInfo> rooms)
    {
        ShowRoomList();
        // 清空UI，等待OnRoomListReceived刷新
        foreach (var item in roomItems)
        {
            Destroy(item.gameObject);
        }
        roomItems.Clear();
        int index = 0;
        foreach (var room in rooms)
        {
            if (room.Status == "waiting")
            {
                CreateRoomUIItem(room, index++);
            }
        }
        
    }
    
 
    
   
    
    void OnRoomInfoUpdate(RoomInfo roomInfo)
    {
       
        
        // 更新当前房间信息
        NetworkManager.Instance.currentRoom = roomInfo;
        
        // 如果房间信息有错误状态，显示错误
        if (roomInfo.Status == "error")
        {
            Debug.LogError($"Room error: {roomInfo.RoomName}");
            // 错误处理：恢复按钮状态或显示错误信息
            return;
        }
        
        // 成功响应：显示房间界面并更新信息
        ShowRoom();
        
        // 更新房间信息显示
        roomNameText.text = roomInfo.RoomName;
        playerCountText.text = $"{roomInfo.PlayerIds.Count}/{roomInfo.MaxPlayers}";
        
        // 使用HostName，如果没有则使用截断的HostId
        hostText.text = $"Host: {roomInfo.HostName }";
        
        // 更新玩家列表
        UpdatePlayerList();
        
        // 只有房主可以开始游戏
        startGameButton.gameObject.SetActive(NetworkManager.Instance.isHost);
        Debug.Log($"Start button active: {NetworkManager.Instance.isHost}");
    }
    
    void OnGameStartRoom(GameStart gameStart)
    {
        Debug.Log("Game starting...");
        // 隐藏UI，开始游戏
        mainMenuPanel.SetActive(false);
        roomListPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
    }
    
    void OnGameStartFun(GameStart gameStart)
    {
        Debug.Log($"Game started! Room: {gameStart.RoomId}, Players: {string.Join(",", gameStart.PlayerIds)}");
        foreach (var playerId in gameStart.PlayerIds)
        {
            if (!GameStateManager.Instance.playerTanks.ContainsKey(playerId))
            {
                GameStateManager.Instance.CreatePlayerTank(playerId);
            }
        }
        // 用服务器下发的随机种子初始化Unity随机数
        UnityEngine.Random.InitState((int)gameStart.RandomSeed);
        // 随机生成一个敌人坦克
        Vector2 enemyPos = new Vector2(UnityEngine.Random.Range(3, 8), UnityEngine.Random.Range(3, 8));
        GameObject enemyTank = GameObject.Instantiate(GameManager.Instance.tankPrefab, enemyPos, Quaternion.identity);
        enemyTank.GetComponent<Renderer>().material.color = Color.red; // 敌人坦克用红色区分
        enemyTank.name = "EnemyTank";
    }
    
    #endregion
    
    
    
    void QuitGame()
    {
        Application.Quit();
    }
} 