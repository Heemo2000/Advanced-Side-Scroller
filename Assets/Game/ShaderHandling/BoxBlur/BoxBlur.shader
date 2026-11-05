Shader "Custom/BoxBlur2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Blur ("Blur strength (filter size (2n+1)^2)", Integer) = 1
        _Scale ("Scale (texel offset)", Range(1,5)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos   : TEXCOORD0;
                float2 uv          : TEXCOORD1;
                float4 color       : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_CameraSortingLayerTexture);
            SAMPLER(sampler_CameraSortingLayerTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                int _Blur;
                float _Scale;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 pos = IN.screenPos.xy / IN.screenPos.w;
                float2 texel = _Scale / _ScreenParams.xy;
                int blurSize = max(_Blur, 0);

                float3 sum = 0;
                [loop]
                for (int i = -blurSize; i <= blurSize; i++)
                {
                    [loop]
                    for (int j = -blurSize; j <= blurSize; j++)
                    {
                        sum += SAMPLE_TEXTURE2D(_CameraSortingLayerTexture,
                                                sampler_CameraSortingLayerTexture,
                                                pos + float2(i, j) * texel).rgb;
                    }
                }
                sum /= (2 * blurSize + 1) * (2 * blurSize + 1);

                float spriteA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).a;
                float3 col = lerp(sum, sum * IN.color.rgb, IN.color.a);
                return float4(col, spriteA);
            }
            ENDHLSL
        }
    }
}