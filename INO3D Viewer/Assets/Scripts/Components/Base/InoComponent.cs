using UnityEngine;

namespace Assets.Scripts.Components.Base
{
    public class InoComponent : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                SetTargetPosition();
            }
        }

        void SetTargetPosition()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                transform.position = hit.point;
            }
        }
    }
}
