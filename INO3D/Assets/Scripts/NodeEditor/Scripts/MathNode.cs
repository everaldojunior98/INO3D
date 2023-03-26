using System.Collections.Generic;
using RuntimeNodeEditor;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.NodeEditor.Scripts
{
    public class MathNode : Node
    {
        #region Fields

        private enum Operations
        {
            Add,
            Subtract,
            Multiply,
            Divide
        }

        [SerializeField]
        private TMP_Dropdown operationField;

        [SerializeField]
        private SocketInput inputASocket;

        [SerializeField]
        private SocketInput inputBSocket;

        [SerializeField]
        private SocketOutput outputSocket;

        private IOutput inputAIncomingOutput;
        private IOutput inputBIncomingOutput;

        #endregion

        #region Overrides

        public override void Setup()
        {
            Register(inputASocket);
            Register(inputBSocket);
            Register(outputSocket);

            SetHeader("Math");
            outputSocket.SetValue("0");

            operationField.AddOptions(new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData(Operations.Add.ToString()),
                new TMP_Dropdown.OptionData(Operations.Subtract.ToString()),
                new TMP_Dropdown.OptionData(Operations.Multiply.ToString()),
                new TMP_Dropdown.OptionData(Operations.Divide.ToString())
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
            var inputA = inputAIncomingOutput?.GetValue<string>() ?? "0";
            var inputB = inputBIncomingOutput?.GetValue<string>() ?? "0";

            string operation;
            switch ((Operations)operationField.value)
            {
                default:
                    operation = "+";
                    break;
                case Operations.Add:
                    operation = "+";
                    break;
                case Operations.Subtract:
                    operation = "-";
                    break;
                case Operations.Multiply:
                    operation = "*";
                    break;
                case Operations.Divide:
                    operation = "/";
                    break;
            }

            var result = $"{inputA} {operation} {inputB}";
            outputSocket.SetValue(result);
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