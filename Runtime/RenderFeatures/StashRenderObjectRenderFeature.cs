using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RendererUtils;

namespace SamsBackpack.RenderFeatureStash
{
    public class StashRenderObjectRenderFeature : ScriptableRendererFeature
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

        [Header("Target")]
        public TextureInfo target;

        [Header("Filter")]
        public LayerMask layerMask;


        [Header("Overrides")]
        public bool overrideMaterial;
        public Material material;
        public int materialPassIndex;

        [Space]
        public bool overrideShader;
        public Shader shader;
        public int shaderPassIndex;

        private StashRenderObjectPass pass;

        public override void Create()
        {
            pass = new StashRenderObjectPass();
            pass.renderPassEvent = renderPassEvent;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            //Texture
            pass.target = target;

            //Filter
            pass.layerMask = layerMask;

            //Overrides
            pass.overrideMaterial = overrideMaterial ? material : null;
            pass.overrideMaterialPassIndex = materialPassIndex;

            pass.overrideShader = overrideShader ? shader : null;
            pass.overrideShaderPassIndex = shaderPassIndex;


            if (target == null)
                return;

            pass.name = name;
            renderer.EnqueuePass(pass);
        }

        public enum Target
        {
            CameraColorDepth,
            CustomTexture
        }

        class StashRenderObjectPass : ScriptableRenderPass
        {
            public string name;

            //Texture
            public TextureInfo target;

            //Filter
            public LayerMask layerMask;

            //Overrides
            public Material overrideMaterial;
            public int overrideMaterialPassIndex;
            public Shader overrideShader;
            public int overrideShaderPassIndex;

            private class PassData
            {
                public RendererListHandle renderList;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                using (var builder = renderGraph.AddRasterRenderPass<PassData>(name, out var passData))
                {
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
                    TextureStash stash = frameData.GetOrCreate<TextureStash>();

                    ShaderTagId[] shaderTags = new ShaderTagId[]
                    {
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId("UniversalForwardOnly"),
                    new ShaderTagId("SRPDefaultUnlit")
                    };

                    FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);

                    RendererListDesc renderListDesc = new RendererListDesc(shaderTags, universalRenderingData.cullResults, cameraData.camera)
                    {
                        sortingCriteria = SortingCriteria.CommonOpaque,
                        layerMask = layerMask,
                        overrideMaterial = overrideMaterial,
                        overrideMaterialPassIndex = overrideMaterialPassIndex,
                        overrideShader = overrideShader,
                        overrideShaderPassIndex = overrideShaderPassIndex,
                        renderQueueRange = RenderQueueRange.opaque,
                    };


                    passData.renderList = renderGraph.CreateRendererList(renderListDesc);
                    builder.UseRendererList(passData.renderList);

                    TextureHandle dstTexture = stash.GetTexture(target, renderGraph, resourceData);
                    builder.SetRenderAttachment(dstTexture, 0);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                }
            }

            static void ExecutePass(PassData data, RasterGraphContext context)
            {
                context.cmd.DrawRendererList(data.renderList);
            }
        }
    }
}