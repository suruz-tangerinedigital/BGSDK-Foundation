using System;

namespace HeathenEngineering.BGSDK.DataModel
{
    [Serializable]
    public struct TokenAttributes
    {
        public enum Type
        {
            property,
            stat,
            boost,
        }

        public string name;
        public string type;
        public string value;
        [UnityEngine.Tooltip("This is only applied when the type is \"stat\"")]
        public ulong maxValue;
    }
}
