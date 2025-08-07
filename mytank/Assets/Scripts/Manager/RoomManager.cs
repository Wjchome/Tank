using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tankgame;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;

public enum PlayerRole
{
    Original = 1,
    Tanker = 2,
    Priest = 3,
    Soldier = 4
}

public class RoomManager : SingletonMono<RoomManager>
{
    [Header("UI References")] public GameObject mainMenuPanel;
    public GameObject mySettingsPanel;
    public GameObject roomListPanel;
    public GameObject createRoomPanel;
    public GameObject roomPanel;
    public GameObject foodPanel;

    [Header("Main Menu")] public Button mySettingsButton;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button foodIllustrationButton;
    public Button quitButton;

    [Header("MySetting")] public Image colorShow;
    public TMP_InputField myNameInput;
    public Slider myColorR;
    public Slider myColorG;
    public Slider myColorB;
    public Button backButton;
    public ToggleGroup toggleGroup;
    public PlayerRole playerRole;
    public Sprite[] sprites;


    [Header("Create Room")] public TMP_InputField roomNameInput;
    public TMP_Dropdown maxPlayersDropdown;
    public Button createButton;
    public Button cancelCreateButton;

    [Header("Room List")] public Transform roomListContent;
    public GameObject roomUIItemPrefab;
    public Button refreshRoomListButton;
    public Button backToMainButton;

    [Header("Room")] public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI playerCountText;
    public TextMeshProUGUI hostText;
    public Transform playerListContent;
    public GameObject playerUIItemPrefab;
    public Button leaveRoomButton;
    public Button startGameButton;
    public TMP_Dropdown levelDropdown;


    [Header("Food Panel")] public Transform foodPanelContent;
    public Foodillustration foodillustrationPrefab;
    public Button foodBackToMainButton;
    public Image foodIcon;
    public TextMeshProUGUI foodNameShow;
    public TextMeshProUGUI foodDescShow;

    private List<RoomUIItem> roomItems = new List<RoomUIItem>();

    private List<PlayerUIItem> playerUIItems = new List<PlayerUIItem>();


    private List<GameObject> panels = new List<GameObject>();
    private int currentPanelIndex = 0;
    public float durationTime;

    public Dictionary<string, PlayerRole> playerRoleDict = new Dictionary<string, PlayerRole>();
    public Dictionary<PlayerRole, Sprite> playerRoleSpriteDict = new Dictionary<PlayerRole, Sprite>();

