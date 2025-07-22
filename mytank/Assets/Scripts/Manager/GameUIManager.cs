
using System;
using DG.Tweening;
using UnityEngine;
    using UnityEngine.UI;

    public class GameUIManager:SingletonMono<GameUIManager>
    {
        
        public Transform playerPanelParent;
        public GameObject playerPanelPrefab;
        
        public Button gameOverButton;


        public Vector2 firstPos=new Vector2(700, 400);
        public Vector2 secondPos=new Vector2(0, 400);
        private void Awake()
        {
            gameOverButton.onClick.AddListener(()=>Gameover());
            
        }

        void Gameover()
        {
            playerPanelParent.GetComponent<RectTransform>().DOAnchorPos(firstPos,0.5f).SetEase(Ease.OutQuad);
            NetworkManager.Instance.LeaveRoom(NetworkManager.Instance.currentRoom.RoomId);
            gameOverButton.gameObject.SetActive(false);
            DOVirtual.DelayedCall(0.5f, () => 
            {
                for (int i = playerPanelParent.childCount - 1; i >= 0; i--)
                {
                    Destroy(playerPanelParent.GetChild(i).gameObject);
                }

            });
        }
    }
