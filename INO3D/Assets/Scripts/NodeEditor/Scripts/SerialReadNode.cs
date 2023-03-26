using RuntimeNodeEditor;
using UnityEngine;

namespace Assets.Scripts.NodeEditor.Scripts
{
    public class SerialReadNode : Node
    {
        #region Fields

        [SerializeField] 
        private SocketOutput outputSocket;

        #endregion

        #region Overrides

        public override void Setup()
        {
            Register(outputSocket);
            SetHeader("SerialRead");
            outputSocket.SetValue("serialRead()");
        }

        #endregion
    }
}