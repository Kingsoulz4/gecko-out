using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Cryptography;
using UnityEngine;

namespace Geckout
{
    public class BodySelected : MonoBehaviour
    {
        [SerializeField] private float moveDistance = 0.5f;
        [SerializeField] private float duration = 0.5f;
        private Quaternion originalRotation = Quaternion.identity;
        private Vector3 tmpPos = new Vector3();


        private void LateUpdate()
        {
            transform.rotation = originalRotation;
            tmpPos = transform.parent.position; 
            tmpPos.z = -1;
            tmpPos.y += 0.5f;
            transform.position = tmpPos;
        }   

        public void DoEffect()
        {
           float startY = transform.position.y;
            transform.DOMoveY(startY + moveDistance, duration).
                SetLoops(-1, DG.Tweening.LoopType.Yoyo).SetEase(DG.Tweening.Ease.InOutSine);
        }
    }
}
