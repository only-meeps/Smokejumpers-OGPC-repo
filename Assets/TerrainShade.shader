Shader "Custom/TerrainShade" {
    Properties {
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        int baseColourCount;
        fixed4 baseColours[8];
        float baseStartHeights[8];

        float minHeight;
        float maxHeight;

        struct Input {
            float3 worldPos;
        };

        float inverseLerp(float a, float b, float value) {
            return saturate((value - a) / (b - a));
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            o.Albedo = baseColours[0].rgb; // Set initial color to the first color
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);

            for (int i = 0; i < baseColourCount; i++) {
                if (heightPercent <= baseStartHeights[i]) {
                    o.Albedo = baseColours[i].rgb;
                }
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}