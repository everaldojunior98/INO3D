namespace Assets.Scripts.Components.Base
{
    public abstract class SaveFile
    {
        public string Name;
        public string Hash;
        public string ParentHash;
        public string ParentName;
        public string PrefabName;

        public float PositionX;
        public float PositionY;
        public float PositionZ;

        public float RotationX;
        public float RotationY;
        public float RotationZ;
    }
}