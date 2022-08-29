using UnityEngine;
using TMPro;

public class FrameRateCounter : MonoBehaviour // has to be the same as the file name?
{
    [SerializeField]
    TextMeshProUGUI display;

    [SerializeField, Range(0.1f, 2f)]
    float sampleDuration = 1f;

    public enum DisplayMode {FPS, MS}

    [SerializeField]
    DisplayMode displayMode = DisplayMode.FPS;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    int frames;
    float duration, bestDuration = float.MaxValue, worstDuration;

    // Update is called once per frame
    void Update() {
        float frameDuration = Time.unscaledDeltaTime;
        frames += 1;
        duration += frameDuration;

        if (frameDuration < bestDuration) {
            bestDuration = frameDuration;
        }
        if (frameDuration > worstDuration) {
            worstDuration = frameDuration;
        }

        if (duration >= sampleDuration) {
            if (displayMode == DisplayMode.FPS) {
                display.SetText(
                    "FPS\n{0:1}\n{1:1}\n{2:1}",
                    1f / bestDuration,
                    frames / duration,
                    1f / worstDuration
                );
            }
            else {
                display.SetText(
                    "MS\n{0:1}\n{1:1}\n{2:1}",
                    1000f * bestDuration,
                    1000f * duration / frames,
                    1000f * worstDuration
                );
            }
            frames = 0;
            duration = 0f;
            bestDuration = float.MaxValue;
            worstDuration = 0f;
        }
    }
}
