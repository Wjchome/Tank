using System;
using System.Collections;
using UnityEngine;

namespace DefaultNamespace
{
    public class Test1:MonoBehaviour
    {
        public Transform content;
        public GameObject abc;
        public float interval;
        private void Start()
        {
            StartCoroutine(a());
        }

        IEnumerator a()
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                Instantiate(abc, content);
                
            }
        }
    }
}