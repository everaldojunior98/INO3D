using System.Collections.Generic;
using System.Linq;
using RuntimeNodeEditor;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.NodeEditor.Scripts
{
    public class ConditionalNode : Node
    {
        #region Fields

        private enum Operations
        {
            Equals,
            NotEqual,
            Greater,
            Less,
            GreaterEqual,
            LessEqual
        }

        [SerializeField]
        private TMP_Dropdown operationField;

        [SerializeField]
        private SocketInput inputASocket;

        [SerializeField]
        private SocketInput inputBSocket;

        [SerializeField]
        private SocketOutput trueNextNodeSocket;

        [SerializeField]
        private SocketOutput falseNextNodeSocket;

        private IOutput inputAIncomingOutput;
        private IOutput inputBIncomingOutput;

        #endregion

        #region Overrides

        public override void Setup()
        {
            IsTerminal = true;
            Register(inputASocket);
            Register(inputBSocket);
            Register(PreviousNodeSocket);
            Register(trueNextNodeSocket);
            Register(falseNextNodeSocket);

            SetHeader("Conditional");

            operationField.AddOptions(new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("="),
                new TMP_Dropdown.OptionData("!="),
                new TMP_Dropdown.OptionData(">"),
                new TMP_Dropdown.OptionData("<"),
                new TMP_Dropdown.OptionData(">="),
                new TMP_Dropdown.OptionData("<=")
            });

            operationField.onValueChanged.AddListener(selected =>
            {
                OnConnectedValueUpdated();
            });

            OnConnectionEvent += OnConnection;
            OnDisconnectEvent += OnDisconnect;

            OnConnectedValueUpdated();
        }

        public override void OnSerialize(Serializer serializer)
        {
            serializer.Add("Value", operationField.value.ToString());
        }

        public override void OnDeserialize(Serializer serializer)
        {
            var operation = int.Parse(serializer.Get("Value"));
            operationField.SetValueWithoutNotify(operation);

            OnConnectedValueUpdated();
        }

        public override void OnConnectedValueUpdated()
        {
            var inputA = inputAIncomingOutput?.GetValue<string>() ?? "null";
            var inputB = inputBIncomingOutput?.GetValue<string>() ?? "null";

            string operation;
            switch ((Operations)operationField.value)
            {
                default:
                    operation = "==";
                    break;
                case Operations.Equals:
                    operation = "==";
                    break;
                case Operations.NotEqual:
                    operation = "!=";
                    break;
                case Operations.Greater:
                    operation = ">";
                    break;
                case Operations.Less:
                    operation = "<";
                    break;
                case Operations.GreaterEqual:
                    operation = ">=";
                    break;
                case Operations.LessEqual:
                    operation = "<=";
                    break;
            }

            var trueCode = string.Empty;
            if (trueNextNodeSocket.connection != null)
                trueCode = trueNextNodeSocket.connection.input.OwnerNode.Value;

            var falseCode = string.Empty;
            if (falseNextNodeSocket.connection != null)
                falseCode = falseNextNodeSocket.connection.input.OwnerNode.Value;

            var result = "if (" + inputA + " " + operation + " " + inputB + "){" + trueCode + "}else{" + falseCode + "}";
            Value = result;

            if (PreviousNodeSocket.Connections.Count > 0)
                PreviousNodeSocket.Connections.First().output.OwnerNode.OnConnectedValueUpdated();
        }

        #endregion

        #region Private Methods

        private void OnConnection(SocketInput input, IOutput output)
        {
            output.ValueUpdated += OnConnectedValueUpdated;
            if (input == inputASocket)
                inputAIncomingOutput = output;
            else if (input == inputBSocket)
                inputBIncomingOutput = output;
            OnConnectedValueUpdated();
        }

        private void OnDisconnect(SocketInput input, IOutput output)
        {
            output.ValueUpdated -= OnConnectedValueUpdated;
            if (input == inputASocket)
                inputAIncomingOutput = null;
            else if (input == inputBSocket)
                inputBIncomingOutput = null;
            OnConnectedValueUpdated();
        }

        #endregion
    }
}