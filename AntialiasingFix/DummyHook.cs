using UnityEngine;

namespace DynamicResolution
{
    public class DummyHook : MonoBehaviour
    {

        public RenderTexture rt;
        public Camera mainCamera;
        public Camera camera;

        private Material downsampleShader;
        public CameraHook hook;

        public void Awake()
        {
            camera = GetComponent<Camera>();
            downsampleShader = new Material(downsampleShaderSource);
        }

        public void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (rt == null)
            {
                return;
            }

            mainCamera.targetTexture = rt;
            mainCamera.Render();
            mainCamera.targetTexture = null;

            float factor = hook.GetSSAAFactor();

            if (factor > 1.0f)
            {
                float psx = 1.0f / (float)rt.width;
                float psy = 1.0f / (float)rt.height;

                downsampleShader.SetFloat("_PixelSizeX", psx);
                downsampleShader.SetFloat("_PixelSizeY", psy);

                Graphics.Blit(rt, dst, downsampleShader);
            }
            else
            {
                Graphics.Blit(rt, dst);
            }
        }

        public void Update()
        {
            camera.fieldOfView = mainCamera.fieldOfView;
            camera.nearClipPlane = mainCamera.nearClipPlane;
            camera.farClipPlane = mainCamera.farClipPlane;
            camera.transform.position = mainCamera.transform.position;
            camera.transform.rotation = mainCamera.transform.rotation;
            camera.rect = mainCamera.rect;
        }


