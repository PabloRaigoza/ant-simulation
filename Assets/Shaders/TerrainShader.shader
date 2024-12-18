Shader "Custom/TerrainShader"
{
    Properties
    {
        _textureScale("Texture scale", float) = 1
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        #define MAX_TEXTURES 32

        float _textureScale;
        float minTerrainRadius;
        float maxTerrainRadius;

        float terrainRadiuses[MAX_TEXTURES];
        UNITY_DECLARE_TEX2DARRAY(terrainTextures);

        int numTextures;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 scaledWorldPos = IN.worldPos / _textureScale;
            float worldPosX = IN.worldPos.x;
            float worldPosY = IN.worldPos.y;
            float worldPosZ = IN.worldPos.z;
            float radius = sqrt(worldPosX * worldPosX + worldPosY * worldPosY + worldPosZ * worldPosZ);
            
            // nomalize y-coordinate to between 0 and 1
            float radiusValue = saturate((radius - minTerrainRadius) / (maxTerrainRadius - minTerrainRadius));
            
            // determine the layer index that this height corresponds to 
            int layerIndex = -1;
            for (int i = 0; i < numTextures - 1; i++) 
            {   
                if (radiusValue >= terrainRadiuses[i] && radiusValue < terrainRadiuses[i + 1]) 
                {
                    layerIndex = i;
                    break;
                }
            }
            
            if (layerIndex == -1) {layerIndex = numTextures - 1;}

            // compute lat and long
            float lat = acos(worldPosY / radius);
            float lon = atan2(worldPosZ, worldPosX);

            // normalize lat and long to between 0 and 1
            float latValue = saturate(lat / 3.14159);
            float lonValue = saturate((lon + 3.14159) / (2 * 3.14159));

            // get the texture value
            o.Albedo = UNITY_SAMPLE_TEX2DARRAY(terrainTextures, float3(lonValue, latValue, layerIndex));
            // o.Albedo = float4(0, 0, 1, 1);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
