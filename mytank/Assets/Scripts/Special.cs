
    using System;
    using System.Collections;
    using UnityEngine;

    public class Special:MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        
 
        private void Start()
        {
            StartCoroutine(ChangeColor());
        }

        IEnumerator ChangeColor()
        {
            while(true)
            {
                spriteRenderer.material.color = Color.yellow;
                yield return new WaitForSeconds(0.15f);
                spriteRenderer.material.color = Color.red;
                yield return new WaitForSeconds(0.15f);
                spriteRenderer.material.color = Color.blue;
                yield return new WaitForSeconds(0.15f);
                
            }
        }
    }
