using System.Reflection;
using UnityEngine;

namespace DynamicResolution
{
    public class CameraRenderer : MonoBehaviour
    {

        public RenderTexture fullResRT;
        public RenderTexture halfVerticalResRT;

        public static Camera mainCamera;
        public Camera camera;

        private Material downsampleShader;
        private Material downsampleX2Shader;

        private static UndergroundView undergroundView;
        private static Camera undergroundCamera;

        private static FieldInfo undergroundRGBDField;

        public void Awake()
        {
            camera = GetComponent<Camera>();
            downsampleShader = new Material(downsampleShaderSource);
            downsampleX2Shader = new Material(downsampleX2ShaderSource);

            undergroundView = FindObjectOfType<UndergroundView>();
            undergroundRGBDField = typeof (UndergroundView).GetField("m_undergroundRGBD",
                BindingFlags.Instance | BindingFlags.NonPublic);

            undergroundCamera = Util.GetPrivate<Camera>(undergroundView, "m_undergroundCamera");

            RedirectionHelper.RedirectCalls
            (
                typeof (UndergroundView).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic),
                typeof (CameraRenderer).GetMethod("UndegroundViewLateUpdate", BindingFlags.Instance | BindingFlags.NonPublic)
            );
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

        void UndegroundViewLateUpdate()
        {
            var undergroundRGBD = Util.GetFieldValue<RenderTexture>(undergroundRGBDField, undergroundView);

            if (undergroundRGBD != null)
            {
                RenderTexture.ReleaseTemporary(undergroundRGBD);
                Util.SetFieldValue(undergroundRGBDField, undergroundView, null);
            }

            if (undergroundCamera != null && mainCamera != null)
            {
                if (undergroundCamera.cullingMask != 0)
                {
                    int width = CameraHook.instance.width;
                    int height = CameraHook.instance.height;
                    undergroundRGBD = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);

                    undergroundCamera.fieldOfView = mainCamera.fieldOfView;
                    undergroundCamera.nearClipPlane = mainCamera.nearClipPlane;
                    undergroundCamera.farClipPlane = mainCamera.farClipPlane;
                    undergroundCamera.rect = mainCamera.rect;
                    undergroundCamera.targetTexture = undergroundRGBD;
                    undergroundCamera.enabled = true;

                    Util.SetFieldValue(undergroundRGBDField, undergroundView, undergroundRGBD);
                }
                else
                {
                    undergroundCamera.enabled = false;
                }
            }
        }

        public void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (fullResRT == null)
            {
                return;
            }

            mainCamera.targetTexture = fullResRT;
            mainCamera.Render();
            mainCamera.targetTexture = null;
            
            float factor = CameraHook.instance.currentSSAAFactor;

            if (factor != 1.0f)
            {
                Material shader = downsampleShader;

                if (factor <= 2.0f)
                {
                    shader = downsampleX2Shader;
                }

                downsampleShader.SetVector("_ResampleOffset", new Vector4(fullResRT.texelSize.x, 0.0f, 0.0f, 0.0f));
                Graphics.Blit(fullResRT, halfVerticalResRT, shader);

                downsampleShader.SetVector("_ResampleOffset", new Vector4(0.0f, fullResRT.texelSize.y, 0.0f, 0.0f));
                Graphics.Blit(halfVerticalResRT, dst, shader);
            }
            else
            {
                Graphics.Blit(fullResRT, dst);
            }
        }

        private readonly string downsampleX2ShaderSource = @"// Compiled shader for all platforms, uncompressed size: 11.4KB

// Skipping shader variants that would not be included into build of current scene.

