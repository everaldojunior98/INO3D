using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class LimitFrameRate : MonoBehaviour
    {
        private void Start()
        {
#if !UNITY_EDITOR
        Application.targetFrameRate = 30;
#endif
        }
    }
}