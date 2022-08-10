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

        private bool canDrag;
        private bool isDragging;

        private Vector3 dragStartPosition;

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
                        canDrag = true;
                        isDragging = false;
                        selectedComponent = component;
                    }
                    else
                    {
                        canDrag = false;
                        isDragging = false;
                        selectedComponent = null;
                    }
                }
                else
                {
                    canDrag = false;
                    isDragging = false;
                    selectedComponent = null;
                }
            }

            if (Input.GetKey(selectButton))
            {
                if (canDrag)
                {
                    var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit, float.MaxValue, ~inoLayerMask))
                        if (selectedComponent != null)
                        {
                            if (!isDragging)
                                dragStartPosition = hit.point;

                            selectedComponent.transform.position += hit.point - dragStartPosition;
                            dragStartPosition = hit.point;
                            isDragging = true;
                        }
                }
            }

            if (Input.GetKeyUp(selectButton))
            {
                canDrag = false;
                isDragging = false;
            }
        }

        #endregion
    }
}