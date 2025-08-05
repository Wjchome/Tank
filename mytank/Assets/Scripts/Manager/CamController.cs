
    using System;
    using UnityEngine;

    using DG.Tweening;

    public class CamController : SingletonMono<CamController>
    {
        public Vector3 orignalPosition;
    
        // 震动参数
        public float shakeDuration = 0.4f;   // 震动持续时间
        public float shakeStrength = 0.2f;   // 震动强度
        public int shakeVibrato = 10;        // 震动频率（类似振幅）
        public float shakeRandomness = 90f;  // 随机性（0-180）

        public void Change(int size)
        {
            transform.position = new Vector3(size, size, -10);
            orignalPosition = new Vector3(size, size, -10);
            GetComponent<Camera>().orthographicSize = size + 1;
        }

        public void ShakeDead()
        {
            // 确保震动结束后回归原位
            transform.DOShakePosition(
                duration: shakeDuration,
                strength: shakeStrength,
                vibrato: shakeVibrato,
                randomness: shakeRandomness,
                snapping: false,
                fadeOut: true
            ).OnComplete(() => transform.position = orignalPosition);
        }
    }