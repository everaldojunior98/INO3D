using System;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Windows
{
    public class SettingsWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerClickHandler
    {
        #region Fields

        public TMP_Text Header;

        public Transform LanguageTransform;
        public Transform SensitivityTransform;
        public Transform ShowWarningsTransform;

        public Button CloseButton;
        public Button SaveButton;

        private Vector2 lastMousePosition;

        #endregion

        #region Unity Methods

        private void Start()
        {
            Header.text = LocalizationManager.Instance.Localize("Settings");
            CloseButton.onClick.AddListener(() =>
            {
                Destroy(gameObject);
            });

            var selectedLanguage = LocalizationManager.Instance.GetCurrentLanguage();
            var cameraSensitivity = LocalizationManager.Instance.GetCameraSensitivity();
            var showWarnings = LocalizationManager.Instance.GetShowWarnings();

            var languages = LocalizationManager.Instance.GetLanguages();
            LanguageTransform.GetChild(0).GetComponent<TMP_Text>().text = LocalizationManager.Instance.Localize("Language");
            var languageComboBox = LanguageTransform.transform.GetChild(1).GetComponent<TMP_Dropdown>();
            languageComboBox.AddOptions(languages.Select(s => new TMP_Dropdown.OptionData(s)).ToList());
            languageComboBox.value = selectedLanguage;

            SensitivityTransform.GetChild(0).GetComponent<TMP_Text>().text = LocalizationManager.Instance.Localize("CameraSensitivity");
            var sensitivityInputText = SensitivityTransform.transform.GetChild(1).GetComponent<TMP_InputField>();
            sensitivityInputText.contentType = TMP_InputField.ContentType.DecimalNumber;
            sensitivityInputText.text = cameraSensitivity.ToString(CultureInfo.InvariantCulture);

            ShowWarningsTransform.GetChild(0).GetComponent<TMP_Text>().text = LocalizationManager.Instance.Localize("ShowWarnings");
            var showWarningsToggle = ShowWarningsTransform.GetChild(1).GetComponent<Toggle>();
            showWarningsToggle.isOn = showWarnings;

            SaveButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.Localize("Save");
            SaveButton.onClick.AddListener(() =>
            {
                try
                {
                    if (languageComboBox.value != selectedLanguage)
                    {
                        PopUpManager.Instance.Show(LocalizationManager.Instance.Localize("Warning"),
                            LocalizationManager.Instance.Localize("LanguagePopupMessage"),
                            () =>
                            {
                                LocalizationManager.Instance.SaveLanguage(languages[languageComboBox.value]);
                                LocalizationManager.Instance.SaveCameraSensitivity(float.Parse(sensitivityInputText.text));
                                LocalizationManager.Instance.SaveShowWarnings(showWarningsToggle.isOn);
                                Destroy(gameObject);
                            },
                            null, null);
                    }
                    else
                    {
                        LocalizationManager.Instance.SaveLanguage(languages[languageComboBox.value]);
                        LocalizationManager.Instance.SaveCameraSensitivity(float.Parse(sensitivityInputText.text));
                        LocalizationManager.Instance.SaveShowWarnings(showWarningsToggle.isOn);
                        Destroy(gameObject);
                    }
                }
                catch
                {
                }
            });
        }

        #endregion

        #region Drag Methods

        public void OnPointerClick(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
            lastMousePosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var currentMousePosition = eventData.position;
            var diff = currentMousePosition - lastMousePosition;
            var rect = GetComponent<RectTransform>();

            var newPosition = rect.position + new Vector3(diff.x, diff.y, transform.position.z);
            var oldPos = rect.position;
            rect.position = newPosition;
            if (!IsRectTransformInsideScreen(rect))
                rect.position = oldPos;
            lastMousePosition = currentMousePosition;
        }

        private bool IsRectTransformInsideScreen(RectTransform rectTransform)
        {
            var isInside = false;
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            var rect = new Rect(0, 0, Screen.width, Screen.height);
            var visibleCorners = corners.Count(corner => rect.Contains(corner));
            if (visibleCorners == 4)
                isInside = true;
            return isInside;
        }

        #endregion
    }
}