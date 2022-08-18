using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class LimitFrameRate : MonoBehaviour
    {
        private int currentRefreshRate;

        private void Start()
        {
            QualitySettings.vSyncCount = 0;
        }

        private void Update()
        {
            if (Screen.currentResolution.refreshRate != currentRefreshRate)
            {
                currentRefreshRate = Screen.currentResolution.refreshRate;
                Application.targetFrameRate = currentRefreshRate;
            }
        }
    }
}