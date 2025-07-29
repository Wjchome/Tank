
using System;
using DG.Tweening;
using UnityEngine;
    using UnityEngine.UI;

    public class GameUIManager:SingletonMono<GameUIManager>
    {
        
        public Transform playerPanelParent;
        public GameObject playerPanelPrefab;
        public Vector2 firstPos=new Vector2(700, 400); 
        public Vector2 secondPos=new Vector2(0, 400);
        
        
        public Button gameOverButton;

        public GameObject foodPanel;
        public Button backButton;
        
        public Sprite[] backSprites;
        public Vector2 firstPos1=new Vector2(-1000, 0); 
        public Vector2 secondPos1=new Vector2(-600, 0);
        
        private void Awake()
        {
            gameOverButton.onClick.AddListener(()=>Gameover());
            gameOverButton.gameObject.SetActive(false);
            backButton.onClick.AddListener(GoLeft);
        }

        void GoLeft()
        {
            foodPanel.GetComponent<RectTransform>().DOAnchorPos(firstPos1, 0.5f);
            backButton.GetComponent<Image>().sprite = backSprites[0];
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoRight);
        }

        void GoRight()
        {
            foodPanel.GetComponent<RectTransform>().DOAnchorPos(secondPos1, 0.5f);
            backButton.GetComponent<Image>().sprite = backSprites[1];
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GoLeft);
        }
        
        
        void Gameover()
        {
            playerPanelParent.GetComponent<RectTransform>().DOAnchorPos(firstPos,0.5f).SetEase(Ease.OutQuad);
            NetworkManager.Instance.GameOverRequest(NetworkManager.Instance.currentRoom.RoomId);
            gameOverButton.gameObject.SetActive(false);
            DOVirtual.DelayedCall(0.5f, () =>
            {
                ResetGame();

            });
        }


        private void ResetGame()
        {
            for (int i = playerPanelParent.childCount - 1; i >= 0; i--)
            {
                Destroy(playerPanelParent.GetChild(i).gameObject);
                
            }

            LevelManager.Instance.levelNameText.text = "";
            
        }
    }
