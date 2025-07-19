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
        // 主菜单按钮
        createRoomButton.onClick.AddListener(ShowCreateRoom);
        joinRoomButton.onClick.AddListener(ShowRoomList);
        quitButton.onClick.AddListener(QuitGame);
        
        // 创建房间按钮
        createButton.onClick.AddListener(CreateRoom);
        cancelCreateButton.onClick.AddListener(ShowMainMenu);
        
        // 房间列表按钮
        refreshRoomListButton.onClick.AddListener(RequestRoomListFromServer);
        backToMainButton.onClick.AddListener(ShowMainMenu);
        
        // 房间内按钮
        leaveRoomButton.onClick.AddListener(LeaveRoom);
        startGameButton.onClick.AddListener(StartGame);
        
        // 网络事件
        NetworkManager.Instance.OnRoomListReceived += OnRoomListReceived;
       NetworkManager.Instance.OnRoomInfoUpdate += OnRoomInfoUpdate;
        NetworkManager.Instance.OnGameStart += OnGameStartRoom;
        NetworkManager.Instance.OnGameStart += OnGameStartFun;
    }

    void RequestRoomListFromServer()
    {
        NetworkManager.Instance.RequestRoomList();
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
        RequestRoomListFromServer(); // 主动请求服务器获取房间列表
    }
    
    void ShowRoom()
    {
        Debug.Log("=== ShowRoom called ===");
        Debug.Log($"Setting mainMenuPanel.active = false");
        mainMenuPanel.SetActive(false);
        Debug.Log($"Setting roomListPanel.active = false");
        roomListPanel.SetActive(false);
        Debug.Log($"Setting createRoomPanel.active = false");
        createRoomPanel.SetActive(false);
        Debug.Log($"Setting roomPanel.active = true");
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
    
    // 房间操作
    void CreateRoom()
    {
        string roomName = roomNameInput.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"Room_{Random.Range(1000, 9999)}";
        }
        
        int maxPlayers = maxPlayersDropdown.value + 1; //1， 2, 3, 4
        NetworkManager.Instance.CreateRoom(roomName, maxPlayers);
    }
    
    void JoinRoom(string roomId)
    {
        NetworkManager.Instance.JoinRoom(roomId);
    }
    
    void LeaveRoom()
    {
        if (NetworkManager.Instance.currentRoom != null)
        {
            // 发送离开房间请求
            NetworkManager.Instance.LeaveRoom(NetworkManager.Instance.currentRoom.RoomId);
            NetworkManager.Instance.currentRoom = null;
        }
        
        // 返回主菜单
        ShowMainMenu();
    }
    
    void StartGame()
    {
        NetworkManager.Instance.GameStartRequest(NetworkManager.Instance.currentRoom.RoomId);
        
    }
    
    void ClearRoomList()
    {
        // 不再主动刷新本地缓存，而是等服务器返回
        // 这里只清空UI，等待OnRoomListReceived刷新
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
        // 显示房间内玩家
        foreach (var playerId in NetworkManager.Instance.currentRoom.PlayerIds)
        {
            PlayerUIItem playerUIItem = Instantiate(playerUIItemPrefab, playerListContent).GetComponent<PlayerUIItem>();
            playerUIItems.Add(playerUIItem);
            
            playerUIItem.playerNameText.text = playerId.Substring(0, 8) + "...";
            
            if (playerId == NetworkManager.Instance.currentRoom.HostId)
            {
                playerUIItem.playerStatusText.text = "Host";
                playerUIItem.playerStatusText.color = Color.yellow;
            }
            else
            {
                playerUIItem.playerStatusText.text = "Player";
                playerUIItem.playerStatusText.color = Color.white;
            }
            
            
        RectTransform rectTransform = playerUIItem.transform.GetComponent<RectTransform>();
        rectTransform.anchoredPosition=new Vector2(0,-100*index++);
            
        }
    }
    
    // 事件处理
    void OnRoomListReceived(List<RoomInfo> rooms)
    {
        ClearRoomList();
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
            // 离开房间成功
            Debug.Log("=== Left room successfully ===");
            ShowMainMenu();
            return;
        }
        
        Debug.Log($"=== OnRoomInfoUpdate called ===");
        Debug.Log($"RoomId: {roomInfo.RoomId}");
        Debug.Log($"RoomName: {roomInfo.RoomName}");
        Debug.Log($"Status: {roomInfo.Status}");
        Debug.Log($"PlayerCount: {roomInfo.PlayerIds.Count}/{roomInfo.MaxPlayers}");
        
        NetworkManager.Instance.currentRoom = roomInfo;
        Debug.Log("Calling ShowRoom()...");
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