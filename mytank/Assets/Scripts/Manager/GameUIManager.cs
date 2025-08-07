using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameUIManager : SingletonMono<GameUIManager>
{
    public GameObject gamePanel;

    public Transform playerPanelParent;
    public GameObject playerPanelPrefab;
    public Vector2 firstPos;
    public Vector2 secondPos;


    public Button gameOverButton;

    public Vector2 firstPos1;
    public Vector2 secondPos1;
    public GameObject foodPanel;
    public Button backButton1;

    public Vector2 firstPos2;
    public Vector2 secondPos2;
    public GameObject playerInfoPanel;
    public Button backButton2;
    
    
   
    public RectTransform canvas;
    public RectTransform moveParent;
    public InputButton upButton;
    public InputButton leftButton;
    public InputButton downButton;
    public InputButton rightButton;
    public RectTransform shootPos;
    public InputButton shootButton;


    private void Awake()
    {
        secondPos1 = foodPanel.GetComponent<RectTransform>().anchoredPosition;
        firstPos1 = secondPos1 - new Vector2(400, 0);
        
        secondPos2 = playerInfoPanel.GetComponent<RectTransform>().anchoredPosition;
        firstPos2 =secondPos2 -new Vector2(1400, 0);

        gameOverButton.onClick.AddListener(Gameover);
        gameOverButton.gameObject.SetActive(false); 
        
        backButton1.onClick.AddListener(()=>GoLeft(backButton1,foodPanel.GetComponent<RectTransform>(),firstPos1,secondPos1));
        
        backButton2.onClick.AddListener(()=>GoLeft(backButton2,playerInfoPanel.GetComponent<RectTransform>(),firstPos2,secondPos2));

    }

    void GoLeft(Button thisBtn,RectTransform moveTarget,Vector2 leftPos,Vector2 rightPos)
    {
        moveTarget.DOAnchorPos(leftPos, 0.5f);
        thisBtn.onClick.RemoveAllListeners();
        thisBtn.onClick.AddListener(()=>GoRight(thisBtn,moveTarget,leftPos,rightPos));
    }

    void GoRight(Button thisBtn,RectTransform moveTarget,Vector2 leftPos,Vector2 rightPos)
    {
        moveTarget.DOAnchorPos(rightPos, 0.5f);
        thisBtn.onClick.RemoveAllListeners();
        thisBtn.onClick.AddListener(()=>GoLeft(thisBtn,moveTarget,leftPos,rightPos));
    }


    void Gameover()
    {
        playerPanelParent.GetComponent<RectTransform>().DOAnchorPos(firstPos, 0.5f).SetEase(Ease.OutQuad);
        NetworkManager.Instance.GameOverRequest(NetworkManager.Instance.currentRoom.RoomId);
        gameOverButton.gameObject.SetActive(false);

        DOVirtual.DelayedCall(0.5f, () => { ResetGame(); });
    }


    private void ResetGame()
    {
        for (int i = playerPanelParent.childCount - 1; i >= 0; i--)
        {
            Destroy(playerPanelParent.GetChild(i).gameObject);
        }

        LevelManager.Instance.levelNameText.text = "";
        gamePanel.SetActive(false);
    }
}