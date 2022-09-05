using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

public class Fractal : MonoBehaviour
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobFor {

        public float scale;
        public float deltaTime;

        public NativeArray<FractalPart> parts;
        [ReadOnly]
        public NativeArray<FractalPart> parents;
        [WriteOnly]
        public NativeArray<float3x4> matrices;

        public void Execute(int i) { // iterator of for loop
            FractalPart parent = parents[i / 5]; // integer division
			FractalPart part = parts[i];
            part.spinAngle += part.spinVelocity * deltaTime;

            float3 upAxis = mul(mul(parent.worldRotation, part.rotation), up()); // away from parent
            float3 sagAxis = cross(up(), upAxis);
            float sagMagnitude = length(sagAxis);

            quaternion baseRotation;
            if (sagMagnitude > 0f) {
                sagAxis /= sagMagnitude;
                quaternion sagRotation = quaternion.AxisAngle(sagAxis, part.maxSagAngle * sagMagnitude);
                baseRotation = mul(sagRotation, parent.worldRotation);
            }
            else {
                baseRotation = parent.worldRotation;
            }

            part.worldRotation = mul( baseRotation, 
                mul( part.rotation, quaternion.RotateY(part.spinAngle)));
             part.worldPosition =
                parent.worldPosition +
                mul(part.worldRotation, float3(0f, 1.5f * scale, 0f));
            parts[i] = part;
            float3x3 r = float3x3(part.worldRotation) * scale; // building TRS matrix
            matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
        }
    }

    struct FractalPart {
        public float3 worldPosition;
        public quaternion rotation, worldRotation;
        public float maxSagAngle, spinAngle, spinVelocity;
    }

    NativeArray<FractalPart>[] parts;

    NativeArray<float3x4>[] matrices; // transform matrixes for all parts

    static readonly int colorAId = Shader.PropertyToID("_ColorA");
    static readonly int colorBId = Shader.PropertyToID("_ColorB");
    static readonly int matricesId = Shader.PropertyToID("_Matrices");
    static readonly int sequenceNumbersId = Shader.PropertyToID("_SequenceNumbers");

    static MaterialPropertyBlock propertyBlock;

    [SerializeField, Range(3,8)] // we subtract twice in Update()
    int depth = 4;
    // Start is called before the first frame update

    [SerializeField]
    Mesh mesh, leafMesh;

    [SerializeField]
    Material material;

    [SerializeField]
    Gradient gradientA, gradientB;

    [SerializeField]
    Color leafColorA, leafColorB;

    [SerializeField, Range(0f, 90f)]
    float maxSagAngleA = 15f, maxSagAngleB = 25f;

    [SerializeField, Range(0f, 90f)]
	float spinSpeedA = 20f, spinSpeedB = 25f;

    [SerializeField, Range(0f, 1f)]
    float reverseSpinChance = 0.25f;

    static quaternion[] rotations = {quaternion.identity, 
        quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
        quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
    };

    FractalPart CreatePart(int childIndex) => new FractalPart {
        rotation = rotations[childIndex],
        maxSagAngle = radians(Random.Range(maxSagAngleA, maxSagAngleB)),
        spinVelocity =
            (Random.value < reverseSpinChance ? -1f : 1f) *
            radians(Random.Range(spinSpeedA, spinSpeedB))
    };

    ComputeBuffer[] matricesBuffers;
    Vector4[] sequenceNumbers; // must be vector 4 to go to GPU

    void OnEnable() {
        parts = new NativeArray<FractalPart>[depth];
        matrices = new NativeArray<float3x4>[depth];
        matricesBuffers = new ComputeBuffer[depth];
        sequenceNumbers = new Vector4[depth];
        int stride = 12 * 4; // (3x4 matrix of floats), 12 * 4 bytes
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5) {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent); // each layer gets bigger
            matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);
            sequenceNumbers[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
        }

        parts[0][0] = CreatePart(0);
        for (int li = 1; li < parts.Length; li++) { // each level/layer
            NativeArray<FractalPart> levelParts = parts[li]; // copy this level
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5) { // each object ?
                for (int ci = 0; ci < 5; ci++) { // new children
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }

        propertyBlock ??= new MaterialPropertyBlock();
    }

    void OnDisable () {
        for (int i = 0; i < matricesBuffers.Length; i++) {
            matricesBuffers[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }
        parts = null;
        matrices = null;
        matricesBuffers = null;
        sequenceNumbers = null;
    }

    // gets called after a change is made to a component via the editor
    // reset the fractal
    void OnValidate() {
        if (parts != null && enabled) {
            OnDisable();
            OnEnable();
        }
    }

    void Update () {

        float deltaTime = Time.deltaTime;
        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += rootPart.spinVelocity * deltaTime;
        rootPart.worldRotation = mul( transform.rotation, mul( rootPart.rotation, quaternion.RotateY(rootPart.spinAngle)));
        rootPart.worldPosition = transform.position;
        parts[0][0] = rootPart;
        float objectScale = transform.lossyScale.x;
        float3x3 r = float3x3(rootPart.worldRotation) * objectScale; // building TRS matrix
        matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);

        float scale = objectScale;
        JobHandle jobHandle = default;
        for (int li = 1; li < parts.Length; li++) { // root note stays at the origin
            scale *= 0.5f;
            jobHandle = new UpdateFractalLevelJob {
                deltaTime = deltaTime,
                scale = scale,
                parents = parts[li -1],
                parts = parts[li],
                matrices = matrices[li]
            }.ScheduleParallel(parts[li].Length, 5, jobHandle); // create & schedule job and grab the handle
		}
        jobHandle.Complete();

        int leafIndex = matricesBuffers.Length - 1;
        var bounds = new Bounds(Vector3.zero, 3f * objectScale * Vector3.one);
        for (int i = 0; i < matricesBuffers.Length; i++) { // draw loop
            ComputeBuffer buffer = matricesBuffers[i];
            buffer.SetData(matrices[i]);
            Color colorA, colorB;
            Mesh instanceMesh;
            if (i == leafIndex ) {
                colorA = leafColorA;
                colorB = leafColorB;
                instanceMesh = leafMesh;
            }
            else { 
                float gradientInterpolater = i / (matricesBuffers.Length - 1f);
                colorA = gradientA.Evaluate(gradientInterpolater);
                colorB = gradientB.Evaluate(gradientInterpolater);
                instanceMesh = mesh;
            }
            propertyBlock.SetColor(colorAId, colorA);
            propertyBlock.SetColor(colorBId, colorB);
            propertyBlock.SetBuffer(matricesId, buffer); // forces the GPU to use the associated data with this draw
            propertyBlock.SetVector(sequenceNumbersId, sequenceNumbers[i]);
            Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, bounds, buffer.count, propertyBlock);
        }
    }
}
