using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    [RequireComponent(typeof(Button))]
    public class CustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        #region Properties

        public Action OnMouseUp;
        public Action OnMouseDown;

        #endregion

        #region Fields

        private Button button;
        private Image image;

        private bool lastInteractable;

        #endregion

        #region Unity Methods

        private void Start()
        {
            button = GetComponent<Button>();
            image = transform.GetChild(0).GetComponent<Image>();
        }

        private void Update()
        {
            if (lastInteractable != button.interactable)
            {
                if (!button.interactable)
                    image.color = button.colors.disabledColor;
                else
                    image.color = button.colors.normalColor;
                lastInteractable = button.interactable;
            }
        }

        #endregion

        #region Mouse Events

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable)
                return;
            OnMouseDown?.Invoke();
            image.color = button.colors.pressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!button.interactable)
                return;
            OnMouseUp?.Invoke();
            image.color = button.colors.normalColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable)
                return;
            image.color = button.colors.highlightedColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!button.interactable)
                return;
            image.color = button.colors.normalColor;
        }

        #endregion
    }
}