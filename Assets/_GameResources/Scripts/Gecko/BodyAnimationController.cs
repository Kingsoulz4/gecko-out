using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class BodyAnimationController : MonoBehaviour
    {
        [SerializeField] private BodyController m_bodyController;
        [SerializeField] private Animator m_headAnimator;
        [SerializeField] private Animator m_frontLegAnimator;
        [SerializeField] private Animator m_backLegAnimator;
        [SerializeField] private Animator m_tailAnimator;

        private void Update()
        {
            m_headAnimator.SetBool("isMoving", m_bodyController.IsMoving);
            m_tailAnimator.SetBool("isMoving", m_bodyController.IsMoving);
            m_backLegAnimator.SetBool("isMoving", m_bodyController.IsMoving);
            m_frontLegAnimator.SetBool("isMoving", m_bodyController.IsMoving);

        }


    }
}
