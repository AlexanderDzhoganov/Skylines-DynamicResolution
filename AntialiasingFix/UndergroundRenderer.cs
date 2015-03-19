using ColossalFramework;
using ColossalFramework.Steamworks;
using UnityEngine;

namespace DynamicResolution
{
    public class UndergroundRenderer : MonoBehaviour
    {

        private Camera undergroundCamera;
        private Camera mainCamera;
        private Material overlayMaterial;
        private Material metroOverlayMaterial;
        private OverlayEffect overlayEffect;

        private int ID_UndergroundTexture;
        private int ID_UndergroundUVScale;

        public RenderTexture rt;

        void Awake()
        {
            var cameraController = FindObjectOfType<CameraController>();
            mainCamera = cameraController.GetComponent<Camera>();

            var undergroundView = FindObjectOfType<UndergroundView>();
            undergroundCamera = undergroundView.GetComponent<Camera>();

            overlayEffect = FindObjectOfType<OverlayEffect>();

            overlayMaterial = new Material(undergroundShader);
            metroOverlayMaterial = new Material(overlayEffect.m_overlayShader);
            ID_UndergroundTexture = Shader.PropertyToID("_UndergroundTexture");
            ID_UndergroundUVScale = Shader.PropertyToID("_UndergroundUVScale");
        }

        void OnPostRender()
        {
            if (undergroundCamera.cullingMask != 0)
            {
                undergroundCamera.fieldOfView = mainCamera.fieldOfView;
                undergroundCamera.nearClipPlane = mainCamera.nearClipPlane;
                undergroundCamera.farClipPlane = mainCamera.farClipPlane;
                undergroundCamera.rect = new Rect(0, 0, 1, 1);
                undergroundCamera.targetTexture = rt;
                undergroundCamera.enabled = true;

                if (rt != null && Application.isPlaying && Singleton<LoadingManager>.instance.m_loadingComplete)
                {
                    RenderManager.Managers_UndergroundOverlay(Singleton<RenderManager>.instance.CurrentCameraInfo);
                }
            }
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (undergroundCamera.cullingMask == 8192)
            {
                overlayMaterial.SetTexture(ID_UndergroundTexture, rt);
                overlayMaterial.SetVector(this.ID_UndergroundUVScale, Vector4.one);

                Graphics.Blit(src, dst, overlayMaterial);
            }
            else if (undergroundCamera.cullingMask == 16384)
            {
                metroOverlayMaterial.SetTexture(ID_UndergroundTexture, rt);
                metroOverlayMaterial.SetVector(this.ID_UndergroundUVScale, Vector4.one);
                metroOverlayMaterial.EnableKeyword("UNDERGROUND_ON");
                Graphics.Blit(src, dst, metroOverlayMaterial);
            }
            else
            {
                Graphics.Blit(src, dst);
            }
        }

