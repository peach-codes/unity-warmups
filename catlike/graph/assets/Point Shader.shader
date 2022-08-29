Shader "Graph/Point surface" {
    // it appears vs code does not supoort help here as much :(

    Properties {
        // interface with the editor, set default
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }

        // written in a mix of CF and HLSL, shader languages?
    SubShader {
        CGPROGRAM
         // compiler directives
        #pragma surface ConfigureSurface Standard fullforwardshadows
        #pragma target 3.0 // minimum target level and quality

        // coloring per vertex, not per obj center
        struct Input { // keyword
            float3 worldPos; // shader equivalent to Vector#
        }; // dont forget :)

        float _Smoothness;
        // more strange specific syntax, inout is cool though
        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
            surface.Smoothness = _Smoothness; // make it configurable in the editor
            //surface.Albedo = input.worldPos * 0.5 + 0.5; // XYZ [-1,1] -> RGB [0,?]
            surface.Albedo = saturate(input.worldPos * 0.5 + 0.5); // Z is ~0, saturate clamps outputs to [0,1]
        }

        ENDCG
    }

    Fallback "Diffuse"
}
