using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Geckout
{
    public class BodyOptionsChanger : MonoBehaviour
    {
        [SerializeField] private List<GameObject> m_listOptions;

        private int currentIndex = 0;

        private void Start()
        {
            currentIndex = Random.Range(0, m_listOptions.Count);
            foreach (var item in m_listOptions)
            {
                item.SetActive(false);
            }

            m_listOptions[currentIndex].SetActive(true);
        }

        void Update()
        {
            if(Input.GetMouseButtonUp(0))
            {
                currentIndex++;
                var screenPoint = Input.mousePosition;
                var ray = RectTransformUtility.ScreenPointToRay(Camera.main, screenPoint);
                if (Physics.Raycast(ray, out var hitInfo, 1000))
                {
                    if (hitInfo.transform.TryGetComponent<Segment>(out var segment))
                    {
                        if(segment.segmentType != SegmentType.HEAD)
                        {
                            return;
                        }

                        if (currentIndex >= m_listOptions.Count)
                        {
                            currentIndex = 0;
                        }

                        foreach (var item in m_listOptions)
                        {
                            item.SetActive(false);
                        }

                        m_listOptions[currentIndex].SetActive(true);
                        //segment.
                    }
                }
                
            }
        }
    }
}
