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
        createRoomButton.onClick.AddListener(ShowCreateRoom);
        joinRoomButton.onClick.AddListener(ShowRoomList);
        quitButton.onClick.AddListener(QuitGame);
        myColorR.onValueChanged.AddListener(ColorShow);
        myColorG.onValueChanged.AddListener(ColorShow);
        myColorB.onValueChanged.AddListener(ColorShow);
        
        // 创建房间按钮 - 只发送请求
        createButton.onClick.AddListener(SendCreateRoomRequest);
        cancelCreateButton.onClick.AddListener(ShowMainMenu);
        
        // 房间列表按钮 - 只发送请求
        refreshRoomListButton.onClick.AddListener(SendRoomListRequest);
        backToMainButton.onClick.AddListener(ShowMainMenu);
        
        // 房间内按钮 - 只发送请求
        leaveRoomButton.onClick.AddListener(SendLeaveRoomRequest);
        startGameButton.onClick.AddListener(SendStartGameRequest);
        
        // 网络事件 - 处理UI更新
        NetworkManager.Instance.OnRoomListReceived += OnRoomListReceived;
        NetworkManager.Instance.OnRoomInfoUpdate += OnRoomInfoUpdate;
        NetworkManager.Instance.OnGameStart += OnGameStartRoom;
        NetworkManager.Instance.OnGameStart += OnGameStartFun;
    }
    
    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnRoomListReceived -= OnRoomListReceived;
          NetworkManager.Instance.OnRoomInfoUpdate -= OnRoomInfoUpdate;
            NetworkManager.Instance.OnGameStart -= OnGameStartRoom;
            NetworkManager.Instance.OnGameStart -= OnGameStartFun;
        }
    }

    void ColorShow(float colorValue)
    {
        colorShow.color=new Color(myColorR.value,myColorG.value,myColorB.value);
    }
    
    // UI显示控制
    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        roomListPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
      
    }
    
  
    
   

    void QuitGame()
    {
        Application.Quit();
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
        ClearRoomList(); // 清空UI
        SendRoomListRequest(); // 自动请求房间列表
    }
    
    void ShowRoom()
    {
        
        mainMenuPanel.SetActive(false);
        
        roomListPanel.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(true);
        
        if (NetworkManager.Instance.currentRoom.HostId != NetworkManager.Instance.playerID)
        {
            Debug.Log("I'm not the host, disabling start button");
            startGameButton.interactable = false;
        }
        else
        {
            Debug.Log("I'm the host, enabling start button");
            startGameButton.interactable = true;
        }
        Debug.Log("=== ShowRoom completed ===");
    }
    
    // 房间操作 - 纯发送请求，不包含UI逻辑
    void SendCreateRoomRequest()
    {
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"Room_{Random.Range(1000, 9999)}";
        }
        
        int maxPlayers = maxPlayersDropdown.value + 1;
        NetworkManager.Instance.CreateRoom(roomName, maxPlayers);
    }
    
    void SendRoomListRequest()
    {
        NetworkManager.Instance.RequestRoomList();
    }
    
    void SendLeaveRoomRequest()
    {
        if (NetworkManager.Instance.currentRoom != null)
        {
            NetworkManager.Instance.LeaveRoom(NetworkManager.Instance.currentRoom.RoomId);
        }
    }
    
    void SendStartGameRequest()
    {
        if (NetworkManager.Instance.currentRoom != null)
        {
            NetworkManager.Instance.GameStartRequest(NetworkManager.Instance.currentRoom.RoomId);
        }
    }
    
    void JoinRoom(string roomId)
    {
        NetworkManager.Instance.JoinRoom(roomId);
    }
    
    void ClearRoomList()
    {
        // 清空UI，等待OnRoomListReceived刷新
        foreach (var item in roomItems)
        {
            Destroy(item.gameObject);
        }
        roomItems.Clear();
    }
    
    void CreateRoomUIItem(RoomInfo room,int index)
    {
        
        var roomUIItem = Instantiate( roomUIItemPrefab, roomListContent).GetComponent<RoomUIItem>();
        roomItems.Add(roomUIItem);
        
        // 设置房间信息
    
        roomUIItem.roomNameText.text = room.RoomName;
        roomUIItem.playerCountText.text = $"{room.PlayerIds.Count}/{room.MaxPlayers}";
        roomUIItem.hostText.text = $"Host: {room.HostId.Substring(0, 8)}...";
        
        // 如果房间满了，禁用加入按钮
        if (room.PlayerIds.Count >= room.MaxPlayers)
        {
            roomUIItem.joinButton.interactable = false;
            roomUIItem.joinButton.GetComponentInChildren<TextMeshProUGUI>().text = "Full";
        }
        else
        {
            roomUIItem.joinButton.onClick.AddListener(() => JoinRoom(room.RoomId));
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
        
        if (NetworkManager.Instance.currentRoom == null) return;
        int index = 0;
        
       
            bool isHost = NetworkManager.Instance.playerID == NetworkManager.Instance.currentRoom.HostId;
            foreach (var playerInfo in NetworkManager.Instance.currentRoom.PlayerInfos)
            {
                PlayerUIItem playerUIItem = Instantiate(playerUIItemPrefab, playerListContent).GetComponent<PlayerUIItem>();
                playerUIItems.Add(playerUIItem);
                
                // 显示玩家名字
                playerUIItem.playerNameText.text = playerInfo.PlayerName;
                
                // 设置玩家颜色
                Color playerColor = new Color(
                    playerInfo.ColorR / 255f,
                    playerInfo.ColorG / 255f,
                    playerInfo.ColorB / 255f
                );
               
                playerUIItem.playerColor.color = playerColor;
                if (playerInfo.PlayerId == NetworkManager.Instance.currentRoom.HostId)
                {
                    playerUIItem.playerStatusText.text = "Host";
                    playerUIItem.playerStatusText.color = Color.yellow;
                    
                }
                else
                {
                    playerUIItem.playerStatusText.text = "Player";
                    playerUIItem.playerStatusText.color = Color.white;
                }

                if (isHost&&playerInfo.PlayerId != NetworkManager.Instance.currentRoom.HostId)
                {
                    playerUIItem.kickButton.interactable = true;
                    playerUIItem.kickButton.onClick.AddListener(()=>
                        NetworkManager.Instance.KickPlayer(NetworkManager.Instance.currentRoom.RoomId,playerInfo.PlayerId ));
                }
                else
                {
                    playerUIItem.kickButton.interactable = false;
                }
                
                RectTransform rectTransform = playerUIItem.transform.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0, -100 * index++);
            }
        
        
    }
    
    // 事件处理
    void OnRoomListReceived(List<RoomInfo> rooms)
    {
        // 刷新房间列表后，重新创建UI项
        int index = 0;
        foreach (var room in NetworkManager.Instance.availableRooms)
        {
            if (room.Status == "waiting")
            {
                CreateRoomUIItem(room,index++);
            }
        }
    }
    
 
    
   
    
    void OnRoomInfoUpdate(RoomInfo roomInfo)
    {
        if (roomInfo == null)
        {
            // 可能是离开房间成功，也可能是操作失败
            Debug.Log("=== RoomInfo is null ===");
            
            // 如果当前不在房间中，显示主菜单
            if (NetworkManager.Instance.currentRoom == null)
            {
                ShowMainMenu();
            }
            return;
        }
        
      
        
        NetworkManager.Instance.currentRoom = roomInfo;
        
        ShowRoom();
        
        // 更新房间信息显示
        roomNameText.text = roomInfo.RoomName;
        playerCountText.text = $"{roomInfo.PlayerIds.Count}/{roomInfo.MaxPlayers}";
        hostText.text = $"Host: {roomInfo.HostId.Substring(0, 8)}...";
        
        Debug.Log("Calling UpdatePlayerList()...");
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
} 