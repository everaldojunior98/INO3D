using UnityEngine;

namespace Assets.Scripts.Components.Base
{
    public class InoPort : MonoBehaviour
    {
        #region Fields

        private readonly Vector3 defaultSize = 0.04f * Vector3.one;

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
            ShowIndicator();
        }

        private void OnMouseDown()
        {
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