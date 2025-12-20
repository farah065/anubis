Shader "Hidden/Custom/BitDepth"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            float _Depth;
            
            half4 Frag (Varyings i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);
                float levels = pow(2.0, _Depth);
                col.rgb = round(col.rgb * (levels - 1.0)) / (levels - 1.0);
                return col;
            }
            ENDHLSL
        }
    }
}