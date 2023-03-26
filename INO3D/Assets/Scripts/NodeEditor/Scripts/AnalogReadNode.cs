using System.Collections.Generic;
using RuntimeNodeEditor;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.NodeEditor.Scripts
{
    public class AnalogReadNode : Node
    {
        #region Fields

        private enum Ports
        {
            D0,
            D1,
            D2,
            D3,
            D4,
            D5,
            D6,
            D7,
            D8,
            D9,
            D10,
            D11,
            D12,
            D13,
            A0,
            A1,
            A2,
            A3,
            A4,
            A5
        }

        [SerializeField]
        private TMP_Dropdown portField;

        [SerializeField]
        private SocketOutput outputSocket;

        #endregion

        #region Overrides

        public override void Setup()
        {
            Register(outputSocket);

            SetHeader("AnalogRead");

            portField.AddOptions(new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData(Ports.D0.ToString()),
                new TMP_Dropdown.OptionData(Ports.D1.ToString()),
                new TMP_Dropdown.OptionData(Ports.D2.ToString()),
                new TMP_Dropdown.OptionData(Ports.D3.ToString()),
                new TMP_Dropdown.OptionData(Ports.D4.ToString()),
                new TMP_Dropdown.OptionData(Ports.D5.ToString()),
                new TMP_Dropdown.OptionData(Ports.D6.ToString()),
                new TMP_Dropdown.OptionData(Ports.D7.ToString()),
                new TMP_Dropdown.OptionData(Ports.D8.ToString()),
                new TMP_Dropdown.OptionData(Ports.D9.ToString()),
                new TMP_Dropdown.OptionData(Ports.D10.ToString()),
                new TMP_Dropdown.OptionData(Ports.D11.ToString()),
                new TMP_Dropdown.OptionData(Ports.D12.ToString()),
                new TMP_Dropdown.OptionData(Ports.D13.ToString()),
                new TMP_Dropdown.OptionData(Ports.A0.ToString()),
                new TMP_Dropdown.OptionData(Ports.A1.ToString()),
                new TMP_Dropdown.OptionData(Ports.A2.ToString()),
                new TMP_Dropdown.OptionData(Ports.A3.ToString()),
                new TMP_Dropdown.OptionData(Ports.A4.ToString()),
                new TMP_Dropdown.OptionData(Ports.A5.ToString())
            });

            portField.onValueChanged.AddListener(selected =>
            {
                OnConnectedValueUpdated();
            });

            OnConnectedValueUpdated();
        }

        public override void OnSerialize(Serializer serializer)
        {
            serializer.Add("Value", portField.value.ToString());
        }

        public override void OnDeserialize(Serializer serializer)
        {
            var operation = int.Parse(serializer.Get("Value"));
            portField.SetValueWithoutNotify(operation);

            OnConnectedValueUpdated();
        }

        public override void OnConnectedValueUpdated()
        {
            string port;
            switch ((Ports) portField.value)
            {
                default:
                    port = "0";
                    break;
                case Ports.D0:
                    port = "0";
                    break;
                case Ports.D1:
                    port = "1";
                    break;
                case Ports.D2:
                    port = "2";
                    break;
                case Ports.D3:
                    port = "3";
                    break;
                case Ports.D4:
                    port = "4";
                    break;
                case Ports.D5:
                    port = "5";
                    break;
                case Ports.D6:
                    port = "6";
                    break;
                case Ports.D7:
                    port = "7";
                    break;
                case Ports.D8:
                    port = "8";
                    break;
                case Ports.D9:
                    port = "9";
                    break;
                case Ports.D10:
                    port = "10";
                    break;
                case Ports.D11:
                    port = "11";
                    break;
                case Ports.D12:
                    port = "12";
                    break;
                case Ports.D13:
                    port = "13";
                    break;
                case Ports.A0:
                    port = "A0";
                    break;
                case Ports.A1:
                    port = "A1";
                    break;
                case Ports.A2:
                    port = "A2";
                    break;
                case Ports.A3:
                    port = "A3";
                    break;
                case Ports.A4:
                    port = "A4";
                    break;
                case Ports.A5:
                    port = "A5";
                    break;
            }

            var result = $"analogRead({port})";
            outputSocket.SetValue(result);
        }

        #endregion
    }
}