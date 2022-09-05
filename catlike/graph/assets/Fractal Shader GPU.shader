Shader "Fractal/Fractal Surface GPU"
{

    SubShader {
        CGPROGRAM
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        #pragma editor_sync_compilation
        #pragma target 4.5
        #include "FractalGPU.hlsl" // includes ConfigureProcedural, _Step

         // coloring per vertex, not per obj center
        struct Input { // keyword
            float3 worldPos; // shader equivalent to Vector#
        }; // dont forget :)

        float _Smoothness;

        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
            surface.Smoothness = GetFractalColor().a;
            surface.Albedo = GetFractalColor().rgb;
        }

        ENDCG
    }
    
    FallBack "Diffuse"
}
