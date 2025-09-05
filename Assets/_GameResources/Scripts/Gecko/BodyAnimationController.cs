using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Geckout
{
    public class BodyAnimationController : MonoBehaviour
    {
        [SerializeField] private BodyController m_bodyController;

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
            }

        }


    }
}