Shader ""Downsample_x2"" {
Properties {
 _MainTex (""Base (RGB)"", 2D) = ""white"" { }
 _ResampleOffset (""_ResampleOffset"", Vector) = (0,0,0,0)
}
SubShader { 
 LOD 100
 Tags { ""RenderType""=""Opaque"" }


 // Stats for Vertex shader:
 //       d3d11 : 5 math
 //    d3d11_9x : 5 math
 //        d3d9 : 5 math
 //        gles : 5 math, 2 texture
 //       gles3 : 5 math, 2 texture
 //       metal : 3 math
 //      opengl : 5 math, 2 texture
 // Stats for Fragment shader:
 //       d3d11 : 3 math, 2 texture
 //    d3d11_9x : 3 math, 2 texture
 //        d3d9 : 5 math, 2 texture
 //       metal : 5 math, 2 texture
 Pass {
  Tags { ""RenderType""=""Opaque"" }
  GpuProgramID 60791
Program ""vp"" {
SubProgram ""opengl "" {
// Stats: 5 math, 2 textures
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
uniform vec2 _ResampleOffset;
varying vec2 xlv_TEXCOORD0;
void main ()
{
  vec4 col_1;
  col_1.xyz = ((texture2D (_MainTex, xlv_TEXCOORD0) * 0.5) + (texture2D (_MainTex, (xlv_TEXCOORD0 + _ResampleOffset)) * 0.5)).xyz;
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
// Stats: 5 math, 2 textures
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
uniform highp vec2 _ResampleOffset;
varying mediump vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 col_1;
  highp vec2 P_2;
  P_2 = (xlv_TEXCOORD0 + _ResampleOffset);
  col_1.xyz = ((texture2D (_MainTex, xlv_TEXCOORD0) * 0.5) + (texture2D (_MainTex, P_2) * 0.5)).xyz;
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
// Stats: 5 math, 2 textures
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
uniform highp vec2 _ResampleOffset;
in mediump vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 col_1;
  highp vec2 P_2;
  P_2 = (xlv_TEXCOORD0 + _ResampleOffset);
  col_1.xyz = ((texture (_MainTex, xlv_TEXCOORD0) * 0.5) + (texture (_MainTex, P_2) * 0.5)).xyz;
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
// Stats: 5 math, 2 textures
Vector 0 [_ResampleOffset]
SetTexture 0 [_MainTex] 2D 0
""ps_2_0
def c1, 0.5, 1, 0, 0
dcl t0.xy
dcl_2d s0
add r0.xy, t0, c0
texld r0, r0, s0
texld r1, t0, s0
mul r0.xyz, r0, c1.x
mad_pp r0.xyz, r1, c1.x, r0
mov_pp r0.w, c1.y
mov_pp oC0, r0

""
}
SubProgram ""d3d11 "" {
// Stats: 3 math, 2 textures
SetTexture 0 [_MainTex] 2D 0
ConstBuffer ""$Globals"" 128
Vector 96 [_ResampleOffset] 2
BindCB  ""$Globals"" 0
""ps_4_0
eefiecedihnkjoohpcckbgjnkgnacgameaolabfcabaaaaaaomabaaaaadaaaaaa
cmaaaaaaieaaaaaaliaaaaaaejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefccmabaaaa
eaaaaaaaelaaaaaafjaaaaaeegiocaaaaaaaaaaaahaaaaaafkaaaaadaagabaaa
aaaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaagcbaaaaddcbabaaaabaaaaaa
gfaaaaadpccabaaaaaaaaaaagiaaaaacacaaaaaaaaaaaaaidcaabaaaaaaaaaaa
egbabaaaabaaaaaaegiacaaaaaaaaaaaagaaaaaaefaaaaajpcaabaaaaaaaaaaa
egaabaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaakhcaabaaa
aaaaaaaaegacbaaaaaaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaadpaaaaaaaa
efaaaaajpcaabaaaabaaaaaaegbabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaadcaaaaamhccabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaaaaaaaadp
aaaaaadpaaaaaadpaaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaa
abeaaaaaaaaaiadpdoaaaaab""
}
SubProgram ""gles "" {
""!!GLES""
}
SubProgram ""d3d11_9x "" {
// Stats: 3 math, 2 textures
SetTexture 0 [_MainTex] 2D 0
ConstBuffer ""$Globals"" 128
Vector 96 [_ResampleOffset] 2
BindCB  ""$Globals"" 0
""ps_4_0_level_9_1
eefiecedcdcaomnipkgbffggcmjlljnpgbcjcahaabaaaaaanaacaaaaaeaaaaaa
daaaaaaabaabaaaaeeacaaaajmacaaaaebgpgodjniaaaaaaniaaaaaaaaacpppp
keaaaaaadeaaaaaaabaaciaaaaaadeaaaaaadeaaabaaceaaaaaadeaaaaaaaaaa
aaaaagaaabaaaaaaaaaaaaaaaaacppppfbaaaaafabaaapkaaaaaaadpaaaaiadp
aaaaaaaaaaaaaaaabpaaaaacaaaaaaiaaaaaadlabpaaaaacaaaaaajaaaaiapka
acaaaaadaaaaadiaaaaaoelaaaaaoekaecaaaaadaaaaapiaaaaaoeiaaaaioeka
ecaaaaadabaaapiaaaaaoelaaaaioekaafaaaaadaaaaahiaaaaaoeiaabaaaaka
aeaaaaaeaaaachiaabaaoeiaabaaaakaaaaaoeiaabaaaaacaaaaciiaabaaffka
abaaaaacaaaicpiaaaaaoeiappppaaaafdeieefccmabaaaaeaaaaaaaelaaaaaa
fjaaaaaeegiocaaaaaaaaaaaahaaaaaafkaaaaadaagabaaaaaaaaaaafibiaaae
aahabaaaaaaaaaaaffffaaaagcbaaaaddcbabaaaabaaaaaagfaaaaadpccabaaa
aaaaaaaagiaaaaacacaaaaaaaaaaaaaidcaabaaaaaaaaaaaegbabaaaabaaaaaa
egiacaaaaaaaaaaaagaaaaaaefaaaaajpcaabaaaaaaaaaaaegaabaaaaaaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaakhcaabaaaaaaaaaaaegacbaaa
aaaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaadpaaaaaaaaefaaaaajpcaabaaa
abaaaaaaegbabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaam
hccabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaaaaaaaadpaaaaaadpaaaaaadp
aaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaaabeaaaaaaaaaiadp
doaaaaabejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaaaaaaaaaaabaaaaaa
adaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaaadaaaaaaabaaaaaa
adadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfceeaaklklklepfdeheo
cmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaaadaaaaaaaaaaaaaa
apaaaaaafdfgfpfegbhcghgfheaaklkl""
}
SubProgram ""gles3 "" {
""!!GLES3""
}
SubProgram ""metal "" {
// Stats: 5 math, 2 textures
SetTexture 0 [_MainTex] 2D 0
ConstBuffer ""$Globals"" 8
Vector 0 [_ResampleOffset] 2
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
  float2 _ResampleOffset;
};
fragment xlatMtlShaderOutput xlatMtlMain (xlatMtlShaderInput _mtl_i [[stage_in]], constant xlatMtlShaderUniform& _mtl_u [[buffer(0)]]
  ,   texture2d<half> _MainTex [[texture(0)]], sampler _mtlsmp__MainTex [[sampler(0)]])
{
  xlatMtlShaderOutput _mtl_o;
  half4 col_1;
  float2 P_2;
  P_2 = ((float2)_mtl_i.xlv_TEXCOORD0 + _mtl_u._ResampleOffset);
  col_1.xyz = ((_MainTex.sample(_mtlsmp__MainTex, (float2)(_mtl_i.xlv_TEXCOORD0)) * (half)0.5) + (_MainTex.sample(_mtlsmp__MainTex, (float2)(P_2)) * (half)0.5)).xyz;
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

        private readonly string downsampleShaderSource = @"// Compiled shader for all platforms, uncompressed size: 19.6KB

// Skipping shader variants that would not be included into build of current scene.

Shader ""LancsozResample"" {
Properties {
 _MainTex (""Base (RGB)"", 2D) = ""white"" { }
 _ResampleOffset (""_ResampleOffset"", Vector) = (0,0,0,0)
}
SubShader { 
 LOD 100
 Tags { ""RenderType""=""Opaque"" }


 // Stats for Vertex shader:
 //       d3d11 : 5 math
 //    d3d11_9x : 5 math
 //        d3d9 : 5 math
 //        gles : 33 math, 9 texture
 //       gles3 : 33 math, 9 texture
 //       metal : 3 math
 //      opengl : 33 math, 9 texture
 // Stats for Fragment shader:
 //       d3d11 : 15 math, 9 texture
 //    d3d11_9x : 15 math, 9 texture
 //        d3d9 : 21 math, 9 texture
 //       metal : 33 math, 9 texture
 Pass {
  Tags { ""RenderType""=""Opaque"" }
  GpuProgramID 11936
Program ""vp"" {
SubProgram ""opengl "" {
// Stats: 33 math, 9 textures
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
uniform vec2 _ResampleOffset;
varying vec2 xlv_TEXCOORD0;
void main ()
{
  vec4 col_1;
  vec2 cse_2;
  cse_2 = -(_ResampleOffset);
  col_1.xyz = (((
    ((((
      ((texture2D (_MainTex, xlv_TEXCOORD0) * 0.38026) + (texture2D (_MainTex, (xlv_TEXCOORD0 + cse_2)) * 0.27667))
     + 
      (texture2D (_MainTex, (xlv_TEXCOORD0 + _ResampleOffset)) * 0.27667)
    ) + (texture2D (_MainTex, 
      (xlv_TEXCOORD0 + (2.0 * cse_2))
    ) * 0.08074)) + (texture2D (_MainTex, (xlv_TEXCOORD0 + 
      (2.0 * _ResampleOffset)
    )) * 0.08074)) + (texture2D (_MainTex, (xlv_TEXCOORD0 + (3.0 * cse_2))) * -0.02612))
   + 
    (texture2D (_MainTex, (xlv_TEXCOORD0 + (3.0 * _ResampleOffset))) * -0.02612)
  ) + (texture2D (_MainTex, 
    (xlv_TEXCOORD0 + (4.0 * cse_2))
  ) * -0.02143)) + (texture2D (_MainTex, (xlv_TEXCOORD0 + 
    (4.0 * _ResampleOffset)
  )) * -0.02143)).xyz;
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
// Stats: 33 math, 9 textures
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
uniform highp vec2 _ResampleOffset;
varying mediump vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 col_1;
  highp vec2 P_2;
  highp vec2 cse_3;
  cse_3 = -(_ResampleOffset);
  P_2 = (xlv_TEXCOORD0 + cse_3);
  highp vec2 P_4;
  P_4 = (xlv_TEXCOORD0 + _ResampleOffset);
  highp vec2 P_5;
  P_5 = (xlv_TEXCOORD0 + (2.0 * cse_3));
  highp vec2 P_6;
  P_6 = (xlv_TEXCOORD0 + (2.0 * _ResampleOffset));
  highp vec2 P_7;
  P_7 = (xlv_TEXCOORD0 + (3.0 * cse_3));
  highp vec2 P_8;
  P_8 = (xlv_TEXCOORD0 + (3.0 * _ResampleOffset));
  highp vec2 P_9;
  P_9 = (xlv_TEXCOORD0 + (4.0 * cse_3));
  highp vec2 P_10;
  P_10 = (xlv_TEXCOORD0 + (4.0 * _ResampleOffset));
  col_1.xyz = (((
    ((((
      ((texture2D (_MainTex, xlv_TEXCOORD0) * 0.38026) + (texture2D (_MainTex, P_2) * 0.27667))
     + 
      (texture2D (_MainTex, P_4) * 0.27667)
    ) + (texture2D (_MainTex, P_5) * 0.08074)) + (texture2D (_MainTex, P_6) * 0.08074)) + (texture2D (_MainTex, P_7) * -0.02612))
   + 
    (texture2D (_MainTex, P_8) * -0.02612)
  ) + (texture2D (_MainTex, P_9) * -0.02143)) + (texture2D (_MainTex, P_10) * -0.02143)).xyz;
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
// Stats: 33 math, 9 textures
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
uniform highp vec2 _ResampleOffset;
in mediump vec2 xlv_TEXCOORD0;
void main ()
{
  lowp vec4 col_1;
  highp vec2 P_2;
  highp vec2 cse_3;
  cse_3 = -(_ResampleOffset);
  P_2 = (xlv_TEXCOORD0 + cse_3);
  highp vec2 P_4;
  P_4 = (xlv_TEXCOORD0 + _ResampleOffset);
  highp vec2 P_5;
  P_5 = (xlv_TEXCOORD0 + (2.0 * cse_3));
  highp vec2 P_6;
  P_6 = (xlv_TEXCOORD0 + (2.0 * _ResampleOffset));
  highp vec2 P_7;
  P_7 = (xlv_TEXCOORD0 + (3.0 * cse_3));
  highp vec2 P_8;
  P_8 = (xlv_TEXCOORD0 + (3.0 * _ResampleOffset));
  highp vec2 P_9;
  P_9 = (xlv_TEXCOORD0 + (4.0 * cse_3));
  highp vec2 P_10;
  P_10 = (xlv_TEXCOORD0 + (4.0 * _ResampleOffset));
  col_1.xyz = (((
    ((((
      ((texture (_MainTex, xlv_TEXCOORD0) * 0.38026) + (texture (_MainTex, P_2) * 0.27667))
     + 
      (texture (_MainTex, P_4) * 0.27667)
    ) + (texture (_MainTex, P_5) * 0.08074)) + (texture (_MainTex, P_6) * 0.08074)) + (texture (_MainTex, P_7) * -0.02612))
   + 
    (texture (_MainTex, P_8) * -0.02612)
  ) + (texture (_MainTex, P_9) * -0.02143)) + (texture (_MainTex, P_10) * -0.02143)).xyz;
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
// Stats: 21 math, 9 textures
Vector 0 [_ResampleOffset]
SetTexture 0 [_MainTex] 2D 0
""ps_2_0
def c1, 0.276670009, 0.380259991, -2, 0.0807399973
def c2, -3, -0.0261199996, 3, -4
def c3, -0.0214300007, 1, 0, 0
dcl t0.xy
dcl_2d s0
add r0.xy, t0, -c0
add r1.xy, t0, c0
mov r0.z, c1.z
mad r2.xy, c0, r0.z, t0
mad r3.xy, c0, -r0.z, t0
mov r4.xy, c0
mad r5.xy, r4, c2.x, t0
mad r6.xy, r4, c2.z, t0
mad r7.xy, r4, c2.w, t0
mad r4.xy, r4, -c2.w, t0
texld r0, r0, s0
texld r8, t0, s0
texld r1, r1, s0
texld r2, r2, s0
texld r3, r3, s0
texld r5, r5, s0
texld r6, r6, s0
texld r7, r7, s0
texld r4, r4, s0
mul r0.xyz, r0, c1.x
mad_pp r0.xyz, r8, c1.y, r0
mad_pp r0.xyz, r1, c1.x, r0
mad_pp r0.xyz, r2, c1.w, r0
mad_pp r0.xyz, r3, c1.w, r0
mad_pp r0.xyz, r5, c2.y, r0
mad_pp r0.xyz, r6, c2.y, r0
mad_pp r0.xyz, r7, c3.x, r0
mad_pp r0.xyz, r4, c3.x, r0
mov_pp r0.w, c3.y
mov_pp oC0, r0

""
}
SubProgram ""d3d11 "" {
// Stats: 15 math, 9 textures
SetTexture 0 [_MainTex] 2D 0
ConstBuffer ""$Globals"" 128
Vector 96 [_ResampleOffset] 2
BindCB  ""$Globals"" 0
""ps_4_0
eefiecedniblpifgigbfpjoeihdffkfjbgadlnegabaaaaaacmafaaaaadaaaaaa
cmaaaaaaieaaaaaaliaaaaaaejfdeheofaaaaaaaacaaaaaaaiaaaaaadiaaaaaa
aaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaabaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffiedepepfcee
aaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaaaaaaaaaa
adaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklklfdeieefcgmaeaaaa
eaaaaaaablabaaaafjaaaaaeegiocaaaaaaaaaaaahaaaaaafkaaaaadaagabaaa
aaaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaagcbaaaaddcbabaaaabaaaaaa
gfaaaaadpccabaaaaaaaaaaagiaaaaacadaaaaaaaaaaaaajdcaabaaaaaaaaaaa
egbabaaaabaaaaaaegiacaiaebaaaaaaaaaaaaaaagaaaaaaefaaaaajpcaabaaa
aaaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadiaaaaak
hcaabaaaaaaaaaaaegacbaaaaaaaaaaaaceaaaaalbkhindolbkhindolbkhindo
aaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaabaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaa
halbmcdohalbmcdohalbmcdoaaaaaaaaegacbaaaaaaaaaaaaaaaaaaidcaabaaa
abaaaaaaegbabaaaabaaaaaaegiacaaaaaaaaaaaagaaaaaaefaaaaajpcaabaaa
abaaaaaaegaabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaam
hcaabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaalbkhindolbkhindolbkhindo
aaaaaaaaegacbaaaaaaaaaaadcaaaaanpcaabaaaabaaaaaaegiecaaaaaaaaaaa
agaaaaaaaceaaaaaaaaaaamaaaaaaamaaaaaeamaaaaaeamaegbebaaaabaaaaaa
efaaaaajpcaabaaaacaaaaaaegaabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaaefaaaaajpcaabaaaabaaaaaaogakbaaaabaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaaacaaaaaaaceaaaaa
adflkfdnadflkfdnadflkfdnaaaaaaaaegacbaaaaaaaaaaadcaaaaandcaabaaa
acaaaaaaegiacaaaaaaaaaaaagaaaaaaaceaaaaaaaaaaaeaaaaaaaeaaaaaaaaa
aaaaaaaaegbabaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaaacaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaa
acaaaaaaaceaaaaaadflkfdnadflkfdnadflkfdnaaaaaaaaegacbaaaaaaaaaaa
dcaaaaamhcaabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaajmpjnflmjmpjnflm
jmpjnflmaaaaaaaaegacbaaaaaaaaaaadcaaaaanpcaabaaaabaaaaaaegiecaaa
aaaaaaaaagaaaaaaaceaaaaaaaaaeaeaaaaaeaeaaaaaiamaaaaaiamaegbebaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaaabaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaogakbaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaaacaaaaaa
aceaaaaajmpjnflmjmpjnflmjmpjnflmaaaaaaaaegacbaaaaaaaaaaadcaaaaam
hcaabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaapiinkplmpiinkplmpiinkplm
aaaaaaaaegacbaaaaaaaaaaadcaaaaandcaabaaaabaaaaaaegiacaaaaaaaaaaa
agaaaaaaaceaaaaaaaaaiaeaaaaaiaeaaaaaaaaaaaaaaaaaegbabaaaabaaaaaa
efaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaa
aaaaaaaadcaaaaamhccabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaapiinkplm
piinkplmpiinkplmaaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaaaaaaaaaa
abeaaaaaaaaaiadpdoaaaaab""
}
SubProgram ""gles "" {
""!!GLES""
}
SubProgram ""d3d11_9x "" {
// Stats: 15 math, 9 textures
SetTexture 0 [_MainTex] 2D 0
ConstBuffer ""$Globals"" 128
Vector 96 [_ResampleOffset] 2
BindCB  ""$Globals"" 0
""ps_4_0_level_9_1
eefiecedfgdnigikofahfphgjobgcagkinkphpogabaaaaaanmahaaaaaeaaaaaa
daaaaaaanmacaaaafaahaaaakiahaaaaebgpgodjkeacaaaakeacaaaaaaacpppp
haacaaaadeaaaaaaabaaciaaaaaadeaaaaaadeaaabaaceaaaaaadeaaaaaaaaaa
aaaaagaaabaaaaaaaaaaaaaaaaacppppfbaaaaafabaaapkalbkhindohalbmcdo
aaaaaamaadflkfdnfbaaaaafacaaapkaaaaaeamajmpjnflmaaaaeaeaaaaaiama
fbaaaaafadaaapkapiinkplmaaaaiadpaaaaaaaaaaaaaaaabpaaaaacaaaaaaia
aaaaadlabpaaaaacaaaaaajaaaaiapkaacaaaaadaaaaadiaaaaaoelaaaaaoekb
acaaaaadabaaadiaaaaaoelaaaaaoekaabaaaaacaaaaaeiaabaakkkaaeaaaaae
acaaadiaaaaaoekaaaaakkiaaaaaoelaaeaaaaaeadaaadiaaaaaoekaaaaakkib
aaaaoelaabaaaaacaeaaadiaaaaaoekaaeaaaaaeafaaadiaaeaaoeiaacaaaaka
aaaaoelaaeaaaaaeagaaadiaaeaaoeiaacaakkkaaaaaoelaaeaaaaaeahaaadia
aeaaoeiaacaappkaaaaaoelaaeaaaaaeaeaaadiaaeaaoeiaacaappkbaaaaoela
ecaaaaadaaaaapiaaaaaoeiaaaaioekaecaaaaadaiaaapiaaaaaoelaaaaioeka
ecaaaaadabaaapiaabaaoeiaaaaioekaecaaaaadacaaapiaacaaoeiaaaaioeka
ecaaaaadadaaapiaadaaoeiaaaaioekaecaaaaadafaaapiaafaaoeiaaaaioeka
ecaaaaadagaaapiaagaaoeiaaaaioekaecaaaaadahaaapiaahaaoeiaaaaioeka
ecaaaaadaeaaapiaaeaaoeiaaaaioekaafaaaaadaaaaahiaaaaaoeiaabaaaaka
aeaaaaaeaaaachiaaiaaoeiaabaaffkaaaaaoeiaaeaaaaaeaaaachiaabaaoeia
abaaaakaaaaaoeiaaeaaaaaeaaaachiaacaaoeiaabaappkaaaaaoeiaaeaaaaae
aaaachiaadaaoeiaabaappkaaaaaoeiaaeaaaaaeaaaachiaafaaoeiaacaaffka
aaaaoeiaaeaaaaaeaaaachiaagaaoeiaacaaffkaaaaaoeiaaeaaaaaeaaaachia
ahaaoeiaadaaaakaaaaaoeiaaeaaaaaeaaaachiaaeaaoeiaadaaaakaaaaaoeia
abaaaaacaaaaciiaadaaffkaabaaaaacaaaicpiaaaaaoeiappppaaaafdeieefc
gmaeaaaaeaaaaaaablabaaaafjaaaaaeegiocaaaaaaaaaaaahaaaaaafkaaaaad
aagabaaaaaaaaaaafibiaaaeaahabaaaaaaaaaaaffffaaaagcbaaaaddcbabaaa
abaaaaaagfaaaaadpccabaaaaaaaaaaagiaaaaacadaaaaaaaaaaaaajdcaabaaa
aaaaaaaaegbabaaaabaaaaaaegiacaiaebaaaaaaaaaaaaaaagaaaaaaefaaaaaj
pcaabaaaaaaaaaaaegaabaaaaaaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaa
diaaaaakhcaabaaaaaaaaaaaegacbaaaaaaaaaaaaceaaaaalbkhindolbkhindo
lbkhindoaaaaaaaaefaaaaajpcaabaaaabaaaaaaegbabaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaaabaaaaaa
aceaaaaahalbmcdohalbmcdohalbmcdoaaaaaaaaegacbaaaaaaaaaaaaaaaaaai
dcaabaaaabaaaaaaegbabaaaabaaaaaaegiacaaaaaaaaaaaagaaaaaaefaaaaaj
pcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaa
dcaaaaamhcaabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaalbkhindolbkhindo
lbkhindoaaaaaaaaegacbaaaaaaaaaaadcaaaaanpcaabaaaabaaaaaaegiecaaa
aaaaaaaaagaaaaaaaceaaaaaaaaaaamaaaaaaamaaaaaeamaaaaaeamaegbebaaa
abaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaaabaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaogakbaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaaacaaaaaa
aceaaaaaadflkfdnadflkfdnadflkfdnaaaaaaaaegacbaaaaaaaaaaadcaaaaan
dcaabaaaacaaaaaaegiacaaaaaaaaaaaagaaaaaaaceaaaaaaaaaaaeaaaaaaaea
aaaaaaaaaaaaaaaaegbabaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaa
acaaaaaaeghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaa
egacbaaaacaaaaaaaceaaaaaadflkfdnadflkfdnadflkfdnaaaaaaaaegacbaaa
aaaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaajmpjnflm
jmpjnflmjmpjnflmaaaaaaaaegacbaaaaaaaaaaadcaaaaanpcaabaaaabaaaaaa
egiecaaaaaaaaaaaagaaaaaaaceaaaaaaaaaeaeaaaaaeaeaaaaaiamaaaaaiama
egbebaaaabaaaaaaefaaaaajpcaabaaaacaaaaaaegaabaaaabaaaaaaeghobaaa
aaaaaaaaaagabaaaaaaaaaaaefaaaaajpcaabaaaabaaaaaaogakbaaaabaaaaaa
eghobaaaaaaaaaaaaagabaaaaaaaaaaadcaaaaamhcaabaaaaaaaaaaaegacbaaa
acaaaaaaaceaaaaajmpjnflmjmpjnflmjmpjnflmaaaaaaaaegacbaaaaaaaaaaa
dcaaaaamhcaabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaapiinkplmpiinkplm
piinkplmaaaaaaaaegacbaaaaaaaaaaadcaaaaandcaabaaaabaaaaaaegiacaaa
aaaaaaaaagaaaaaaaceaaaaaaaaaiaeaaaaaiaeaaaaaaaaaaaaaaaaaegbabaaa
abaaaaaaefaaaaajpcaabaaaabaaaaaaegaabaaaabaaaaaaeghobaaaaaaaaaaa
aagabaaaaaaaaaaadcaaaaamhccabaaaaaaaaaaaegacbaaaabaaaaaaaceaaaaa
piinkplmpiinkplmpiinkplmaaaaaaaaegacbaaaaaaaaaaadgaaaaaficcabaaa
aaaaaaaaabeaaaaaaaaaiadpdoaaaaabejfdeheofaaaaaaaacaaaaaaaiaaaaaa
diaaaaaaaaaaaaaaabaaaaaaadaaaaaaaaaaaaaaapaaaaaaeeaaaaaaaaaaaaaa
aaaaaaaaadaaaaaaabaaaaaaadadaaaafdfgfpfaepfdejfeejepeoaafeeffied
epepfceeaaklklklepfdeheocmaaaaaaabaaaaaaaiaaaaaacaaaaaaaaaaaaaaa
aaaaaaaaadaaaaaaaaaaaaaaapaaaaaafdfgfpfegbhcghgfheaaklkl""
}
SubProgram ""gles3 "" {
""!!GLES3""
}
SubProgram ""metal "" {
// Stats: 33 math, 9 textures
SetTexture 0 [_MainTex] 2D 0
ConstBuffer ""$Globals"" 8
Vector 0 [_ResampleOffset] 2
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
  float2 _ResampleOffset;
};
fragment xlatMtlShaderOutput xlatMtlMain (xlatMtlShaderInput _mtl_i [[stage_in]], constant xlatMtlShaderUniform& _mtl_u [[buffer(0)]]
  ,   texture2d<half> _MainTex [[texture(0)]], sampler _mtlsmp__MainTex [[sampler(0)]])
{
  xlatMtlShaderOutput _mtl_o;
  half4 col_1;
  float2 P_2;
  float2 cse_3;
  cse_3 = -(_mtl_u._ResampleOffset);
  P_2 = ((float2)_mtl_i.xlv_TEXCOORD0 + cse_3);
  float2 P_4;
  P_4 = ((float2)_mtl_i.xlv_TEXCOORD0 + _mtl_u._ResampleOffset);
  float2 P_5;
  P_5 = ((float2)_mtl_i.xlv_TEXCOORD0 + (2.0 * cse_3));
  float2 P_6;
  P_6 = ((float2)_mtl_i.xlv_TEXCOORD0 + (2.0 * _mtl_u._ResampleOffset));
  float2 P_7;
  P_7 = ((float2)_mtl_i.xlv_TEXCOORD0 + (3.0 * cse_3));
  float2 P_8;
  P_8 = ((float2)_mtl_i.xlv_TEXCOORD0 + (3.0 * _mtl_u._ResampleOffset));
  float2 P_9;
  P_9 = ((float2)_mtl_i.xlv_TEXCOORD0 + (4.0 * cse_3));
  float2 P_10;
  P_10 = ((float2)_mtl_i.xlv_TEXCOORD0 + (4.0 * _mtl_u._ResampleOffset));
  col_1.xyz = (((
    ((((
      ((_MainTex.sample(_mtlsmp__MainTex, (float2)(_mtl_i.xlv_TEXCOORD0)) * (half)0.38026) + (_MainTex.sample(_mtlsmp__MainTex, (float2)(P_2)) * (half)0.27667))
     + 
      (_MainTex.sample(_mtlsmp__MainTex, (float2)(P_4)) * (half)0.27667)
    ) + (_MainTex.sample(_mtlsmp__MainTex, (float2)(P_5)) * (half)0.08074)) + (_MainTex.sample(_mtlsmp__MainTex, (float2)(P_6)) * (half)0.08074)) + (_MainTex.sample(_mtlsmp__MainTex, (float2)(P_7)) * (half)-0.02612))
   + 
    (_MainTex.sample(_mtlsmp__MainTex, (float2)(P_8)) * (half)-0.02612)
  ) + (_MainTex.sample(_mtlsmp__MainTex, (float2)(P_9)) * (half)-0.02143)) + (_MainTex.sample(_mtlsmp__MainTex, (float2)(P_10)) * (half)-0.02143)).xyz;
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
