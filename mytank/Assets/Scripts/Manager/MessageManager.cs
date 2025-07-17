using TMPro;

public class MessageManager:SingletonMono<MessageManager>
{
    public TextMeshProUGUI sendTxt;
    public TextMeshProUGUI receiveTxt;


    public void SetSendTxt(string send)
    {
        sendTxt.text = send;
    }

    public void SetReceiveTxt(string receive)
    {
        receiveTxt.text = receive;
    }
}