using System.Globalization;
using System.Linq;
using RuntimeNodeEditor;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.NodeEditor.Scripts
{
    public class DelayNode : Node
    {
        #region Fields

        [SerializeField]
        private TMP_InputField valueField;

        #endregion

        #region Overrides

        public override void Setup()
        {
            IsTerminal = true;
            Register(PreviousNodeSocket);
            Register(NextNodeSocket);
            SetHeader("Delay");

            valueField.text = "0";
            OnConnectedValueUpdated();

            valueField.contentType = TMP_InputField.ContentType.IntegerNumber;
            valueField.onEndEdit.AddListener(value =>
            {
                OnConnectedValueUpdated();
            });

            OnConnectionEvent += OnConnection;
            OnDisconnectEvent += OnDisconnect;
        }

        public override void OnSerialize(Serializer serializer)
        {
            serializer.Add("Value", valueField.text);
        }

        public override void OnDeserialize(Serializer serializer)
        {
            var value = serializer.Get("Value");
            valueField.SetTextWithoutNotify(value);

            OnConnectedValueUpdated();
        }

        public override void OnConnectedValueUpdated()
        {
            var nextCode = string.Empty;
            if (NextNodeSocket.connection != null)
                nextCode = NextNodeSocket.connection.input.OwnerNode.Value;

            var delayValue = int.Parse(valueField.text);
            Value = $"delay({delayValue.ToString(CultureInfo.InvariantCulture)});{nextCode}";

            if (PreviousNodeSocket.Connections.Count > 0)
                PreviousNodeSocket.Connections.First().output.OwnerNode.OnConnectedValueUpdated();
        }

        #endregion

        #region Private Methods

        private void OnConnection(SocketInput input, IOutput output)
        {
            output.ValueUpdated += OnConnectedValueUpdated;
            OnConnectedValueUpdated();
        }

        private void OnDisconnect(SocketInput input, IOutput output)
        {
            output.ValueUpdated -= OnConnectedValueUpdated;
            OnConnectedValueUpdated();
        }

        #endregion
    }
}