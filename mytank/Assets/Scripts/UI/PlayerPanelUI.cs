
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class PlayerPanelUI:MonoBehaviour
    {
        public TextMeshProUGUI playerNameText;
        public Image playerImage;
        public TextMeshProUGUI playerHealthText;
        public TextMeshProUGUI playerKillText;

        public int interval;
        public void SetPos(int index)
        {
            GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-index*interval);
        }
        
        public void UpdateUI(TankController tankController)
        {
            playerNameText.text=tankController.playerName;
            if (tankController.IsLocalPlayer)
            {
                playerNameText.color = Color.yellow;
            }
            playerImage.color = tankController.playerColor;
            playerHealthText.text="HP: "+tankController.currentData.HP.ToString();
            playerKillText.text="Kill: "+tankController.killNum.ToString();
        }
    }
