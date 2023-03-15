using UnityEngine;
using static Assets.Scripts.Components.Base.InoComponent;

namespace Assets.Scripts.Components.Base
{
    public class Port
    {
        #region Properties

        public readonly string PortName;
        public readonly Vector3 PortPosition;
        public readonly PortType PortType;
        public readonly PinType PinType;
        public readonly bool CanBeRigid;
        public readonly bool IsTerminalBlock;
        public readonly Vector3 WireDirection;
        public readonly Vector3 WirePositionOffset;
        public readonly Vector3 WireRotationOffset;

        #endregion

        #region Constructor

        public Port(string portName, Vector3 portPosition, PortType portType, PinType pinType, bool canBeRigid, bool isTerminalBlock, Vector3 wireDirection, Vector3 wirePositionOffset, Vector3 wireRotationOffset)
        {
            PortName = portName;
            PortPosition = portPosition;
            PortType = portType;
            PinType = pinType;
            CanBeRigid = canBeRigid;
            IsTerminalBlock = isTerminalBlock;
            WireDirection = wireDirection;
            WirePositionOffset = wirePositionOffset;
            WireRotationOffset = wireRotationOffset;
        }

        #endregion

    }
}