        private readonly string downsampleShaderSource = @"// Compiled shader for all platforms, uncompressed size: 20.8KB

// Skipping shader variants that would not be included into build of current scene.

Shader ""Unlit/Texture"" {
Properties {
 _MainTex (""Base (RGB)"", 2D) = ""white"" { }
 _PixelSizeX (""PixelSizeX"", Float) = 0
 _PixelSizeY (""PixelSizeY"", Float) = 0
}
SubShader { 
 LOD 100
 Tags { ""RenderType""=""Opaque"" }


 // Stats for Vertex shader:
 //       d3d11 : 5 math
 //    d3d11_9x : 5 math
 //        d3d9 : 5 math
 //        gles : 24 math, 9 texture
 //       gles3 : 24 math, 9 texture
 //       metal : 3 math
 //      opengl : 24 math, 9 texture
 // Stats for Fragment shader:
 //       d3d11 : 16 math, 9 texture
 //    d3d11_9x : 16 math, 9 texture
 //        d3d9 : 30 math, 9 texture
 //       metal : 24 math, 9 texture
 Pass {
  Tags { ""RenderType""=""Opaque"" }
  GpuProgramID 37689
Program ""vp"" {
SubProgram ""opengl "" {
// Stats: 24 math, 9 textures
""!!GLSL
#ifdef VERTEX

uniform vec4 _MainTex_ST;
varying vec2 xlv_TEXCOORD0;
void main ()
{
  gl_Position = (gl_ModelViewProjectionMatrix * gl_Vertex);
  xlv_TEXCOORD0 = ((gl_MultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
}


#endif
#ifdef FRAGMENT
uniform sampler2D _MainTex;
uniform float _PixelSizeX;
uniform float _PixelSizeY;
varying vec2 xlv_TEXCOORD0;
void main ()
{
  vec4 col_1;
  vec2 tmpvar_2;
  tmpvar_2.y = 0.0;
  tmpvar_2.x = _PixelSizeX;
  vec2 tmpvar_3;
  tmpvar_3.y = 0.0;
  float cse_4;
  cse_4 = -(_PixelSizeX);
  tmpvar_3.x = cse_4;
  vec2 tmpvar_5;
  tmpvar_5.x = 0.0;
  tmpvar_5.y = _PixelSizeY;
  vec2 tmpvar_6;
  tmpvar_6.x = 0.0;
  float cse_7;
  cse_7 = -(_PixelSizeY);
  tmpvar_6.y = cse_7;
  vec2 tmpvar_8;
  tmpvar_8.x = _PixelSizeX;
  tmpvar_8.y = _PixelSizeY;
  vec2 tmpvar_9;
  tmpvar_9.x = cse_4;
  tmpvar_9.y = cse_7;
  vec2 tmpvar_10;
  tmpvar_10.x = cse_4;
  tmpvar_10.y = _PixelSizeY;
  vec2 tmpvar_11;
  tmpvar_11.x = _PixelSizeX;
  tmpvar_11.y = cse_7;
  col_1.xyz = (((
    ((((
      ((texture2D (_MainTex, xlv_TEXCOORD0) + texture2D (_MainTex, (xlv_TEXCOORD0 + tmpvar_2))) + texture2D (_MainTex, (xlv_TEXCOORD0 + tmpvar_3)))
     + texture2D (_MainTex, 
      (xlv_TEXCOORD0 + tmpvar_5)
    )) + texture2D (_MainTex, (xlv_TEXCOORD0 + tmpvar_6))) + texture2D (_MainTex, (xlv_TEXCOORD0 + tmpvar_8))) + texture2D (_MainTex, (xlv_TEXCOORD0 + tmpvar_9)))
   + texture2D (_MainTex, 
    (xlv_TEXCOORD0 + tmpvar_10)
  )) + texture2D (_MainTex, (xlv_TEXCOORD0 + tmpvar_11))) / 9.0).xyz;
  col_1.w = 1.0;
  gl_FragData[0] = col_1;
}


#endif
""
}
SubProgram ""d3d9 "" {
// Stats: 5 math
Bind ""vertex"" Vertex
Bind ""texcoord"" TexCoord0
Matrix 0 [glstate_matrix_mvp]
Vector 4 [_MainTex_ST]
""vs_2_0
dcl_position v0
dcl_texcoord v1
dp4 oPos.x, c0, v0
dp4 oPos.y, c1, v0
dp4 oPos.z, c2, v0
dp4 oPos.w, c3, v0
mad oT0.xy, v1, c4, c4.zwzw

""
}
SubProgram ""d3d11 "" {
// Stats: 5 math
Bind ""vertex"" Vertex
Bind ""texcoord"" TexCoord0
ConstBuffer ""$Globals"" 128
Vector 112 [_MainTex_ST]
ConstBuffer ""UnityPerDraw"" 336
Matrix 0 [glstate_matrix_mvp]
BindCB  ""$Globals"" 0
BindCB  ""UnityPerDraw"" 1
""vs_4_0
eefiecediclbinofmafhnpjgnjjnoeonbagdabifabaaaaaaamacaaaaadaaaaaa
cmaaaaaaiaaaaaaaniaaaaaaejfdeheoemaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaaebaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafaepfdejfeejepeoaafeeffiedepepfceeaaklkl
epfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
fdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklfdeieefccmabaaaa
eaaaabaaelaaaaaafjaaaaaeegiocaaaaaaaaaaaaiaaaaaafjaaaaaeegiocaaa
abaaaaaaaeaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaabaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaaddccabaaaabaaaaaagiaaaaac
abaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaabaaaaaa
abaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaaaaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaa
acaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaa
egiocaaaabaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaal
dccabaaaabaaaaaaegbabaaaabaaaaaaegiacaaaaaaaaaaaahaaaaaaogikcaaa
aaaaaaaaahaaaaaadoaaaaab""
}
SubProgram ""gles "" {
// Stats: 24 math, 9 textures
""!!GLES


#ifdef VERTEX

attribute vec4 _glesVertex;
attribute vec4 _glesMultiTexCoord0;
uniform highp mat4 glstate_matrix_mvp;
uniform highp vec4 _MainTex_ST;
varying mediump vec2 xlv_TEXCOORD0;
void main ()
{
  mediump vec2 tmpvar_1;
  highp vec2 tmpvar_2;
  tmpvar_2 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1 = tmpvar_2;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = tmpvar_1;
}



#endif
#ifdef FRAGMENT

uniform sampler2D _MainTex;
uniform highp float _PixelSizeX;
uniform highp float _PixelSizeY;
varying mediump vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 col_1;
  highp vec2 tmpvar_2;
  tmpvar_2.y = 0.0;
  tmpvar_2.x = _PixelSizeX;
  highp vec2 P_3;
  P_3 = (xlv_TEXCOORD0 + tmpvar_2);
  highp vec2 tmpvar_4;
  tmpvar_4.y = 0.0;
  highp float cse_5;
  cse_5 = -(_PixelSizeX);
  tmpvar_4.x = cse_5;
  highp vec2 P_6;
  P_6 = (xlv_TEXCOORD0 + tmpvar_4);
  highp vec2 tmpvar_7;
  tmpvar_7.x = 0.0;
  tmpvar_7.y = _PixelSizeY;
  highp vec2 P_8;
  P_8 = (xlv_TEXCOORD0 + tmpvar_7);
  highp vec2 tmpvar_9;
  tmpvar_9.x = 0.0;
  highp float cse_10;
  cse_10 = -(_PixelSizeY);
  tmpvar_9.y = cse_10;
  highp vec2 P_11;
  P_11 = (xlv_TEXCOORD0 + tmpvar_9);
  highp vec2 tmpvar_12;
  tmpvar_12.x = _PixelSizeX;
  tmpvar_12.y = _PixelSizeY;
  highp vec2 P_13;
  P_13 = (xlv_TEXCOORD0 + tmpvar_12);
  highp vec2 tmpvar_14;
  tmpvar_14.x = cse_5;
  tmpvar_14.y = cse_10;
  highp vec2 P_15;
  P_15 = (xlv_TEXCOORD0 + tmpvar_14);
  highp vec2 tmpvar_16;
  tmpvar_16.x = cse_5;
  tmpvar_16.y = _PixelSizeY;
  highp vec2 P_17;
  P_17 = (xlv_TEXCOORD0 + tmpvar_16);
  highp vec2 tmpvar_18;
  tmpvar_18.x = _PixelSizeX;
  tmpvar_18.y = cse_10;
  highp vec2 P_19;
  P_19 = (xlv_TEXCOORD0 + tmpvar_18);
  col_1.xyz = (((
    ((((
      ((texture2D (_MainTex, xlv_TEXCOORD0) + texture2D (_MainTex, P_3)) + texture2D (_MainTex, P_6))
     + texture2D (_MainTex, P_8)) + texture2D (_MainTex, P_11)) + texture2D (_MainTex, P_13)) + texture2D (_MainTex, P_15))
   + texture2D (_MainTex, P_17)) + texture2D (_MainTex, P_19)) / 9.0).xyz;
  col_1.w = 1.0;
  gl_FragData[0] = col_1;
}



#endif""
}
SubProgram ""d3d11_9x "" {
// Stats: 5 math
Bind ""vertex"" Vertex
Bind ""texcoord"" TexCoord0
ConstBuffer ""$Globals"" 128
Vector 112 [_MainTex_ST]
ConstBuffer ""UnityPerDraw"" 336
Matrix 0 [glstate_matrix_mvp]
BindCB  ""$Globals"" 0
BindCB  ""UnityPerDraw"" 1
""vs_4_0_level_9_1
eefiecedklbaenjdmegagpdkcfpdgcpejoanebigabaaaaaapiacaaaaaeaaaaaa
daaaaaaabiabaaaaemacaaaakaacaaaaebgpgodjoaaaaaaaoaaaaaaaaaacpopp
kaaaaaaaeaaaaaaaacaaceaaaaaadmaaaaaadmaaaaaaceaaabaadmaaaaaaahaa
abaaabaaaaaaaaaaabaaaaaaaeaaacaaaaaaaaaaaaaaaaaaaaacpoppbpaaaaac
afaaaaiaaaaaapjabpaaaaacafaaabiaabaaapjaaeaaaaaeaaaaadoaabaaoeja
abaaoekaabaaookaafaaaaadaaaaapiaaaaaffjaadaaoekaaeaaaaaeaaaaapia
acaaoekaaaaaaajaaaaaoeiaaeaaaaaeaaaaapiaaeaaoekaaaaakkjaaaaaoeia
aeaaaaaeaaaaapiaafaaoekaaaaappjaaaaaoeiaaeaaaaaeaaaaadmaaaaappia
aaaaoekaaaaaoeiaabaaaaacaaaaammaaaaaoeiappppaaaafdeieefccmabaaaa
eaaaabaaelaaaaaafjaaaaaeegiocaaaaaaaaaaaaiaaaaaafjaaaaaeegiocaaa
abaaaaaaaeaaaaaafpaaaaadpcbabaaaaaaaaaaafpaaaaaddcbabaaaabaaaaaa
ghaaaaaepccabaaaaaaaaaaaabaaaaaagfaaaaaddccabaaaabaaaaaagiaaaaac
abaaaaaadiaaaaaipcaabaaaaaaaaaaafgbfbaaaaaaaaaaaegiocaaaabaaaaaa
abaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaaaaaaaaaaagbabaaa
aaaaaaaaegaobaaaaaaaaaaadcaaaaakpcaabaaaaaaaaaaaegiocaaaabaaaaaa
acaaaaaakgbkbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaakpccabaaaaaaaaaaa
egiocaaaabaaaaaaadaaaaaapgbpbaaaaaaaaaaaegaobaaaaaaaaaaadcaaaaal
dccabaaaabaaaaaaegbabaaaabaaaaaaegiacaaaaaaaaaaaahaaaaaaogikcaaa
aaaaaaaaahaaaaaadoaaaaabejfdeheoemaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapapaaaaebaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafaepfdejfeejepeoaafeeffiedepepfceeaaklkl
epfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaaadaaaaaa
aaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadamaaaa
fdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklkl""
}
SubProgram ""gles3 "" {
// Stats: 24 math, 9 textures
""!!GLES3#version 300 es


#ifdef VERTEX


in vec4 _glesVertex;
in vec4 _glesMultiTexCoord0;
uniform highp mat4 glstate_matrix_mvp;
uniform highp vec4 _MainTex_ST;
out mediump vec2 xlv_TEXCOORD0;
void main ()
{
  mediump vec2 tmpvar_1;
  highp vec2 tmpvar_2;
  tmpvar_2 = ((_glesMultiTexCoord0.xy * _MainTex_ST.xy) + _MainTex_ST.zw);
  tmpvar_1 = tmpvar_2;
  gl_Position = (glstate_matrix_mvp * _glesVertex);
  xlv_TEXCOORD0 = tmpvar_1;
}



#endif
#ifdef FRAGMENT


layout(location=0) out mediump vec4 _glesFragData[4];
uniform sampler2D _MainTex;
uniform highp float _PixelSizeX;
uniform highp float _PixelSizeY;
in mediump vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 col_1;
  highp vec2 tmpvar_2;
  tmpvar_2.y = 0.0;
  tmpvar_2.x = _PixelSizeX;
  highp vec2 P_3;
  P_3 = (xlv_TEXCOORD0 + tmpvar_2);
  highp vec2 tmpvar_4;
  tmpvar_4.y = 0.0;
  highp float cse_5;
  cse_5 = -(_PixelSizeX);
  tmpvar_4.x = cse_5;
  highp vec2 P_6;
  P_6 = (xlv_TEXCOORD0 + tmpvar_4);
  highp vec2 tmpvar_7;
  tmpvar_7.x = 0.0;
  tmpvar_7.y = _PixelSizeY;
  highp vec2 P_8;
  P_8 = (xlv_TEXCOORD0 + tmpvar_7);
  highp vec2 tmpvar_9;
  tmpvar_9.x = 0.0;
  highp float cse_10;
  cse_10 = -(_PixelSizeY);
  tmpvar_9.y = cse_10;
  highp vec2 P_11;
  P_11 = (xlv_TEXCOORD0 + tmpvar_9);
  highp vec2 tmpvar_12;
  tmpvar_12.x = _PixelSizeX;
  tmpvar_12.y = _PixelSizeY;
  highp vec2 P_13;
  P_13 = (xlv_TEXCOORD0 + tmpvar_12);
  highp vec2 tmpvar_14;
  tmpvar_14.x = cse_5;
  tmpvar_14.y = cse_10;
  highp vec2 P_15;
  P_15 = (xlv_TEXCOORD0 + tmpvar_14);
  highp vec2 tmpvar_16;
  tmpvar_16.x = cse_5;
  tmpvar_16.y = _PixelSizeY;
  highp vec2 P_17;
  P_17 = (xlv_TEXCOORD0 + tmpvar_16);
  highp vec2 tmpvar_18;
  tmpvar_18.x = _PixelSizeX;
  tmpvar_18.y = cse_10;
  highp vec2 P_19;
  P_19 = (xlv_TEXCOORD0 + tmpvar_18);
  col_1.xyz = (((
    ((((
      ((texture (_MainTex, xlv_TEXCOORD0) + texture (_MainTex, P_3)) + texture (_MainTex, P_6))
     + texture (_MainTex, P_8)) + texture (_MainTex, P_11)) + texture (_MainTex, P_13)) + texture (_MainTex, P_15))
   + texture (_MainTex, P_17)) + texture (_MainTex, P_19)) / 9.0).xyz;
  col_1.w = 1.0;
  _glesFragData[0] = col_1;
}



#endif""
}
SubProgram ""metal "" {
// Stats: 3 math
Bind ""vertex"" ATTR0
Bind ""texcoord"" ATTR1
ConstBuffer ""$Globals"" 80
Matrix 0 [glstate_matrix_mvp]
Vector 64 [_MainTex_ST]
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
};
struct xlatMtlShaderUniform {
  float4x4 glstate_matrix_mvp;
  float4 _MainTex_ST;
};
vertex xlatMtlShaderOutput xlatMtlMain (xlatMtlShaderInput _mtl_i [[stage_in]], constant xlatMtlShaderUniform& _mtl_u [[buffer(0)]])
{
  xlatMtlShaderOutput _mtl_o;
  half2 tmpvar_1;
  float2 tmpvar_2;
  tmpvar_2 = ((_mtl_i._glesMultiTexCoord0.xy * _mtl_u._MainTex_ST.xy) + _mtl_u._MainTex_ST.zw);
  tmpvar_1 = half2(tmpvar_2);
  _mtl_o.gl_Position = (_mtl_u.glstate_matrix_mvp * _mtl_i._glesVertex);
  _mtl_o.xlv_TEXCOORD0 = tmpvar_1;
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
// Stats: 30 math, 9 textures
Float 0 [_PixelSizeX]
Float 1 [_PixelSizeY]
SetTexture 0 [_MainTex] 2D 0
""ps_2_0
def c2, 0, 0.111111112, 1, 0
dcl t0.xy
dcl_2d s0
add r0.x, t0.x, c0.x
mov r0.y, t0.y
mov r1.x, -c0.x
mov r1.y, c2.x
add r1.xy, r1, t0
mov r2.x, t0.x
add r2.y, t0.y, c1.x
mov r3.x, t0.x
add r3.y, t0.y, -c1.x
add r4.x, t0.x, c0.x
add r4.y, t0.y, c1.x
mov r5.x, -c0.x
mov r5.y, -c1.x
add r5.xy, r5, t0
mov r6.x, -c0.x
mov r6.y, c1.x
add r6.xy, r6, t0
add r7.x, t0.x, c0.x
add r7.y, t0.y, -c1.x
texld r0, r0, s0
texld_pp r8, t0, s0
texld r1, r1, s0
texld r2, r2, s0
texld r3, r3, s0
texld r4, r4, s0
texld r5, r5, s0
texld r6, r6, s0
texld r7, r7, s0
add_pp r0.xyz, r0, r8
add_pp r0.xyz, r1, r0
add_pp r0.xyz, r2, r0
add_pp r0.xyz, r3, r0
add_pp r0.xyz, r4, r0
add_pp r0.xyz, r5, r0
add_pp r0.xyz, r6, r0
add_pp r0.xyz, r7, r0
mul_pp r0.xyz, r0, c2.y
mov_pp r0.w, c2.z
mov_pp oC0, r0

""
}
SubProgram ""d3d11 "" {
// Stats: 16 math, 9 textures
SetTexture 0 [_MainTex] 2D 0
ConstBuffer ""$Globals"" 128
Float 96 [_PixelSizeX]
Float 100 [_PixelSizeY]
BindCB  ""$Globals"" 0
""ps_4_0
eefiecedkokfonbhlflccbocfnmcgfplmggeehbpabaaaaaaliaeaaaaadaaaaaa
cmaaaaaaieaaaaaaliaaaaaaejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcpiadaaaa
eaaaaaaapoaaaaaafjaaaaaeegiocaaaaaaaaaaaahaaaaaafkaaaaadaagabaaa
aaaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaagcbaaaaddcbabaaaabaaaaaa
gfaaaaadpccabaaaaaaaaaaagiaaaaacaeaaaaaadgaaaaagjcaabaaaaaaaaaaa
agiecaaaaaaaaaaaagaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaahpcaabaaaaaaaaaaaegaobaaaaaaaaaaa
egbebaaaabaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaaefaaaaajpcaabaaaaaaaaaaaogakbaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaa
abaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaaaaaaaaahhcaabaaaabaaaaaa
egacbaaaabaaaaaaegacbaaaacaaaaaadgaaaaahhcaabaaaacaaaaaaegiacaia
ebaaaaaaaaaaaaaaagaaaaaadgaaaaaficaabaaaacaaaaaaabeaaaaaaaaaaaaa
aaaaaaahpcaabaaaacaaaaaaogaebaaaacaaaaaaegbebaaaabaaaaaaefaaaaaj
pcaabaaaadaaaaaaegaabaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaa
efaaaaajpcaabaaaacaaaaaaogakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaadaaaaaa
aaaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaadgaaaaaf
bcaabaaaabaaaaaaabeaaaaaaaaaaaaadgaaaaahccaabaaaabaaaaaabkiacaia
ebaaaaaaaaaaaaaaagaaaaaaaaaaaaahdcaabaaaabaaaaaaegaabaaaabaaaaaa
egbabaaaabaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaaaaaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaaaaaaaaaidcaabaaaabaaaaaaegbabaaaabaaaaaaegiacaaa
aaaaaaaaagaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaaaaaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaaaaaaaaahhcaabaaaaaaaaaaaegacbaaaacaaaaaaegacbaaa
aaaaaaaadcaaaaanpcaabaaaabaaaaaaegiecaaaaaaaaaaaagaaaaaaaceaaaaa
aaaaialpaaaaiadpaaaaiadpaaaaialpegbebaaaabaaaaaaefaaaaajpcaabaaa
acaaaaaaegaabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaaefaaaaaj
pcaabaaaabaaaaaaogakbaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaa
aaaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaacaaaaaaaaaaaaah
hcaabaaaaaaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaakhccabaaa
aaaaaaaaegacbaaaaaaaaaaaaceaaaaadjiooddndjiooddndjiooddnaaaaaaaa
dgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaiadpdoaaaaab""
}
SubProgram ""gles "" {
""!!GLES""
}
SubProgram ""d3d11_9x "" {
// Stats: 16 math, 9 textures
SetTexture 0 [_MainTex] 2D 0
ConstBuffer ""$Globals"" 128
Float 96 [_PixelSizeX]
Float 100 [_PixelSizeY]
BindCB  ""$Globals"" 0
""ps_4_0_level_9_1
eefiecedkmmfojdhgdehbpghdppmifgnmcljjeleabaaaaaaeeahaaaaaeaaaaaa
daaaaaaaliacaaaaliagaaaabaahaaaaebgpgodjiaacaaaaiaacaaaaaaacpppp
emacaaaadeaaaaaaabaaciaaaaaadeaaaaaadeaaabaaceaaaaaadeaaaaaaaaaa
aaaaagaaabaaaaaaaaaaaaaaaaacppppfbaaaaafabaaapkaaaaaaaaaaaaaialp
aaaaiadpdjiooddnbpaaaaacaaaaaaiaaaaaadlabpaaaaacaaaaaajaaaaiapka
acaaaaadaaaaabiaaaaaaalaaaaaaakaabaaaaacaaaaaciaaaaafflaabaaaaac
abaaabiaaaaaaakbabaaaaacabaaaciaabaaaakaacaaaaadabaaadiaabaaoeia
aaaaoelaabaaaaacacaaabiaaaaaaalaacaaaaadacaaaciaaaaafflaaaaaffka
abaaaaacadaaabiaaaaaaalaacaaaaadadaaaciaaaaafflaaaaaffkbacaaaaad
aeaaadiaaaaaoelaaaaaoekaacaaaaadafaaadiaaaaaoelaaaaaoekbabaaaaac
agaaadiaaaaaoekaaeaaaaaeagaaadiaagaaoeiaabaamjkaaaaaoelaacaaaaad
ahaaabiaaaaaaalaaaaaaakaacaaaaadahaaaciaaaaafflaaaaaffkbecaaaaad
aaaaapiaaaaaoeiaaaaioekaecaaaaadaiaacpiaaaaaoelaaaaioekaecaaaaad
abaaapiaabaaoeiaaaaioekaecaaaaadacaaapiaacaaoeiaaaaioekaecaaaaad
adaaapiaadaaoeiaaaaioekaecaaaaadaeaaapiaaeaaoeiaaaaioekaecaaaaad
afaaapiaafaaoeiaaaaioekaecaaaaadagaaapiaagaaoeiaaaaioekaecaaaaad
ahaaapiaahaaoeiaaaaioekaacaaaaadaaaachiaaaaaoeiaaiaaoeiaacaaaaad
aaaachiaabaaoeiaaaaaoeiaacaaaaadaaaachiaacaaoeiaaaaaoeiaacaaaaad
aaaachiaadaaoeiaaaaaoeiaacaaaaadaaaachiaaeaaoeiaaaaaoeiaacaaaaad
aaaachiaafaaoeiaaaaaoeiaacaaaaadaaaachiaagaaoeiaaaaaoeiaacaaaaad
aaaachiaahaaoeiaaaaaoeiaafaaaaadaaaachiaaaaaoeiaabaappkaabaaaaac
aaaaciiaabaakkkaabaaaaacaaaicpiaaaaaoeiappppaaaafdeieefcpiadaaaa
eaaaaaaapoaaaaaafjaaaaaeegiocaaaaaaaaaaaahaaaaaafkaaaaadaagabaaa
aaaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaagcbaaaaddcbabaaaabaaaaaa
gfaaaaadpccabaaaaaaaaaaagiaaaaacaeaaaaaadgaaaaagjcaabaaaaaaaaaaa
agiecaaaaaaaaaaaagaaaaaadgaaaaaigcaabaaaaaaaaaaaaceaaaaaaaaaaaaa
aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaahpcaabaaaaaaaaaaaegaobaaaaaaaaaaa
egbebaaaabaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaaaaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaaefaaaaajpcaabaaaaaaaaaaaogakbaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaaefaaaaajpcaabaaaacaaaaaaegbabaaa
abaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaaaaaaaaahhcaabaaaabaaaaaa
egacbaaaabaaaaaaegacbaaaacaaaaaadgaaaaahhcaabaaaacaaaaaaegiacaia
ebaaaaaaaaaaaaaaagaaaaaadgaaaaaficaabaaaacaaaaaaabeaaaaaaaaaaaaa
aaaaaaahpcaabaaaacaaaaaaogaebaaaacaaaaaaegbebaaaabaaaaaaefaaaaaj
pcaabaaaadaaaaaaegaabaaaacaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaa
efaaaaajpcaabaaaacaaaaaaogakbaaaacaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaaaaaaaaahhcaabaaaabaaaaaaegacbaaaabaaaaaaegacbaaaadaaaaaa
aaaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaabaaaaaadgaaaaaf
bcaabaaaabaaaaaaabeaaaaaaaaaaaaadgaaaaahccaabaaaabaaaaaabkiacaia
ebaaaaaaaaaaaaaaagaaaaaaaaaaaaahdcaabaaaabaaaaaaegaabaaaabaaaaaa
egbabaaaabaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaaaaaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaaaaaaaaaidcaabaaaabaaaaaaegbabaaaabaaaaaaegiacaaa
aaaaaaaaagaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaaaaaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaa
egacbaaaabaaaaaaaaaaaaahhcaabaaaaaaaaaaaegacbaaaacaaaaaaegacbaaa
aaaaaaaadcaaaaanpcaabaaaabaaaaaaegiecaaaaaaaaaaaagaaaaaaaceaaaaa
aaaaialpaaaaiadpaaaaiadpaaaaialpegbebaaaabaaaaaaefaaaaajpcaabaaa
acaaaaaaegaabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaaefaaaaaj
pcaabaaaabaaaaaaogakbaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaa
aaaaaaahhcaabaaaaaaaaaaaegacbaaaaaaaaaaaegacbaaaacaaaaaaaaaaaaah
hcaabaaaaaaaaaaaegacbaaaabaaaaaaegacbaaaaaaaaaaadiaaaaakhccabaaa
aaaaaaaaegacbaaaaaaaaaaaaceaaaaadjiooddndjiooddndjiooddnaaaaaaaa
dgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaiadpdoaaaaabejfdeheofaaaaaaa
acaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaa
eeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaaadadaaaafdfgfpfaepfdejfe
ejepeoaafeeffiedepepfceeaaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaa
caaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgf
heaaklkl""
}
SubProgram ""gles3 "" {
""!!GLES3""
}
SubProgram ""metal "" {
// Stats: 24 math, 9 textures
SetTexture 0 [_MainTex] 2D 0
ConstBuffer ""$Globals"" 8
Float 0 [_PixelSizeX]
Float 4 [_PixelSizeY]
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
  float _PixelSizeX;
  float _PixelSizeY;
};
fragment xlatMtlShaderOutput xlatMtlMain (xlatMtlShaderInput _mtl_i [[stage_in]], constant xlatMtlShaderUniform& _mtl_u [[buffer(0)]]
  ,   texture2d<half> _MainTex [[texture(0)]], sampler _mtlsmp__MainTex [[sampler(0)]])
{
  xlatMtlShaderOutput _mtl_o;
  half4 col_1;
  float2 tmpvar_2;
  tmpvar_2.y = 0.0;
  tmpvar_2.x = _mtl_u._PixelSizeX;
  float2 P_3;
  P_3 = ((float2)_mtl_i.xlv_TEXCOORD0 + tmpvar_2);
  float2 tmpvar_4;
  tmpvar_4.y = 0.0;
  float cse_5;
  cse_5 = -(_mtl_u._PixelSizeX);
  tmpvar_4.x = cse_5;
  float2 P_6;
  P_6 = ((float2)_mtl_i.xlv_TEXCOORD0 + tmpvar_4);
  float2 tmpvar_7;
  tmpvar_7.x = 0.0;
  tmpvar_7.y = _mtl_u._PixelSizeY;
  float2 P_8;
  P_8 = ((float2)_mtl_i.xlv_TEXCOORD0 + tmpvar_7);
  float2 tmpvar_9;
  tmpvar_9.x = 0.0;
  float cse_10;
  cse_10 = -(_mtl_u._PixelSizeY);
  tmpvar_9.y = cse_10;
  float2 P_11;
  P_11 = ((float2)_mtl_i.xlv_TEXCOORD0 + tmpvar_9);
  float2 tmpvar_12;
  tmpvar_12.x = _mtl_u._PixelSizeX;
  tmpvar_12.y = _mtl_u._PixelSizeY;
  float2 P_13;
  P_13 = ((float2)_mtl_i.xlv_TEXCOORD0 + tmpvar_12);
  float2 tmpvar_14;
  tmpvar_14.x = cse_5;
  tmpvar_14.y = cse_10;
  float2 P_15;
  P_15 = ((float2)_mtl_i.xlv_TEXCOORD0 + tmpvar_14);
  float2 tmpvar_16;
  tmpvar_16.x = cse_5;
  tmpvar_16.y = _mtl_u._PixelSizeY;
  float2 P_17;
  P_17 = ((float2)_mtl_i.xlv_TEXCOORD0 + tmpvar_16);
  float2 tmpvar_18;
  tmpvar_18.x = _mtl_u._PixelSizeX;
  tmpvar_18.y = cse_10;
  float2 P_19;
  P_19 = ((float2)_mtl_i.xlv_TEXCOORD0 + tmpvar_18);
  col_1.xyz = (((
    ((((
      ((_MainTex.sample(_mtlsmp__MainTex, (float2)(_mtl_i.xlv_TEXCOORD0)) + _MainTex.sample(_mtlsmp__MainTex, (float2)(P_3))) + _MainTex.sample(_mtlsmp__MainTex, (float2)(P_6)))
     + _MainTex.sample(_mtlsmp__MainTex, (float2)(P_8))) + _MainTex.sample(_mtlsmp__MainTex, (float2)(P_11))) + _MainTex.sample(_mtlsmp__MainTex, (float2)(P_13))) + _MainTex.sample(_mtlsmp__MainTex, (float2)(P_15)))
   + _MainTex.sample(_mtlsmp__MainTex, (float2)(P_17))) + _MainTex.sample(_mtlsmp__MainTex, (float2)(P_19))) / (half)9.0).xyz;
  col_1.w = half(1.0);
  _mtl_o._glesFragData_0 = col_1;
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
