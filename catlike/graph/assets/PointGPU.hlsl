#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float3> _Positions;
	float _Step; // this is too small when in the shader graph URP
	// i don't understand why this is broken with the shader graph for URP
	// if I set a manual step value, it works totally fine, but also....

	void ConfigureProcedural () {
			float3 position = _Positions[unity_InstanceID];

			unity_ObjectToWorld = 0.0;
			unity_ObjectToWorld._m03_m13_m23_m33 = float4(position, 1.0); // posistion
			unity_ObjectToWorld._m00_m11_m22 = _Step; // right fucking here //scaling
	}

#endif

void ShaderGraphFunction_float (float3 In, out float3 Out) {
    Out = In;
}

void ShaderGraphFunction_half (half3 In, out half3 Out) {
    Out = In;
}