// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "OpenFlight/Particles"
{
    Properties {
        _MainTex ("Particle Texture", 2D) = "white" {}
    }

    Category {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Blend SrcAlpha One
        Lighting Off
        ColorMask RGB
        Cull Off ZWrite Off

        SubShader {
            Pass {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #pragma multi_compile_particles
                #pragma multi_compile_fog
                #pragma target 3.0

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                struct VertexInput {
                    float4 vertex : POSITION;       //local vertex position
                    float2 texcoord0 : TEXCOORD0;   //uv coordinates
                    fixed4 color : COLOR; 
                };
                
                struct VertexOutput {
                    float4 vertex : SV_POSITION;              //screen clip space position and depth
                    float2 uv0 : TEXCOORD0;                //uv coordinates
                    float4 color : TEXCOORD2;
                    UNITY_FOG_COORDS(3)                    //this initializes the unity fog
                };
                
                VertexOutput vert (VertexInput v) {
                    VertexOutput o = (VertexOutput)0;           
                    o.uv0 = v.texcoord0;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.color = v.color;
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }
                
                fixed4 frag (VertexOutput i) : SV_Target
                {
                    
                    fixed4 diffuseColor = i.color * tex2D(_MainTex, i.uv0);
                    UNITY_APPLY_FOG_COLOR(i.fogCoord, diffuseColor, fixed4(0,0,0,0)); // fog towards black due to our blend mode
                    return diffuseColor;
                }
                ENDCG 
            }
        }
    }
}
