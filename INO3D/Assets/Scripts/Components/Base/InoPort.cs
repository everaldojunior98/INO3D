using Assets.Scripts.Managers;
using UnityEngine;
using static Assets.Scripts.Components.Base.InoComponent;

namespace Assets.Scripts.Components.Base
{
    public class InoPort : MonoBehaviour
    {
        #region Porperties

        public string PortName;
        public PortType PortType;
        public PinType PinType;

        #endregion

        #region Fields

        private GameObject indicator;
        private BoxCollider boxCollider;
        private MeshRenderer meshRenderer;

        private Material selectedMaterial;
        private Material unselectedMaterial;

        private bool canSelect;
        private bool isSelected;

        private InoComponent connectedComponent;

        #endregion

        #region Unity Methods

        private void Start()
        {
            indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicator.transform.parent = transform;
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = ComponentsManager.Instance.DefaultIndicatorSize;
            Destroy(indicator.GetComponent<BoxCollider>());

            meshRenderer = indicator.GetComponent<MeshRenderer>();

            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = ComponentsManager.Instance.DefaultIndicatorSize;

            selectedMaterial = ComponentsManager.Instance.GetSelectedMaterial();
            unselectedMaterial = ComponentsManager.Instance.GetUnselectedMaterial();

            canSelect = true;

            UpdateMaterial();
            HideIndicator();
        }

        private void OnMouseEnter()
        {
            if (UIManager.Instance.IsMouserOverUI() || !canSelect)
                return;

            ShowIndicator();
            UIManager.Instance.DisplayPortOverlay(PortName, PortType, PinType);
        }

        private void OnMouseDown()
        {
            if (UIManager.Instance.IsMouserOverUI() || !canSelect)
                return;

            isSelected = !isSelected;
            UpdateMaterial();

            if (isSelected)
                ComponentsManager.Instance.OnPortSelected(this);
            else
                ComponentsManager.Instance.OnPortUnselected(this);
        }

        private void OnMouseExit()
        {
            if (!isSelected)
                HideIndicator();
            UIManager.Instance.HidePortOverlay();
        }

        #endregion

        #region Private Methods

        private void UpdateMaterial()
        {
            meshRenderer.sharedMaterial = isSelected ? selectedMaterial : unselectedMaterial;
        }

        private void ShowIndicator()
        {
            indicator.SetActive(true);
        }

        private void HideIndicator()
        {
            indicator.SetActive(false);
        }

        #endregion

        #region Public Methods

        public void Connect(InoComponent component)
        {
            connectedComponent = component;
        }

        public void Disconnect()
        {
            connectedComponent = null;
        }

        public InoComponent GetConnectedComponent()
        {
            return connectedComponent;
        }

        public bool IsConnected()
        {
            return connectedComponent != null;
        }

        public void Disable()
        {
            HideIndicator();
            isSelected = false;
            canSelect = false;
        }

        public void Enable()
        {
            canSelect = true;
            UpdateMaterial();
        }

        #endregion
    }
}