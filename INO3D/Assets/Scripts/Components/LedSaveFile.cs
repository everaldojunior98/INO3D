using System;
using Assets.Scripts.Components.Base;

namespace Assets.Scripts.Components
{
    [Serializable]
    public class LedSaveFile : SaveFile
    {
        public int CurrentColor;
    }
}