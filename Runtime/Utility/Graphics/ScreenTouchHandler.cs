using UnityEngine;

namespace TapEmpire.UI
{
    public class ScreenTouchHandler : MonoBehaviour
    {
        private void Update()
        {
            if (Input.touchSupported)
            {
                if (Input.touchCount > 0)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}