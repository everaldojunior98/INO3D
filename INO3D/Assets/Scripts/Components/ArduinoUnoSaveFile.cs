using System;
using Assets.Scripts.Components.Base;

namespace Assets.Scripts.Components
{
    [Serializable]
    public class ArduinoUnoSaveFile : SaveFile
    {
        public bool UseGraph;
        public string Code;
        public string Graph;
        public string GraphCode;
    }
}