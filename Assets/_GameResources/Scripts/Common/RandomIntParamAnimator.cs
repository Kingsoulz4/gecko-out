using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class RandomIntParamAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string paramName;
        [SerializeField] private int minVal = 0;
        [SerializeField] private int maxVal = 3;
        [SerializeField] private float minDelay = 2f;
        [SerializeField] private float maxDelay = 5f;

        private void Start()
        {
            if (animator == null) animator = GetComponent<Animator>();
            StartCoroutine(PlayRandomAnimationLoop());
        }

        private IEnumerator PlayRandomAnimationLoop()
        {
            while (true)
            {
                // Wait random seconds
                float delay = Random.Range(minDelay, maxDelay);
                yield return new WaitForSeconds(delay);

                // Pick random animation
                var val = Random.Range(minVal, maxVal + 1);

                // Play it
                animator.SetInteger(paramName, val);
                yield return null;
                if (animator.GetCurrentAnimatorClipInfo(0).Length > 0)
                {
                    yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
                }
                animator.SetInteger(paramName, 0);

            }
        }
    }
}
