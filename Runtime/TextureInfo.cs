using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace SamsBackpack.RenderFeatureStash
{
    [CreateAssetMenu(menuName = "Rendering/Texture Info")]
    public class TextureInfo : ScriptableObject
    {
        public Source source;
        public bool enableRandomWrite;
        public bool clear;
        public Color clearColor;
        public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
        public FilterMode filterMode = FilterMode.Bilinear;
        public bool customGraphicFormat;
        public GraphicsFormat graphicFormat = GraphicsFormat.R32G32B32A32_SFloat;
        public StashSizeMode sizeMode;
        public float screenRatio = 0.5f;
        public Vector2Int fixedSize = new Vector2Int(512, 512);
    }

    public enum Source
    {
        Custom,
        CameraColor,
        CameraDepth,
        CameraNormal,
        CameraColorCopy,
        CameraDepthCopy,
        SSAO,
        AfterPostProcessColor,
    }

    public enum StashSizeMode
    {
        Screen,
        ScreenRatio,
        FixedSize
    }
}