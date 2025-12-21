using System.Numerics;
using CSCanbulatEngine.GameObjectScripts;

namespace CSCanbulatEngine.Physics;

/// <summary>
/// The physics engine for the Canbulat Engine
/// </summary>
public static class ChernikovEngine
{
    public static Vector2 Gravity => ProjectSettings.Gravity;

    private static readonly List<Rigidbody> _rigidbodies = new();

    /// <summary>
    /// Register a rigidbody to the ChernikovEngine
    /// </summary>
    /// <param name="rb">Rigidbody</param>
    public static void Register(Rigidbody rb)
    {
        if (!_rigidbodies.Contains(rb)) _rigidbodies.Add(rb);
    }

    /// <summary>
    /// Unregister a rigidbody from the ChernikovEngine
    /// </summary>
    /// <param name="rb">Rigidbody</param>
    public static void Unregister(Rigidbody rb)
    {
        _rigidbodies.Remove(rb);
    }

    /// <summary>
    /// Called from Engine.OnUpdate while in Play mode
    /// </summary>
    /// <param name="deltaTime">Time between this and last frame</param>
    public static void Step(float deltaTime)
    {
        if (deltaTime <= 0f) return;

        foreach (var rb in _rigidbodies)
        {
            if (rb == null || !rb.isEnabled || !rb.IsSimulated) continue;

            rb.Integrate(deltaTime);
        }
    }
    
    public static void ResetRigidbodyValues()
    {
        foreach (var rb in _rigidbodies)
        {
            rb.Velocity = Vector2.Zero;
        }
    }
}