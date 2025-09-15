Shader "Custom/ToonSaturatedNoOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Ramp ("Ramp Texture", 2D) = "white" {}
        _RampSteps ("Ramp Steps", Range(1, 10)) = 3
        _Saturation ("Saturation", Range(0, 3)) = 1.5
        _EdgeDarkness ("Edge Darkness", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100

        // Main pass only - outline pass has been removed
        Pass
        {
            Name "MAIN"
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                SHADOW_COORDS(3)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            sampler2D _Ramp;
            float _RampSteps;
            float _Saturation;
            float _EdgeDarkness;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                TRANSFER_SHADOW(o);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Basic lighting
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = dot(i.worldNormal, lightDir);
                
                // Sample shadow
                fixed shadow = SHADOW_ATTENUATION(i);
                
                // Convert to stepped toon lighting
                float toon = floor(NdotL * _RampSteps) / _RampSteps;
                toon = max(0, toon) * shadow;
                
                // Apply ramp texture if needed
                fixed4 rampCol = tex2D(_Ramp, float2(toon, 0.5));
                
                // Sample texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Calculate edge darkening based on view direction
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float VdotN = dot(viewDir, i.worldNormal);
                float edge = 1 - saturate(VdotN);
                edge = pow(edge, 2); // Adjust falloff
                
                // Apply edge darkening
                col.rgb *= lerp(1.0, 1.0 - _EdgeDarkness, edge);
                
                // Apply toon lighting with saturation
                float3 saturatedColor = lerp(col.rgb, col.rgb * (1.0 + _Saturation), toon);
                col.rgb = saturatedColor * rampCol.rgb * _LightColor0.rgb;
                
                // Add ambient light
                col.rgb += col.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb;
                
                return col;
            }
            ENDCG
        }
        
        // Shadow pass
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    
    FallBack "Diffuse"
}