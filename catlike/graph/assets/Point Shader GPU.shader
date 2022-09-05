Shader "Custom/Point Shader GPU"
{
    Properties {
        _Smoothness("Smoothness", Range(0,1)) = 0.5
    }

    SubShader {
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        #pragma editor_sync_compilation
        #pragma target 4.5
        #include "PointGPU.hlsl" // includes ConfigureProcedural, _Step

         // coloring per vertex, not per obj center
        struct Input { // keyword
            float3 worldPos; // shader equivalent to Vector#
        }; // dont forget :)

        float _Smoothness;

        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
            surface.Smoothness = _Smoothness;
            surface.Albedo = saturate(input.worldPos * 0.5 + 0.5);
        }

        ENDCG
    }
    
    FallBack "Diffuse"
}
