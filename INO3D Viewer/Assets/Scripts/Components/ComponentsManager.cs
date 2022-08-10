using Assets.Scripts.Components.Base;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class ComponentsManager : MonoBehaviour
    {
        #region Fields

        [SerializeField] LayerMask inoLayerMask;
        [SerializeField] KeyCode selectButton;

        private InoComponent selectedComponent;
        private Camera mainCamera;

        private bool isDragging;

        #endregion

        #region Unity Methods

        private void Start()
        {
            mainCamera = CameraController.Instance.GetMainCamera();
        }

        private void Update()
        {
            if (Input.GetKeyDown(selectButton))
            {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, inoLayerMask))
                {
                    var component = hit.transform.GetComponent<InoComponent>();
                    if (component != null)
                    {
                        isDragging = true;
                        selectedComponent = component;
                    }
                    else
                    {
                        isDragging = false;
                        selectedComponent = null;
                    }
                }
                else
                {
                    isDragging = false;
                    selectedComponent = null;
                }
            }

            if (Input.GetKey(selectButton))
            {
                if (isDragging)
                {
                    var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, float.MaxValue, ~inoLayerMask))
                        if (selectedComponent != null)
                            selectedComponent.transform.position = hit.point;
                }
            }

            if (Input.GetKeyUp(selectButton))
                isDragging = false;
        }

        #endregion
    }
}