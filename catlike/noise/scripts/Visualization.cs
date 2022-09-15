using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public abstract class Visualization : MonoBehaviour {
    static int configId = Shader.PropertyToID("_Config");
    static int positionsId = Shader.PropertyToID("_Positions");
    static int normalsId = Shader.PropertyToID("_Normals");

    public enum Shape { Plane, Sphere, Torus }

	static Shapes.ScheduleDelegate[] shapeJobs = {
		Shapes.Job<Shapes.Plane>.ScheduleParallel,
		Shapes.Job<Shapes.Sphere>.ScheduleParallel,
		Shapes.Job<Shapes.Torus>.ScheduleParallel
	};

    [SerializeField]
    Mesh instanceMesh;

    [SerializeField]
    Material material;

    [SerializeField, Range(4, 512)]
    int resolution = 16;

    [SerializeField, Range(-0.5f, 0.5f)]
    float displacement = 0f;

    [SerializeField]
    Shape shape;

    [SerializeField, Range(0.1f, 10f)]
    float instanceScale = 1f;

    NativeArray<float3x4> positions;
    NativeArray<float3x4> normals;

    ComputeBuffer positionsBuffer;
    ComputeBuffer normalsBuffer;

    MaterialPropertyBlock propertyBlock;

    protected abstract void EnableVisualization(
        int dataLength, MaterialPropertyBlock propertyBlock
    );
    
    protected abstract void DisableVisualization();
    
    protected abstract void UpdateVisualization (
		NativeArray<float3x4> positions, int resolution, JobHandle handle
	);

    bool isDirty;

    void OnEnable() {
        isDirty = true;
        int length = resolution * resolution;
        length /= 4 + (length & 1);
        positions = new NativeArray<float3x4>(length, Allocator.Persistent);
        normals = new NativeArray<float3x4>(length, Allocator.Persistent);
        positionsBuffer = new ComputeBuffer(length * 4, 3 * 4);
        normalsBuffer = new ComputeBuffer(length * 4, 3 * 4);

        propertyBlock ??= new MaterialPropertyBlock();
        EnableVisualization(length, propertyBlock);
        propertyBlock.SetBuffer(positionsId, positionsBuffer);
        propertyBlock.SetBuffer(normalsId, normalsBuffer);
        propertyBlock.SetVector(configId, new Vector4(resolution, instanceScale / resolution, displacement)); // store resolution & reciprocal
    }

    void onDisable() {
        positions.Dispose();
        normals.Dispose();
        positionsBuffer.Release();
        normalsBuffer.Release();
        positionsBuffer = null;
        normalsBuffer = null;
        DisableVisualization();
    }

    void OnValidate() {
        if (positionsBuffer != null && enabled) {
            onDisable();
            OnEnable();
        }
    }

    Bounds bounds;

    void Update() {
        if (isDirty || transform.hasChanged) {
            isDirty = false;
            transform.hasChanged = false;

            UpdateVisualization(
                positions, resolution,
                shapeJobs[(int)shape] (
                    positions, normals, resolution, transform.localToWorldMatrix, default
                )
            );

            positionsBuffer.SetData(positions.Reinterpret<float3>(3 * 4 * 4));
            normalsBuffer.SetData(normals.Reinterpret<float3>(3 * 4 * 4));

            bounds = new Bounds(
                transform.position,
                float3(2f * cmax(abs(transform.lossyScale)) + displacement)
            );
        }

        Graphics.DrawMeshInstancedProcedural(
            instanceMesh, 0, material, bounds, resolution * resolution, propertyBlock
        );
    }

}