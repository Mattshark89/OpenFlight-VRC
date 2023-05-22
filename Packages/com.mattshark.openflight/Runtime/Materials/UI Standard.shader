 Shader "Colorize"
 {
     Properties
     {
         _MainTex("_MainTex", 2D) = "white" {}      // Note _MainTex is a special name: This can also be accessed from C# via mainTexture property. 
     }
         SubShader
         {
             Pass
             {
             Name "ColorizeSubshader"
 
             // ---
             // For Alpha transparency:   https://docs.unity3d.com/462/Documentation/Manual/SL-SubshaderTags.html
             Tags
             {
                 "Queue" = "Transparent"
                 "RenderType" = "Transparent"
             }
             Blend SrcAlpha OneMinusSrcAlpha
             //AlphaToMask On
             ZWrite On
             // ---
 
             CGPROGRAM
             #pragma vertex   MyVertexShaderFunction 
             #pragma fragment  MyFragmentShaderFunction
             #pragma fragmentoption ARB_precision_hint_fastest 
             #include "UnityCG.cginc"
 
             sampler2D _MainTex;
 
             //fixed4 _Color0;
 
             // http://wiki.unity3d.com/index.php/Shader_Code : 
             // There are some pre-defined structs e.g.: v2f_img, appdata_base, appdata_tan, appdata_full, v2f_vertex_lit
             //
             // but if you want to create a custom struct, then the see Acceptable Field types and names at http://wiki.unity3d.com/index.php/Shader_Code 
             // my custom struct recieving data from unity
             struct my_needed_data_from_unity
             {
                 float4 vertex   : POSITION;  // The vertex position in model space.          //  Name&type must be the same!
                 float4 texcoord : TEXCOORD0; // The first UV coordinate.                     //  Name&type must be the same!
                 float4 color    : COLOR;     //    The color value of this vertex specifically. //  Name&type must be the same!
             };
 
             // my custom Vertex to Fragment struct
             struct my_v2f
             {
                 float4  pos : SV_POSITION;
                 float2  uv : TEXCOORD0;
                 float4  color : COLOR;
             };
 
             my_v2f  MyVertexShaderFunction(my_needed_data_from_unity  v)
             {
                 my_v2f  result;
                 result.pos = UnityObjectToClipPos(v.vertex);  // Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
                 result.uv = v.texcoord.xy;
                 result.color = v.color;
                 return result;
             }
             
             float4 MyFragmentShaderFunction(my_v2f  i) : COLOR
             {
                 half4 texcolor = tex2D(_MainTex, i.uv) * i.color; // texture's pixel color
                 return texcolor;
             }
 
             ENDCG
         }
     }
     //Fallback "Diffuse"
 }
