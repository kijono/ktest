Shader "KTest/CSMat"
{
    Properties
    {
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
		_MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        #pragma instancing_options procedural:ConfigureProcedural
        #pragma target 4.5

        struct Input{
            float3 worldPos;
			float2 uv_MainTex;
        };

        half _Smoothness;
        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        StructuredBuffer<float4x4> _mats;
        #endif

		sampler2D _MainTex;
        void ConfigureProcedural()
        {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            unity_ObjectToWorld = _mats[unity_InstanceID];
            unity_WorldToObject = transpose(_mats[unity_InstanceID]);
            #endif
        }
		
        void ConfigureSurface (Input input, inout SurfaceOutputStandard surface)
        {
            surface.Albedo = saturate(input.worldPos * 0.5 + 0.5) * 0.2 + tex2D(_MainTex, input.uv_MainTex);
            surface.Smoothness = _Smoothness;
        }
        
        ENDCG
    }
    FallBack "Diffuse"
}