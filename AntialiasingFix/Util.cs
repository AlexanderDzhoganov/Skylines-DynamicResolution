using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace DynamicResolution
{
    public enum RescalingFilter
    {
        Bilinear = 0,
        Lancoz = 1
    }

    public static class DebugUtil
    {

        public static void DumpRenderTexture(RenderTexture rt, string pngOutPath)
        {
            var oldRT = RenderTexture.active;

            var tex = new Texture2D(rt.width, rt.height);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            File.WriteAllBytes(pngOutPath, tex.EncodeToPNG());
            RenderTexture.active = oldRT;
        }

    }

}
