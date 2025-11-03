using System.Collections.Generic;

using UnityEngine;

namespace SamsBackpack.RenderFeatureStash
{
    [System.Serializable]
    public struct StashInput
    {
        public string propertyName;
        public TextureInfo textureInfo;
    }

    [System.Serializable]
    public class StashInputList : List<StashInput>
    {
        
    }
}
