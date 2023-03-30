using System.Linq;
using Assets.Scripts.Components;
using Assets.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Windows
{
    public class ConsoleWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerClickHandler
    {
        #region Fields

        public TMP_Text Header;
        public TMP_Text Log;
        
        public TMP_InputField TextInputField;

        public Button SendButton;
        public Button ClearButton;
        public Button CloseButton;

        public Toggle AutoScroll;
        public TMP_Text AutoScrollText;

        public ScrollRect ContentScrollRect;

        private Vector2 lastMousePosition;
        private bool hasLogChanged;
        private string log;

        #endregion

        #region Unity Methods

        private void Start()
        {
            Header.text = LocalizationManager.Instance.Localize("Console");
            CloseButton.onClick.AddListener(() =>
            {
                Destroy(gameObject);
            });

            SendButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.Localize("Send");
            SendButton.onClick.AddListener(() =>
            {
                var command = TextInputField.text;
                if (SimulationManager.Instance.IsSimulating() && command.Length > 0)
                {
                    foreach (var arduinoUno in FindObjectsOfType<ArduinoUno>())
                        arduinoUno.WriteSerial(command);
                }
                TextInputField.text = string.Empty;
            });

            ClearButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.Localize("Clear");
            ClearButton.onClick.AddListener(() =>
            {
                log = string.Empty;
                hasLogChanged = true;
            });

            AutoScrollText.text = LocalizationManager.Instance.Localize("Auto-scroll");
        }

        private void Update()
        {
            if (hasLogChanged)
            {
                Log.text = log;

                if (AutoScroll.isOn)
                    ContentScrollRect.normalizedPosition = Vector2.zero;
                hasLogChanged = false;
            }
        }

        #endregion

        #region Public Methods

        public void AddLog(string newLog)
        {
            log += newLog;
            hasLogChanged = true;
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