using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

namespace SamsBackpack.RenderFeatureStash
{
    public class StashPingPongRenderFeature : ScriptableRendererFeature
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public CameraType cameraTypeMask = CameraType.Game | CameraType.SceneView | CameraType.Reflection | CameraType.VR;
        public Material fullScreenMaterial;

        public TextureInfo textureA;
        public TextureInfo textureB;

        public string sourceTexName = "_SourceTex";
        [Min(0)]
        public int blitCount = 2;

        public List<StashInput> inputs = new List<StashInput>();

        private StashPinPongPass pass;

        public override void Create()
        {
            pass = new StashPinPongPass();
            pass.renderPassEvent = renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            pass.name = name;
            pass.sourceTexName = sourceTexName;
            pass.blitCount = blitCount;
            pass.fullScreenMaterial = fullScreenMaterial;
            pass.textureA = textureA;
            pass.textureB = textureB;
            pass.inputs = inputs;

            if (fullScreenMaterial == null || textureA == null || textureB == null || blitCount <= 0
                 || !cameraTypeMask.HasFlag(renderingData.cameraData.cameraType))
                return;

            renderer.EnqueuePass(pass);
        }


        class StashPinPongPass : ScriptableRenderPass
        {
            public string name;
            public string sourceTexName;
            public int blitCount;
            public Material fullScreenMaterial;
            public TextureInfo textureA;
            public TextureInfo textureB;
            public List<StashInput> inputs = new List<StashInput>();

            private class PassData
            {
                public string sourceTexName;
                public TextureHandle source;
                public int blitIndex;
                public Material fullScreenMaterial;
                public (string, TextureHandle)[] inputs;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                TextureStash textureStash = frameData.GetOrCreate<TextureStash>();
                UniversalResourceData ressourceData = frameData.Get<UniversalResourceData>();

                for (int k = 0; k < blitCount; k++)
                {
                    using (var builder = renderGraph.AddRasterRenderPass<PassData>(name + " " + k, out var passData))
                    {
                        int j = k;

                        builder.AllowGlobalStateModification(true);

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
                        passData.sourceTexName = sourceTexName;
                        passData.fullScreenMaterial = fullScreenMaterial;
                        passData.blitIndex = j;

                        //Source
                        passData.source = textureStash.GetTexture((j % 2 == 0) ? textureA : textureB, renderGraph, ressourceData);
                        builder.UseTexture(passData.source, AccessFlags.Read);


                        //Set output
                        TextureHandle targetHandle = textureStash.GetTexture((j % 2 == 0) ? textureB : textureA, renderGraph, ressourceData);
                        builder.SetRenderAttachment(targetHandle, 0);
                        builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                    }
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

                context.cmd.SetGlobalTexture(data.sourceTexName, data.source);
                context.cmd.SetGlobalFloat("_BlitIndex", data.blitIndex);
                context.cmd.SetGlobalFloat("_Ping", data.blitIndex % 2);

                //Blit
                Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.fullScreenMaterial, 0);
            }
        }
    }
}