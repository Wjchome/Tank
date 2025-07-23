
    using System;
    using System.Collections;
    using UnityEngine;

    public class Special:MonoBehaviour
    {
        SpriteRenderer spriteRenderer;
        
 
        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            StartCoroutine(ChangeColor());
        }

        IEnumerator ChangeColor()
        {
            while(true)
            {
                spriteRenderer.material.color = Color.yellow;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.material.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.material.color = Color.blue;
                yield return new WaitForSeconds(0.1f);
                
            }
        }
    }
