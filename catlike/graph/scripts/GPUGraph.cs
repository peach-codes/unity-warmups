using UnityEngine;

public class GPUGraph : MonoBehaviour
{
    const int maxResolution = 1000;
    [SerializeField, Range(10, maxResolution)]
    int resolution = 10; // default value, editable in editor

    [SerializeField]
    Material material;

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    ComputeShader computeShader;

    static readonly int positionsId = Shader.PropertyToID("_Positions");
    static readonly int resolutionId = Shader.PropertyToID("_Resolution");
    static readonly int stepId = Shader.PropertyToID("_Step");
    static readonly int timeId = Shader.PropertyToID("_Time");
    static readonly int transitionProgressId = Shader.PropertyToID("_TransitionProgress");

    [SerializeField]
    Function_Library.FunctionName function; // we're currently using this from the other function libary...

    public enum TransitionMode {Cycle, Random}

    [SerializeField]
    TransitionMode transitionMode = TransitionMode.Cycle;

    [SerializeField, Min(0f)]
    float functionDuration = 1f, transitionDuration = 1f;

    float duration;

    bool transitioning;

    Function_Library.FunctionName transitionFunction; // function we are transitioning _from_

    ComputeBuffer positionsBuffer;

    void UpdateFunctionOnGPU () {
        float step = (2.0f / resolution); // something is wrong?
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);

    if (transitioning) {
			computeShader.SetFloat(
				transitionProgressId,
				Mathf.SmoothStep(0f, 1f, duration / transitionDuration)
			);
		}

        var kernelIndex = (int)function + (int)(transitioning ? transitionFunction : function) * Function_Library.FunctionCount;
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);
        
        int groups = Mathf.CeilToInt(resolution / 8f); // how many groups of 8x8 data?
        computeShader.Dispatch(kernelIndex, groups, groups, 1);
        
        material.SetBuffer(positionsId, positionsBuffer); // updated position
        material.SetFloat(stepId, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * ( 2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }

    void OnEnable() {
        positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4); // each element is 3 * 4 bytes (float)
    }

    void OnDisable() {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    void PickNextFunction() { // weird style
        function = transitionMode == TransitionMode.Cycle ?
            Function_Library.GetNextFunctionName(function) :
            Function_Library.GetRandomFunctionNameOtherThan(function);
    }

    void Update() {
        duration += Time.deltaTime;
        if (transitioning) {
            if (duration >= transitionDuration) { // catch end of transition
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration) {
            duration -= functionDuration;
            transitioning = true;
            transitionFunction = function; // remember, this is the old one
            PickNextFunction();
        }
        UpdateFunctionOnGPU();
    }


}
