using System;
using Assets.Scripts.Components.Base;

namespace Assets.Scripts.Components
{
    [Serializable]
    public class RcCarChassisSaveFile : SaveFile
    {
        public float PositionX;
        public float PositionY;
        public float PositionZ;

        public float RotationX;
        public float RotationY;
        public float RotationZ;
    }
}