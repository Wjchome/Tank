
    using UnityEngine;

    public class CamController:SingletonMono<CamController>
    {
        public void Change(int size)
        {
            transform.position=new Vector3(size,size,-10);
            GetComponent<Camera>().orthographicSize=size+1;
        }
        
    }
