Shader "Hidden/TerrainEngine/Details/BillboardStatic" {
    Properties{
        _WavingTint("Fade Color", Color) = (.7, .6, .5, 0)
        _MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}
        _Cutoff("Cutoff", float) = 0.5
    }

        CGINCLUDE
#include "UnityCG.cginc"

            // Use half precision for improved performance on constrained hardware
            struct v2f {
            float4 pos : SV_POSITION;
            half4 color : COLOR;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        // Vertex function without wind effect
        v2f BillboardVert(inout appdata_full v) {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            // Transform vertex position but omit wind-based modifications
            o.color = v.color;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;

            return o;
        }

        ENDCG

            SubShader{
                Tags {
                    "Queue" = "Geometry+200"
                    "IgnoreProjector" = "True"
                    "RenderType" = "GrassBillboard"
                    "DisableBatching" = "True"
                }
                Cull Off
                LOD 200
                ColorMask RGB

                CGPROGRAM
                #pragma surface surf Lambert vertex:BillboardVert addshadow exclude_path:deferred

                sampler2D _MainTex;
                half _Cutoff;

                struct Input {
                    float2 uv_MainTex;
                    half4 color : COLOR;
                };

                // Simplified surf function without wind or complex effects
                void surf(Input IN, inout SurfaceOutput o) {
                    half4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;
                    o.Albedo = c.rgb;
                    o.Alpha = c.a;

                    // Simple transparency discard
                    if (o.Alpha < _Cutoff) discard;
                }

                ENDCG
        }

            Fallback Off
}
