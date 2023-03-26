using RuntimeNodeEditor;
using UnityEngine;

namespace Assets.Scripts.NodeEditor.Scripts
{
    public class MillisNode : Node
    {
        #region Fields

        [SerializeField] 
        private SocketOutput outputSocket;

        #endregion

        #region Overrides

        public override void Setup()
        {
            Register(outputSocket);
            SetHeader("Millis");
            outputSocket.SetValue("millis()");
        }

        #endregion
    }
}