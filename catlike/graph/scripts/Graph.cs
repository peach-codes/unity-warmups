using UnityEngine;

public class Graph : MonoBehaviour
{

    [SerializeField]
    Transform pointPrefab;

    [SerializeField, Range(10, 200)]
    int resolution = 10; // default value, editable in editor

    [SerializeField]
    Function_Library.FunctionName function;

    public enum TransitionMode {Cycle, Random}

    [SerializeField]
    TransitionMode transitionMode;

    [SerializeField, Min(0f)]
    float functionDuration = 1f, transitionDuration = 1f;

    Transform[] points;

    float duration;

   bool transitioning;

   Function_Library.FunctionName transitionFunction; // function we are transitioning _from_

    void Awake() {
        float step = 2f / resolution; // [-1,1], resolution points
        var scale = Vector3.one * step;
        points = new Transform[resolution * resolution];
        for(int i = 0; i < points.Length; i++) {
            Transform point = points[i] = Instantiate(pointPrefab); // cool
            point.localScale = scale;
            point.SetParent(transform, false); // not entierly sure, something about world position relative to the parent
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
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

        if (transitioning) {
            UpdateFunctionTransition();
        }
        else {
            UpdateFunction();
        }
    }

    // random note, c# does not have an exponential operator :(
    void UpdateFunction() {
        Function_Library.Function f = Function_Library.GetFunction(function);
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step -1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
            if (x == resolution) {
                x = 0; // reset x
                z += 1; // increment z
                v = (z + 0.5f) * step - 1f; // uh, help me, math brain...
            }
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = f(u, v, time);
        }
    }

    void UpdateFunctionTransition() {
        Function_Library.Function from = Function_Library.GetFunction(transitionFunction);
        Function_Library.Function to = Function_Library.GetFunction(function);
        float progress = duration / transitionDuration;
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step -1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++) {
            if (x == resolution) {
                x = 0; // reset x
                z += 1; // increment z
                v = (z + 0.5f) * step - 1f; // uh, help me, math brain...
            }
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = Function_Library.Morph(u, v, time, from, to, progress);
        }
    }

}


