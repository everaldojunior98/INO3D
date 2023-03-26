using RuntimeNodeEditor;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.NodeEditor.Scripts
{
    public class TextNode : Node
    {
        #region Fields

        [SerializeField]
        private TMP_InputField valueField;

        [SerializeField] 
        private SocketOutput outputSocket;

        #endregion

        #region Overrides

        public override void Setup()
        {
            Register(outputSocket);
            SetHeader("Text");

            valueField.text = "NewText";
            HandleFieldValue(valueField.text);

            valueField.contentType = TMP_InputField.ContentType.Standard;
            valueField.onEndEdit.AddListener(HandleFieldValue);
        }

        public override void OnSerialize(Serializer serializer)
        {
            serializer.Add("Value", valueField.text);
        }

        public override void OnDeserialize(Serializer serializer)
        {
            var value = serializer.Get("Value");
            valueField.SetTextWithoutNotify(value);

            HandleFieldValue(value);
        }

        #endregion

        #region Private Methods

        private void HandleFieldValue(string value)
        {
            outputSocket.SetValue("\"" + value + "\"");
        }

        #endregion
    }
}