        private static readonly string undergroundShader = @"// Compiled shader for all platforms, uncompressed size: 13.9KB

// Skipping shader variants that would not be included into build of current scene.

Shader ""UndergroundShader"" {
Properties {
 _MainTex (""Base (RGB)"", 2D) = ""white"" { }
 _UndergroundTexture (""Underground (RGB)"", 2D) = ""white"" { }
 _UndergroundUVScale (""Underground UV scale"", Vector) = (0,0,0,0)
}
SubShader { 
 LOD 100
 Tags { ""RenderType""=""Opaque"" }


 // Stats for Vertex shader:
 //       d3d11 : 6 math
 //    d3d11_9x : 6 math
 //        d3d9 : 6 math
 //        gles : 5 math, 2 texture
 //       gles3 : 5 math, 2 texture
 //       metal : 5 math
 //      opengl : 5 math, 2 texture
 // Stats for Fragment shader:
 //       d3d11 : 3 math, 2 texture
 //    d3d11_9x : 3 math, 2 texture
 //        d3d9 : 5 math, 2 texture
 //       metal : 5 math, 2 texture
 Pass {
  Tags { ""RenderType""=""Opaque"" }
  ZTest Always
  ZWrite Off
  Cull Off
  Fog { Mode Off }
  GpuProgramID 39357
Program ""vp"" {
SubProgram ""opengl "" {
// Stats: 5 math, 2 textures
""!!GLSL
#ifdef VERTEX

uniform vec4 _MainTex_ST;
uniform vec4 _UndergroundTexture_ST;
varying vec2 xlv_TEXCOORD0;
varying vec2 xlv_TEXCOORD1;
void main ()
{
  gl_Position = (gl_ModelViewProjectionMatrix * gl_Vertex);
  xlv_TEXCOORD0 = ((gl_MultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  xlv_TEXCOORD1 = ((gl_MultiTexCoord0.xy * _UndergroundTexture_ST.xy) + _UndergroundTexture_ST.zw);
}


#endif
#ifdef FRAGMENT
uniform sampler2D _MainTex;
uniform sampler2D _UndergroundTexture;
uniform vec2 _UndergroundUVScale;
varying vec2 xlv_TEXCOORD0;
void main ()
{
  vec4 tmpvar_1;
  tmpvar_1 = texture2D (_UndergroundTexture, (xlv_TEXCOORD0 * _UndergroundUVScale));
  vec4 tmpvar_2;
  tmpvar_2.w = 1.0;
  tmpvar_2.xyz = (texture2D (_MainTex, xlv_TEXCOORD0).xyz + (tmpvar_1.xyz * (1.0 - tmpvar_1.w)));
  gl_FragData[0] = tmpvar_2;
}


#endif
""
}
SubProgram ""d3d9 "" {
// Stats: 6 math
Bind ""vertex"" Vertex
Bind ""texcoord"" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 4 [_MainTex_ST]
Vector 5 [_UndergroundTexture_ST]
""vs_2_0
dcl_position v0
dcl_texcoord v1
dp4 oPos.x, c0, v0
dp4 oPos.y, c1, v0
dp4 oPos.z, c2, v0
dp4 oPos.w, c3, v0
mad oT0.xy, v1, c4, c4.zwzw
mad oT1.xy, v1, c5, c5.zwzw

""
}
SubProgram ""d3d11 "" {
// Stats: 6 math
Bind ""vertex"" Vertex
Bind ""texcoord"" TexCoord0
ConstBuffer ""$Globals"" 144
Vector 96 [_MainTex_ST]
Vector 112 [_UndergroundTexture_ST]
ConstBuffer ""UnityPerDraw"" 336
Matrix 0 [glstate_matrix_mvp]
BindCB  ""$Globals"" 0
BindCB  ""UnityPerDraw"" 1
""vs_4_0
eefiecedgdmkjeimfhojckdooppgfidadgmpnpncabaaaaaafmacaaaaadaaaaaa
cmaaaaaaiaaaaaaapaaaaaaaejfdeheoemaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaaebaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafaepfdejfeejepeoaafeeffiedepepfceeaaklkl
epfdeheogiaaaaaaadaaaaaaaiaaaaaafaaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaafmaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
fmaaaaaaabaaaaaaaaaaaaaaadaaaaaaabaaaaaaamadaaaafdfgfpfaepfdejfe
ejepeoaafeeffiedepepfceeaaklklklfdeieefcgeabaaaaeaaaabaafjaaaaaa
fjaaaaaeegiocaaaaaaaaaaaaiaaaaaafjaaaaaeegiocaaaabaaaaaaaeaaaaaa
fpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaabaaaaaaghaaaaaepccabaaa
aaaaaaaaabaaaaaagfaaaaaddccabaaaabaaaaaagfaaaaadmccabaaaabaaaaaa
giaaaaacabaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaa
abaaaaaaabaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaaaaaaaaa
agbabaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaa
abaaaaaaacaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaa
aaaaaaaaegiocaaaabaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaa
dcaaaaaldccabaaaabaaaaaaegbabaaaabaaaaaaegiacaaaaaaaaaaaagaaaaaa
ogikcaaaaaaaaaaaagaaaaaadcaaaaalmccabaaaabaaaaaaagbebaaaabaaaaaa
agiecaaaaaaaaaaaahaaaaaakgiocaaaaaaaaaaaahaaaaaadoaaaaab""
}
SubProgram ""gles "" {
// Stats: 5 math, 2 textures
""!!GLES


#ifdef VERTEX

attribute vec4 _glesVertex;
attribute vec4 _glesMultiTexCoord0;
uniform highp mat4 glstate_matrix_mvp;
uniform highp vec4 _MainTex_ST;
uniform highp vec4 _UndergroundTexture_ST;
varying mediump vec2 xlv_TEXCOORD0;
varying mediump vec2 xlv_TEXCOORD1;
void main ()
{
  mediump vec2 tmpvar_1;
  mediump vec2 tmpvar_2;
  highp vec2 tmpvar_3;
  tmpvar_3 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1 = tmpvar_3;
  highp vec2 tmpvar_4;
  tmpvar_4 = ((_glesMultiTexCoord0.xy * _UndergroundTexture_ST.xy) + _UndergroundTexture_ST.zw);
  tmpvar_2 = tmpvar_4;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = tmpvar_1;
  xlv_TEXCOORD1 = tmpvar_2;
}



#endif
#ifdef FRAGMENT

uniform sampler2D _MainTex;
uniform sampler2D _UndergroundTexture;
uniform highp vec2 _UndergroundUVScale;
varying mediump vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 tmpvar_1;
  highp vec2 P_2;
  P_2 = (xlv_TEXCOORD0 * _UndergroundUVScale);
  tmpvar_1 = texture2D (_UndergroundTexture, P_2);
  lowp vec4 tmpvar_3;
  tmpvar_3.w = 1.0;
  tmpvar_3.xyz = (texture2D (_MainTex, xlv_TEXCOORD0).xyz + (tmpvar_1.xyz * (1.0 - tmpvar_1.w)));
  gl_FragData[0] = tmpvar_3;
}



#endif""
}
SubProgram ""d3d11_9x "" {
// Stats: 6 math
Bind ""vertex"" Vertex
Bind ""texcoord"" TexCoord0
ConstBuffer ""$Globals"" 144
Vector 96 [_MainTex_ST]
Vector 112 [_UndergroundTexture_ST]
ConstBuffer ""UnityPerDraw"" 336
Matrix 0 [glstate_matrix_mvp]
BindCB  ""$Globals"" 0
BindCB  ""UnityPerDraw"" 1
""vs_4_0_level_9_1
eefiecedgfkephfolgpiljaphegkhlacmanpfbojabaaaaaafmadaaaaaeaaaaaa
daaaaaaacmabaaaajiacaaaaomacaaaaebgpgodjpeaaaaaapeaaaaaaaaacpopp
leaaaaaaeaaaaaaaacaaceaaaaaadmaaaaaadmaaaaaaceaaabaadmaaaaaaagaa
acaaabaaaaaaaaaaabaaaaaaaeaaadaaaaaaaaaaaaaaaaaaaaacpoppbpaaaaac
afaaaaiaaaaaapjabpaaaaacafaaabiaabaaapjaaeaaaaaeaaaaadoaabaaoeja
abaaoekaabaaookaaeaaaaaeaaaaamoaabaabejaacaabekaacaalekaafaaaaad
aaaaapiaaaaaffjaaeaaoekaaeaaaaaeaaaaapiaadaaoekaaaaaaajaaaaaoeia
aeaaaaaeaaaaapiaafaaoekaaaaakkjaaaaaoeiaaeaaaaaeaaaaapiaagaaoeka
aaaappjaaaaaoeiaaeaaaaaeaaaaadmaaaaappiaaaaaoekaaaaaoeiaabaaaaac
aaaaammaaaaaoeiappppaaaafdeieefcgeabaaaaeaaaabaafjaaaaaafjaaaaae
egiocaaaaaaaaaaaaiaaaaaafjaaaaaeegiocaaaabaaaaaaaeaaaaaafpaaaaad
pcbabaaaaaaaaaaafpaaaaaddcbabaaaabaaaaaaghaaaaaepccabaaaaaaaaaaa
abaaaaaagfaaaaaddccabaaaabaaaaaagfaaaaadmccabaaaabaaaaaagiaaaaac
abaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaabaaaaaa
abaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaaaaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaa
acaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaa
egiocaaaabaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaal
dccabaaaabaaaaaaegbabaaaabaaaaaaegiacaaaaaaaaaaaagaaaaaaogikcaaa
aaaaaaaaagaaaaaadcaaaaalmccabaaaabaaaaaaagbebaaaabaaaaaaagiecaaa
aaaaaaaaahaaaaaakgiocaaaaaaaaaaaahaaaaaadoaaaaabejfdeheoemaaaaaa
acaaaaaaaiaaaaaadiaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaa
ebaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadadaaaafaepfdejfeejepeo
aafeeffiedepepfceeaaklklepfdeheogiaaaaaaadaaaaaaaiaaaaaafaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaafmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadamaaaafmaaaaaaabaaaaaaaaaaaaaaadaaaaaaabaaaaaa
amadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklkl""
}
SubProgram ""gles3 "" {
// Stats: 5 math, 2 textures
""!!GLES3#version 300 es


#ifdef VERTEX


in vec4 _glesVertex;
in vec4 _glesMultiTexCoord0;
uniform highp mat4 glstate_matrix_mvp;
uniform highp vec4 _MainTex_ST;
uniform highp vec4 _UndergroundTexture_ST;
out mediump vec2 xlv_TEXCOORD0;
out mediump vec2 xlv_TEXCOORD1;
void main ()
{
  mediump vec2 tmpvar_1;
  mediump vec2 tmpvar_2;
  highp vec2 tmpvar_3;
  tmpvar_3 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1 = tmpvar_3;
  highp vec2 tmpvar_4;
  tmpvar_4 = ((_glesMultiTexCoord0.xy * _UndergroundTexture_ST.xy) + _UndergroundTexture_ST.zw);
  tmpvar_2 = tmpvar_4;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = tmpvar_1;
  xlv_TEXCOORD1 = tmpvar_2;
}



#endif
#ifdef FRAGMENT


layout(location=0) out mediump vec4 _glesFragData[4];
uniform sampler2D _MainTex;
uniform sampler2D _UndergroundTexture;
uniform highp vec2 _UndergroundUVScale;
in mediump vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 tmpvar_1;
  highp vec2 P_2;
  P_2 = (xlv_TEXCOORD0 * _UndergroundUVScale);
  tmpvar_1 = texture (_UndergroundTexture, P_2);
  lowp vec4 tmpvar_3;
  tmpvar_3.w = 1.0;
  tmpvar_3.xyz = (texture (_MainTex, xlv_TEXCOORD0).xyz + (tmpvar_1.xyz * (1.0 - tmpvar_1.w)));
  _glesFragData[0] = tmpvar_3;
}



#endif""
}
SubProgram ""metal "" {
// Stats: 5 math
Bind ""vertex"" ATTR0
Bind ""texcoord"" ATTR1
ConstBuffer ""$Globals"" 96
Matrix 0 [glstate_matrix_mvp]
Vector 64 [_MainTex_ST]
Vector 80 [_UndergroundTexture_ST]
""metal_vs
#include <metal_stdlib>
using namespace metal;
struct xlatMtlShaderInput {
  float4 _glesVertex [[attribute(0)]];
  float4 _glesMultiTexCoord0 [[attribute(1)]];
};
struct xlatMtlShaderOutput {
  float4 gl_Position [[position]];
  half2 xlv_TEXCOORD0;
  half2 xlv_TEXCOORD1;
};
struct xlatMtlShaderUniform {
  float4x4 glstate_matrix_mvp;
  float4 _MainTex_ST;
  float4 _UndergroundTexture_ST;
};
vertex xlatMtlShaderOutput xlatMtlMain (xlatMtlShaderInput _mtl_i [[stage_in]], constant xlatMtlShaderUniform& _mtl_u [[buffer(0)]])
{
  xlatMtlShaderOutput _mtl_o;
  half2 tmpvar_1;
  half2 tmpvar_2;
  float2 tmpvar_3;
  tmpvar_3 = ((_mtl_i._glesMultiTexCoord0.xy * _mtl_u._MainTex_ST.xy) + _mtl_u._MainTex_ST.zw);
  tmpvar_1 = half2(tmpvar_3);
  float2 tmpvar_4;
  tmpvar_4 = ((_mtl_i._glesMultiTexCoord0.xy * _mtl_u._UndergroundTexture_ST.xy) + _mtl_u._UndergroundTexture_ST.zw);
  tmpvar_2 = half2(tmpvar_4);
  _mtl_o.gl_Position = (_mtl_u.glstate_matrix_mvp * _mtl_i._glesVertex);
  _mtl_o.xlv_TEXCOORD0 = tmpvar_1;
  _mtl_o.xlv_TEXCOORD1 = tmpvar_2;
  return _mtl_o;
}

""
}
}
Program ""fp"" {
SubProgram ""opengl "" {
""!!GLSL""
}
SubProgram ""d3d9 "" {
// Stats: 5 math, 2 textures
Vector 0 [_UndergroundUVScale]
SetTexture 0 [_MainTex] 2D 0
SetTexture 1 [_UndergroundTexture] 2D 1
""ps_2_0
def c1, 1, 0, 0, 0
dcl t0.xy
dcl_2d s0
dcl_2d s1
mul r0.xy, t0, c0
texld_pp r1, t0, s0
texld_pp r0, r0, s1
add_pp r0.w, -r0.w, c1.x
mad_pp r0.xyz, r0, r0.w, r1
mov_pp r0.w, c1.x
mov_pp oC0, r0

""
}
SubProgram ""d3d11 "" {
// Stats: 3 math, 2 textures
SetTexture 0 [_MainTex] 2D 0
SetTexture 1 [_UndergroundTexture] 2D 1
ConstBuffer ""$Globals"" 144
Vector 128 [_UndergroundUVScale] 2
BindCB  ""$Globals"" 0
""ps_4_0
eefiecedbbdhfkdkcfjjijpbamifoekcelgefnecabaaaaaaamacaaaaadaaaaaa
cmaaaaaajmaaaaaanaaaaaaaejfdeheogiaaaaaaadaaaaaaaiaaaaaafaaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaafmaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafmaaaaaaabaaaaaaaaaaaaaaadaaaaaaabaaaaaa
amaaaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcdeabaaaaeaaaaaaaenaaaaaa
fjaaaaaeegiocaaaaaaaaaaaajaaaaaafkaaaaadaagabaaaaaaaaaaafkaaaaad
aagabaaaabaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaafibiaaaeaahabaaa
abaaaaaaffffaaaagcbaaaaddcbabaaaabaaaaaagfaaaaadpccabaaaaaaaaaaa
giaaaaacacaaaaaaefaaaaajpcaabaaaaaaaaaaaegbabaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadiaaaaaidcaabaaaabaaaaaaegbabaaaabaaaaaa
egiacaaaaaaaaaaaaiaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaa
eghobaaaabaaaaaaaagabaaaabaaaaaaaaaaaaaiicaabaaaaaaaaaaadkaabaia
ebaaaaaaabaaaaaaabeaaaaaaaaaiadpdcaaaaajhccabaaaaaaaaaaaegacbaaa
abaaaaaapgapbaaaaaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaa
abeaaaaaaaaaiadpdoaaaaab""
}
SubProgram ""gles "" {
""!!GLES""
}
SubProgram ""d3d11_9x "" {
// Stats: 3 math, 2 textures
SetTexture 0 [_MainTex] 2D 0
SetTexture 1 [_UndergroundTexture] 2D 1
ConstBuffer ""$Globals"" 144
Vector 128 [_UndergroundUVScale] 2
BindCB  ""$Globals"" 0
""ps_4_0_level_9_1
eefiecedakfdkaiecbbbehalhmdpngjplmcloknbabaaaaaaaaadaaaaaeaaaaaa
daaaaaaacaabaaaafmacaaaammacaaaaebgpgodjoiaaaaaaoiaaaaaaaaacpppp
laaaaaaadiaaaaaaabaacmaaaaaadiaaaaaadiaaacaaceaaaaaadiaaaaaaaaaa
abababaaaaaaaiaaabaaaaaaaaaaaaaaaaacppppfbaaaaafabaaapkaaaaaiadp
aaaaaaaaaaaaaaaaaaaaaaaabpaaaaacaaaaaaiaaaaaaplabpaaaaacaaaaaaja
aaaiapkabpaaaaacaaaaaajaabaiapkaafaaaaadaaaaadiaaaaaoelaaaaaoeka
ecaaaaadabaacpiaaaaaoelaaaaioekaecaaaaadaaaacpiaaaaaoeiaabaioeka
acaaaaadaaaaciiaaaaappibabaaaakaaeaaaaaeaaaachiaaaaaoeiaaaaappia
abaaoeiaabaaaaacaaaaciiaabaaaakaabaaaaacaaaicpiaaaaaoeiappppaaaa
fdeieefcdeabaaaaeaaaaaaaenaaaaaafjaaaaaeegiocaaaaaaaaaaaajaaaaaa
fkaaaaadaagabaaaaaaaaaaafkaaaaadaagabaaaabaaaaaafibiaaaeaahabaaa
aaaaaaaaffffaaaafibiaaaeaahabaaaabaaaaaaffffaaaagcbaaaaddcbabaaa
abaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacacaaaaaaefaaaaajpcaabaaa
aaaaaaaaegbabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaai
dcaabaaaabaaaaaaegbabaaaabaaaaaaegiacaaaaaaaaaaaaiaaaaaaefaaaaaj
pcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaabaaaaaaaagabaaaabaaaaaa
aaaaaaaiicaabaaaaaaaaaaadkaabaiaebaaaaaaabaaaaaaabeaaaaaaaaaiadp
dcaaaaajhccabaaaaaaaaaaaegacbaaaabaaaaaapgapbaaaaaaaaaaaegacbaaa
aaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaiadpdoaaaaabejfdeheo
giaaaaaaadaaaaaaaiaaaaaafaaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaa
apaaaaaafmaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadadaaaafmaaaaaa
abaaaaaaaaaaaaaaadaaaaaaabaaaaaaamaaaaaafdfgfpfaepfdejfeejepeoaa
feeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklkl
""
}
SubProgram ""gles3 "" {
""!!GLES3""
}
SubProgram ""metal "" {
// Stats: 5 math, 2 textures
SetTexture 0 [_MainTex] 2D 0
SetTexture 1 [_UndergroundTexture] 2D 1
ConstBuffer ""$Globals"" 8
Vector 0 [_UndergroundUVScale] 2
""metal_fs
#include <metal_stdlib>
using namespace metal;
struct xlatMtlShaderInput {
  half2 xlv_TEXCOORD0;
};
struct xlatMtlShaderOutput {
  half4 _glesFragData_0 [[color(0)]];
};
struct xlatMtlShaderUniform {
  float2 _UndergroundUVScale;
};
fragment xlatMtlShaderOutput xlatMtlMain (xlatMtlShaderInput _mtl_i [[stage_in]], constant xlatMtlShaderUniform& _mtl_u [[buffer(0)]]
  ,   texture2d<half> _MainTex [[texture(0)]], sampler _mtlsmp__MainTex [[sampler(0)]]
  ,   texture2d<half> _UndergroundTexture [[texture(1)]], sampler _mtlsmp__UndergroundTexture [[sampler(1)]])
{
  xlatMtlShaderOutput _mtl_o;
  half4 tmpvar_1;
  float2 P_2;
  P_2 = ((float2)_mtl_i.xlv_TEXCOORD0 * _mtl_u._UndergroundUVScale);
  tmpvar_1 = _UndergroundTexture.sample(_mtlsmp__UndergroundTexture, (float2)(P_2));
  half4 tmpvar_3;
  tmpvar_3.w = half(1.0);
  tmpvar_3.xyz = (_MainTex.sample(_mtlsmp__MainTex, (float2)(_mtl_i.xlv_TEXCOORD0)).xyz + (tmpvar_1.xyz * ((half)1.0 - tmpvar_1.w)));
  _mtl_o._glesFragData_0 = tmpvar_3;
  return _mtl_o;
}

""
}
}
 }
}
}";

    }

}