    void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        ShowPanel(0);
    }

    public void GetSelectedToggle()
    {
        Toggle selectedToggle = toggleGroup.GetFirstActiveToggle();
        if (selectedToggle != null)
        {
            playerRole = playerRoleDict[selectedToggle.name];
            colorShow.sprite = playerRoleSpriteDict[playerRole];
        }
    }

    void InitializeUI()
    {
        playerRoleDict = new Dictionary<string, PlayerRole>
        {
            { "Original", PlayerRole.Original },
            { "Tanker", PlayerRole.Tanker },
            { "Priest", PlayerRole.Priest },
            { "Soldier", PlayerRole.Soldier },
        };
        playerRoleSpriteDict = new Dictionary<PlayerRole, Sprite>
        {
            { PlayerRole.Original, sprites[0] },
            { PlayerRole.Tanker, sprites[1] },
            { PlayerRole.Priest, sprites[2] },
            { PlayerRole.Soldier, sprites[3] },
        };

        panels = new List<GameObject>
        {
            mainMenuPanel,
            mySettingsPanel,
            createRoomPanel,
            roomListPanel,
            roomPanel,
            foodPanel
        };
        // 设置最大玩家数下拉菜单
        maxPlayersDropdown.ClearOptions();
        maxPlayersDropdown.AddOptions(new List<string> { "1", "2", "3", "4" });
        maxPlayersDropdown.value = 0; // 默认2人

        // 设置默认房间名
        roomNameInput.text = $"Room_{Random.Range(1000, 9999)}";


        levelDropdown.ClearOptions();
        List<string> levelOptions = new List<string>();
        for (int i = 0; i < LevelManager.Instance.availableLevels.Count; i++)
        {
            var level = LevelManager.Instance.availableLevels[i];
            levelOptions.Add($"{i + 1}. {level.levelName}");
        }

        levelDropdown.AddOptions(levelOptions);
        levelDropdown.value = 0; // 默认选择第一个关卡

        bool isFirst = false;
        foreach (var food in FoodManager.Instance.foodDatas.Union(FoodManager.Instance.superFoodDatas))
        {
            var entity = Instantiate(foodillustrationPrefab, foodPanelContent);
            entity.foodIcon.sprite = food.sprite;
            entity.foodButton.onClick.AddListener(() =>
            {
                foodIcon.sprite = food.sprite;
                foodNameShow.text = food.foodName;
                foodDescShow.text = "";
                int i = 1;
                foreach (var desc in food.foodDescription)
                {
                    foodDescShow.text += $"{i++}.{desc}\n";
                }

                AudioManager.Instance.Play("Illustration");
            });
            if (!isFirst)
            {
                foodIcon.sprite = food.sprite;
                foodNameShow.text = food.foodName;
                foodDescShow.text = "";
                int i = 1;
                foreach (var desc in food.foodDescription)
                {
                    foodDescShow.text += $"{i++}.{desc}\n";
                }

                isFirst = true;
            }
        }
    }

    void AnimateButton(Button button)
    {
        var originalScale = button.transform.localScale;
        // 按下动画
        button.transform.DOScale(originalScale * 0.9f, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // 释放动画（带弹性效果）
                button.transform.DOScale(originalScale * 1.1f, 0.3f)
                    .SetEase(Ease.OutElastic)
                    .OnComplete(() =>
                    {
                        // 确保最终恢复精确原始大小
                        button.transform.localScale = originalScale;
                    });
            });
    }

    void SubscribeToEvents()
    {
        // 主菜单按钮 - 只负责UI切换
        mySettingsButton.onClick.AddListener(() =>
        {
            ShowPanel(1);
            AudioManager.Instance.Play("UIClick");
            // AnimateButton(mySettingsButton);
        });
        createRoomButton.onClick.AddListener(() =>
        {
            ShowPanel(2);
            AudioManager.Instance.Play("UIClick");
        }); //本地切换创建页面
        joinRoomButton.onClick.AddListener(() =>
        {
            SendRoomListRequest();
            AudioManager.Instance.Play("UIClick");
        }); //仅发送房间列表请求
        foodIllustrationButton.onClick.AddListener(() =>
        {
            ShowPanel(5);
            AudioManager.Instance.Play("UIClick");
        });
        quitButton.onClick.AddListener(QuitGame); //本地退出
        // 设置
        myColorR.onValueChanged.AddListener(ColorShow);
        myColorG.onValueChanged.AddListener(ColorShow);
        myColorB.onValueChanged.AddListener(ColorShow);
        backButton.onClick.AddListener(() =>
        {
            ShowPanel(0);
            AudioManager.Instance.Play("UIClick");
        });

        // 创建房间按钮 - 只发送请求
        createButton.onClick.AddListener(() =>
        {
            SendCreateRoomRequest();
            AudioManager.Instance.Play("UIClick");
        }); //发出创建列表请求
        cancelCreateButton.onClick.AddListener(() =>
        {
            ShowPanel(0);
            AudioManager.Instance.Play("UIClick");
        }); //本地切回主菜单

        // 房间列表按钮 - 只发送请求
        refreshRoomListButton.onClick.AddListener(() =>
        {
            SendRoomListRequest();
            AudioManager.Instance.Play("UIClick");
        }); //发出房间列表请求
        backToMainButton.onClick.AddListener(() =>
        {
            ShowPanel(0);
            AudioManager.Instance.Play("UIClick");
        }); //本地切回主页面

        // 房间内按钮 - 只发送请求
        leaveRoomButton.onClick.AddListener(() =>
        {
            SendLeaveRoomRequest();
            AudioManager.Instance.Play("UIClick");
        }); //发送离开请求
        startGameButton.onClick.AddListener(() =>
        {
            SendStartGameRequest();
            AudioManager.Instance.Play("UIClick");
        }); //发送游戏开始

        foodBackToMainButton.onClick.AddListener(() =>
        {
            ShowPanel(0);
            AudioManager.Instance.Play("UIClick");
        });
    }

    void ColorShow(float colorValue)
    {
        colorShow.color = new Color(myColorR.value, myColorG.value, myColorB.value);
    }


    void ShowPanel(int index)
    {
        if (currentPanelIndex == index) return;
        if (currentPanelIndex == -1)
        {
            panels[index].SetActive(true);
            panels[index].transform.rotation = Quaternion.Euler(0, 90, 0);
            panels[index].transform.DORotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.InOutQuad);
            currentPanelIndex = index;
        }
        else
        {
            panels[currentPanelIndex].transform.DORotate(new Vector3(0, 90, 0), 0.4f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    panels[currentPanelIndex].SetActive(false);
                    panels[index].SetActive(true);
                    panels[index].transform.rotation = Quaternion.Euler(0, 90, 0);
                    panels[index].transform.DORotate(new Vector3(0, 0, 0), 0.4f).SetEase(Ease.InOutQuad);
                    currentPanelIndex = index;
                });
        }
   
 
    }

    public void OnGameStartRoom()
    {
        
        panels[currentPanelIndex].transform.DORotate(new Vector3(0, 90, 0), 0.5f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                panels[currentPanelIndex].SetActive(false);

                currentPanelIndex = -1;
            });
    }


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

        NetworkManager.Instance.CreateRoom(roomName, maxPlayers, playerName, colorR, colorG, colorB, playerRole);
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
            int selectedLevel = levelDropdown.value; // 获取选中的关卡索引
            NetworkManager.Instance.GameStartRequest(NetworkManager.Instance.currentRoom.RoomId, selectedLevel);
        }
    }

    //发送加入房间请求
    void SendJoinRoom(string roomId)
    {
        var (playerName, colorR, colorG, colorB) = GetPlayerInfo();
        NetworkManager.Instance.JoinRoom(roomId, playerName, colorR, colorG, colorB, playerRole);
    }

    #endregion

    // 获取玩家信息的辅助方法
    (string playerName, int colorR, int colorG, int colorB ) GetPlayerInfo()
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


    void CreateRoomUIItem(RoomInfo room)
    {
        var roomUIItem = Instantiate(roomUIItemPrefab, roomListContent).GetComponent<RoomUIItem>();
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
            roomUIItem.joinButton.GetComponentInChildren<TextMeshProUGUI>().text = "人已满";
        }
        else
        {
            roomUIItem.joinButton.onClick.AddListener(() =>
            {
                SendJoinRoom(room.RoomId);
                AudioManager.Instance.Play("UIClick");
            });
        }

        //设置房间位置
        RectTransform rectTransform = roomUIItem.transform.GetComponent<RectTransform>();
    }


    void UpdatePlayerList()
    {
        // 清空现有玩家列表
        foreach (var item in playerUIItems)
        {
            Destroy(item.gameObject);
        }

        playerUIItems.Clear();

        foreach (var playerInfo in NetworkManager.Instance.currentRoom.PlayerInfos)
        {
            PlayerUIItem playerUIItem = Instantiate(playerUIItemPrefab, playerListContent).GetComponent<PlayerUIItem>();
            playerUIItems.Add(playerUIItem);

            // 设置基本信息
            playerUIItem.playerNameText.text = playerInfo.PlayerName;
            SetPlayerStatus(playerUIItem, playerInfo.PlayerId);
            SetupKickButton(playerUIItem, playerInfo.PlayerId);

            // 设置位置
            RectTransform rectTransform = playerUIItem.transform.GetComponent<RectTransform>();
            // 设置玩家颜色
            Color playerColor = new Color(
                playerInfo.ColorR / 255f,
                playerInfo.ColorG / 255f,
                playerInfo.ColorB / 255f
            );

            playerUIItem.playerImage.sprite = playerRoleSpriteDict[(PlayerRole)playerInfo.PlayerRole];
            playerUIItem.playerImage.color = playerColor;
        }
    }


    void SetPlayerStatus(PlayerUIItem playerUIItem, string playerId)
    {
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
            playerUIItem.kickButton.onClick.AddListener(() =>
            {
                NetworkManager.Instance.KickPlayerRequest(NetworkManager.Instance.currentRoom.RoomId,
                    targetPlayerId);
                AudioManager.Instance.Play("UIClick");
            });
        }
    }

    #region Event

    // 事件处理
    public void OnRoomListReceived(List<RoomInfo> rooms)
    {
        ShowPanel(3);

        // 清空UI，等待OnRoomListReceived刷新
        foreach (var item in roomItems)
        {
            Destroy(item.gameObject);
        }

        roomItems.Clear();
        foreach (var room in rooms)
        {
            if (room.Status == "waiting")
            {
                CreateRoomUIItem(room);
            }
        }
    }


    public void OnRoomInfoUpdate(RoomInfo roomInfo)
    {
        // 如果房间信息有错误状态，显示错误
        if (roomInfo.Status == "error")
        {
            Debug.LogError($"Room error: {roomInfo.RoomName}");
            // 错误处理：恢复按钮状态或显示错误信息
            return;
        }

        ShowPanel(4);
        if (NetworkManager.Instance.isHost)
        {
            startGameButton.gameObject.SetActive(true);
            // 房主可以看到关卡选择
            levelDropdown.gameObject.SetActive(true);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);

            // 非房主看不到关卡选择
            levelDropdown.gameObject.SetActive(false);
        }

        // 更新房间信息显示
        roomNameText.text = roomInfo.RoomName;
        playerCountText.text = $"{roomInfo.PlayerIds.Count}/{roomInfo.MaxPlayers}";

        // 使用HostName，如果没有则使用截断的HostId
        hostText.text = $"Host: {roomInfo.HostName}";

        // 更新玩家列表
        UpdatePlayerList();
    }

    #endregion


    void QuitGame()
    {
        Application.Quit();
    }
}