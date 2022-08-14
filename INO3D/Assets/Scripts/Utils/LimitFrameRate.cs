using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class LimitFrameRate : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
        }
    }
}