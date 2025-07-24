using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;


    public class Test:MonoBehaviour
    {
   
        void  Start()
        {
            DOVirtual.DelayedCall(10, ()=>
            {
                Debug.Log(1);
            });
            
            Destroy(gameObject);
        }
    }
