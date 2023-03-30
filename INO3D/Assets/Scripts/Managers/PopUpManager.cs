using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class PopUpManager : MonoBehaviour
    {
        #region Properties

        public static PopUpManager Instance { get; private set; }

        #endregion

        #region Fields

        public GameObject BackgroundGameObject;
        public GameObject PopupGameObject;

        public TMP_Text HeaderText;
        public TMP_Text ContentText;

        public Button YesButton;
        public Button NoButton;
        public Button CancelButton;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
            Hide();
        }

        #endregion

        #region Public Methods

        public void Show(string header, string content, Action onYesAction, Action onNoAction, Action onCancelAction)
        {
            transform.SetAsLastSibling();
            HeaderText.text = header;
            ContentText.text = content;
            ContentText.rectTransform.anchoredPosition = Vector2.zero;

            if (onYesAction != null)
            {
                YesButton.gameObject.SetActive(true);
                YesButton.GetComponentInChildren<TMP_Text>().text = onNoAction == null && onCancelAction == null
                    ? LocalizationManager.Instance.Localize("Ok")
                    : LocalizationManager.Instance.Localize("Yes");
                YesButton.onClick.RemoveAllListeners();
                YesButton.onClick.AddListener(() =>
                {
                    Hide();
                    onYesAction();
                });
            }
            else
            {
                YesButton.gameObject.SetActive(false);
            }

            if (onNoAction != null)
            {
                NoButton.gameObject.SetActive(true);
                NoButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.Localize("No");
                NoButton.onClick.RemoveAllListeners();
                NoButton.onClick.AddListener(() =>
                {
                    Hide();
                    onNoAction();
                });
            }
            else
            {
                NoButton.gameObject.SetActive(false);
            }

            if (onCancelAction != null)
            {
                CancelButton.gameObject.SetActive(true);
                CancelButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.Localize("Cancel");
                CancelButton.onClick.RemoveAllListeners();
                CancelButton.onClick.AddListener(() =>
                {
                    Hide();
                    onCancelAction();
                });
            }
            else
            {
                CancelButton.gameObject.SetActive(false);
            }

            BackgroundGameObject.SetActive(true);
            PopupGameObject.SetActive(true);
        }

        #endregion

        #region Private Methods

        private void Hide()
        {
            BackgroundGameObject.SetActive(false);
            PopupGameObject.SetActive(false);
        }

        #endregion
    }
}