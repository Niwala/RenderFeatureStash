using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

namespace SamsBackpack.RenderFeatureStash
{
    public class StashFullScreenRenderFeature : ScriptableRendererFeature
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public Material fullScreenMaterial;

        public List<StashInput> inputs = new List<StashInput>();
        public TextureInfo target;

        private FullScreenPass pass;

        public override void Create()
        {
            pass = new FullScreenPass();
            pass.renderPassEvent = renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            pass.name = name;
            pass.fullScreenMaterial = fullScreenMaterial;
            pass.target = target;
            pass.inputs = inputs;

            if (fullScreenMaterial == null || target == null)
                return;

            renderer.EnqueuePass(pass);
        }


        class FullScreenPass : ScriptableRenderPass
        {
            public string name;
            public Material fullScreenMaterial;
            public TextureInfo target;
            public List<StashInput> inputs = new List<StashInput>();

            private class PassData
            {
                public TextureHandle dummy;
                public Material fullScreenMaterial;
                public (string, TextureHandle)[] inputs;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                TextureStash textureStash = frameData.GetOrCreate<TextureStash>();

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(name, out var passData))
                {
                    builder.AllowGlobalStateModification(true);

                    UniversalResourceData ressourceData = frameData.Get<UniversalResourceData>();

                    //Set inputs
                    if (passData.inputs == null || passData.inputs.Length != inputs.Count)
                        passData.inputs = new (string, TextureHandle)[inputs.Count];

                    for (int i = 0; i < inputs.Count; i++)
                    {
                        var input = inputs[i];
                        if (string.IsNullOrEmpty(input.propertyName) || input.textureInfo == null)
                            passData.inputs[i] = default;
                        else
                        {
                            TextureHandle handle = textureStash.GetTexture(input.textureInfo, renderGraph, ressourceData);
                            passData.inputs[i] = (inputs[i].propertyName, handle);
                            builder.UseTexture(handle, AccessFlags.Read);
                        }
                    }

                    //Miscs
                    passData.fullScreenMaterial = fullScreenMaterial;
                    passData.dummy = renderGraph.defaultResources.whiteTexture;
                    builder.UseTexture(passData.dummy, AccessFlags.Read);

                    //Set output
                    TextureHandle targetHandle = textureStash.GetTexture(target, renderGraph, ressourceData);
                    builder.SetRenderAttachment(targetHandle, 0);
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                }
            }

            static void ExecutePass(PassData data, RasterGraphContext context)
            {
                //Set inputs
                for (int i = 0; i < data.inputs.Length; i++)
                {
                    if (string.IsNullOrEmpty(data.inputs[i].Item1))
                        continue;
                    data.fullScreenMaterial.SetTexture(data.inputs[i].Item1, data.inputs[i].Item2);
                }

                //Blit
                Blitter.BlitTexture(context.cmd, data.dummy, new Vector4(1, 1, 0, 0), data.fullScreenMaterial, 0);
            }
        }
    }
}