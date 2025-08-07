
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.UI;

    public static class UIChange
    {
       public static void GoLeft(Button thisBtn,RectTransform moveTarget,Vector2 leftPos,Vector2 rightPos)
        {
            moveTarget.DOAnchorPos(leftPos, 0.5f);
            thisBtn.onClick.RemoveAllListeners();
            thisBtn.onClick.AddListener(()=>GoRight(thisBtn,moveTarget,leftPos,rightPos));
        }
        public static void GoRight(Button thisBtn,RectTransform moveTarget,Vector2 leftPos,Vector2 rightPos)
        {
            moveTarget.DOAnchorPos(rightPos, 0.5f);
            thisBtn.onClick.RemoveAllListeners();
            thisBtn.onClick.AddListener(()=>GoLeft(thisBtn,moveTarget,leftPos,rightPos));
        }
    }
