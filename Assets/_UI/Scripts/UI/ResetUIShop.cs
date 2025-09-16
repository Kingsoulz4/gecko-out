using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetUIShop : MonoBehaviour
{
    [SerializeField] Transform transItemCheck;

    private void FixedUpdate()
    {
        if(transItemCheck != null)
        {
            if(transItemCheck.childCount <= 0 || !transItemCheck.gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
