using Assets.Scripts.Utils;
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

        private readonly Vector3 defaultSize = new Vector3(0.04f, 0.04f, 0.04f);

        private GameObject indicator;
        private BoxCollider boxCollider;
        private MeshRenderer meshRenderer;

        private Material selectedMaterial;
        private Material unselectedMaterial;

        private bool isSelected;

        #endregion

        #region Unity Methods

        private void Start()
        {
            indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
            indicator.transform.parent = transform;
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = defaultSize;
            Destroy(indicator.GetComponent<BoxCollider>());

            meshRenderer = indicator.GetComponent<MeshRenderer>();

            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = defaultSize;

            selectedMaterial = ComponentsManager.Instance.GetSelectedMaterial();
            unselectedMaterial = ComponentsManager.Instance.GetUnselectedMaterial();

            UpdateMaterial();
            HideIndicator();
        }

        private void OnMouseEnter()
        {
            if (UIManager.Instance.IsMouserOverUI())
                return;

            ShowIndicator();
            UIManager.Instance.DisplayPortOverlay(PortName, PortType, PinType);
        }

        private void OnMouseDown()
        {
            if (UIManager.Instance.IsMouserOverUI())
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

        #endregion

        #region Public Methods

        public void ShowIndicator()
        {
            indicator.SetActive(true);
        }

        public void HideIndicator()
        {
            indicator.SetActive(false);
        }

        #endregion
    }
}