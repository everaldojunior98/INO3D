using RuntimeNodeEditor;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.NodeEditor.Scripts
{
    public class SerialWriteNode : Node
    {
        #region Fields

        [SerializeField]
        private SocketInput inputSocket;

        private IOutput inputIncomingOutput;

        #endregion

        #region Overrides

        public override void Setup()
        {
            IsTerminal = true;
            Register(inputSocket);
            Register(PreviousNodeSocket);
            Register(NextNodeSocket);

            SetHeader("SerialWrite");
            
            OnConnectionEvent += OnConnection;
            OnDisconnectEvent += OnDisconnect;

            OnConnectedValueUpdated();
        }

        public override void OnSerialize(Serializer serializer)
        {
        }

        public override void OnDeserialize(Serializer serializer)
        {
        }

        public override void OnConnectedValueUpdated()
        {
            var nextCode = string.Empty;
            if (NextNodeSocket.connection != null)
                nextCode = NextNodeSocket.connection.input.OwnerNode.Value;

            var value = inputIncomingOutput?.GetValue<string>() ?? "\"\"";
            Value = $"serialWrite({value});{nextCode}";

            if (PreviousNodeSocket.Connections.Count > 0)
                PreviousNodeSocket.Connections.First().output.OwnerNode.OnConnectedValueUpdated();
        }

        #endregion

        #region Private Methods

        private void OnConnection(SocketInput input, IOutput output)
        {
            output.ValueUpdated += OnConnectedValueUpdated;
            if (input == inputSocket)
                inputIncomingOutput = output;
            OnConnectedValueUpdated();
        }

        private void OnDisconnect(SocketInput input, IOutput output)
        {
            output.ValueUpdated -= OnConnectedValueUpdated;
            if (input == inputSocket)
                inputIncomingOutput = null;
            OnConnectedValueUpdated();
        }

        #endregion
    }
}