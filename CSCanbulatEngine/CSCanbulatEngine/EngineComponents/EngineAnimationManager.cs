using System.Numerics;

namespace CSCanbulatEngine.EngineComponents;

public class EngineAnimationManager
{
    private long pulseAnimationStart, duration;
    private Vector4 colorStart, pulseColor;
    private bool pulseRunning = false;

    public void SetUpPulseAnimation(Vector4 colorStart, Vector4 pulseColor, long duration)
    {
        pulseAnimationStart = DateTime.Now.Ticks;
        this.colorStart = colorStart;
        this.pulseColor = pulseColor;
        this.duration = duration*10000;
        pulseRunning = true;
    }

    public Vector4? GetPulseAnimationColor()
    {
        if (!pulseRunning)
        {
            return null;
        }
        long time =  (DateTime.Now.Ticks) - pulseAnimationStart;
        if (time > duration)
        {
            pulseRunning = false;
            return null;
        }
        
        float percent = float.Clamp((float)time / duration, 0f, 1f);
        float percentInPi = percent * MathF.PI;
        
        return Vector4.Lerp(colorStart, pulseColor, MathF.Sin(percentInPi));
    }
}