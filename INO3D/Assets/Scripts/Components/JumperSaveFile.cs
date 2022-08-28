using System;
using Assets.Scripts.Components.Base;

namespace Assets.Scripts.Components
{
    [Serializable]
    public class JumperSaveFile : SaveFile
    {
        public float Port1PositionX;
        public float Port1PositionY;
        public float Port1PositionZ;

        public float Port2PositionX;
        public float Port2PositionY;
        public float Port2PositionZ;
    }
}