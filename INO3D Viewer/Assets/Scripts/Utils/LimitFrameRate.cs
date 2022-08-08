using UnityEngine;

public class LimitFrameRate : MonoBehaviour
{
    private void Start()
    {
        Application.targetFrameRate = 30;
    }
}