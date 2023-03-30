using System;
using System.Linq;
using Assets.Scripts.Components.Base;
using Assets.Scripts.Managers;
using Assets.Scripts.NodeEditor.Scripts;
using RuntimeNodeEditor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Windows
{
    public class NodeEditorWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerClickHandler
    {
        #region Fields

        public Action<string, string, string> OnSave;

        public InoComponent Component;
        public string InitialCode;

        public TMP_Text Header;
        public TMP_InputField ArduinoNameInputField;

        public RectTransform NodeRectTransform;
        public Sprite BackgroundSprite;

        public Button CloseButton;
        public Button SaveButton;

        private CodeNodeEditor codeNodeEditor;

        private Vector2 lastMousePosition;

        #endregion

        #region Unity Methods

        private void Start()
        {
            Header.text = LocalizationManager.Instance.Localize("NodeEditor");
            CloseButton.onClick.AddListener(() =>
            {
                Destroy(gameObject);
            });

            SaveButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.Instance.Localize("Save");
            SaveButton.onClick.AddListener(() =>
            {
                OnSave(codeNodeEditor.SaveGraph(), codeNodeEditor.GenerateSetupCode(), codeNodeEditor.GenerateLoopCode());
            });

            codeNodeEditor = GetComponent<CodeNodeEditor>();
            var graph = codeNodeEditor.CreateGraph<NodeGraph>(NodeRectTransform);
            codeNodeEditor.StartEditor(graph);
            codeNodeEditor.LoadGraph(InitialCode);

            var graphRect = NodeRectTransform.GetChild(0);
            graphRect.transform.localScale = Vector3.one;

            var defaultColor = ArduinoNameInputField.GetComponent<Image>().color;

            var graphRectImage = graphRect.GetComponent<Image>();
            graphRectImage.color = defaultColor;
            graphRectImage.sprite = BackgroundSprite;
            graphRectImage.type = Image.Type.Sliced;
            graphRectImage.pixelsPerUnitMultiplier = 5f;

            var graphBackground = graphRect.GetChild(0).GetComponent<Image>();
            graphBackground.color = defaultColor;
            graphBackground.sprite = BackgroundSprite;
            graphBackground.type = Image.Type.Sliced;
            graphBackground.pixelsPerUnitMultiplier = 5f;

        }

        private void Update()
        {
            if (Component != null)
                ArduinoNameInputField.text = Component.Name + ".ino";
            else
                Destroy(gameObject);
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