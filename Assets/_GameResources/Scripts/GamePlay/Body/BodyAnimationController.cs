using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public class BodyAnimationController : MonoBehaviour
    {
        [SerializeField] private BodyController m_bodyController;
        [SerializeField] private float animSpeed = 5;

        private List<Animator> listAnimator = new();

        private bool isMoving = false;

        private void Start()
        {
            listAnimator = GetComponentsInChildren<Animator>().ToList();
        }

        private void Update()
        {
            if (m_bodyController.IsMoving != isMoving)
            {
                isMoving = m_bodyController.IsMoving;
                listAnimator.ForEach(x => x.SetBool("isMoving", isMoving));
                listAnimator.ForEach(x => x.speed = isMoving ? animSpeed : 1);
            }

        }


    }
}
