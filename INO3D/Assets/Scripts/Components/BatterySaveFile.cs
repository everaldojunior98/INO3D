using System;
using Assets.Scripts.Components.Base;

namespace Assets.Scripts.Components
{
    [Serializable]
    public class BatterySaveFile : SaveFile
    {
        public float Voltage;
    }
}