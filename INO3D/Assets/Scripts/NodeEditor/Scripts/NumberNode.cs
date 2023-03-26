using System.Globalization;
using RuntimeNodeEditor;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.NodeEditor.Scripts
{
    public class NumberNode : Node
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
            SetHeader("Number");

            valueField.text = "0";
            HandleFieldValue(valueField.text);

            valueField.contentType = TMP_InputField.ContentType.DecimalNumber;
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
            var floatValue = float.Parse(value);
            outputSocket.SetValue(floatValue.ToString(CultureInfo.InvariantCulture));
        }

        #endregion
    }
}