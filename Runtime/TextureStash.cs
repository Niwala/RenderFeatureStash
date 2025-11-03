using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace SamsBackpack.RenderFeatureStash
{
    public class TextureStash : ContextItem
    {
        public TextureHandle GetTexture(TextureInfo info, RenderGraph renderGraph, UniversalResourceData resourceData)
        {
            switch (info.source)
            {
                case Source.CameraColor:
                    return resourceData.cameraColor;

                case Source.CameraDepth:
                    return resourceData.cameraDepth;

                case Source.CameraNormal:
                    return resourceData.cameraNormalsTexture;

                case Source.CameraColorCopy:
                    return resourceData.cameraOpaqueTexture;

                case Source.CameraDepthCopy:
                    return resourceData.cameraDepthTexture;

                case Source.SSAO:
                    return resourceData.ssaoTexture;

                case Source.AfterPostProcessColor:
                    return resourceData.afterPostProcessColor;

                case Source.Custom:
                    {
                        if (textures.ContainsKey(info))
                        {
                            return textures[info];
                        }

                        TextureHandle handle = CreateTexture(info, renderGraph, resourceData.cameraColor);
                        textures.Add(info, handle);
                        return handle;

                    }
            }

            return TextureHandle.nullHandle;
        }

        public TextureHandle CreateTexture(TextureInfo info, RenderGraph renderGraph, TextureHandle cameraColor)
        {
            TextureDesc cameraDesc = cameraColor.GetDescriptor(renderGraph);
            return renderGraph.CreateTexture(GetDesc(info, renderGraph, cameraDesc));
        }

        private TextureDesc GetDesc(TextureInfo info, RenderGraph renderGraph, TextureDesc cameraDesc)
        {
            TextureDesc texDesc = cameraDesc;
            texDesc.name = info.name;
            texDesc.slices = 1;
            texDesc.wrapMode = info.wrapMode;
            texDesc.clearBuffer = info.clear;
            texDesc.clearColor = info.clearColor;
            texDesc.dimension = TextureDimension.Tex2D;
            texDesc.filterMode = info.filterMode;
            texDesc.enableRandomWrite = info.enableRandomWrite;

            if (info.customGraphicFormat)
            {
                texDesc.colorFormat = info.graphicFormat;
            }

            if (info.sizeMode == StashSizeMode.ScreenRatio)
            {
                texDesc.width = Mathf.Max(4, Mathf.RoundToInt(cameraDesc.width * info.screenRatio));
                texDesc.height = Mathf.Max(4, Mathf.RoundToInt(cameraDesc.height * info.screenRatio));
            }
            else if (info.sizeMode == StashSizeMode.FixedSize)
            {
                texDesc.width = Mathf.Max(4, info.fixedSize.x);
                texDesc.height = Mathf.Max(4, info.fixedSize.y);
            }

            return texDesc;
        }

        private Dictionary<TextureInfo, TextureHandle> textures = new Dictionary<TextureInfo, TextureHandle>();

        public override void Reset()
        {
            textures.Clear();
        }
    }
}