using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using CSCanbulatEngine.GameObjectScripts;

namespace CSCanbulatEngine.Physics;

/// <summary>
/// The physics engine for the Canbulat Engine
/// </summary>
public static class ChernikovEngine
{
    public static Vector2 Gravity => ProjectSettings.Gravity;

    private static readonly List<Rigidbody> _rigidbodies = new();

    private const int SubSteps = 4;

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

        float stepDelta = deltaTime / SubSteps;

        for (int s = 0; s < SubSteps; s++)
        {
            foreach (var rb in _rigidbodies)
            {
                if (rb == null || !rb.isEnabled || !rb.IsSimulated) continue;

                rb.Integrate(deltaTime);
            }

            var colliders = new List<BoxCollider>();
            foreach (var go in Engine.currentScene.GameObjects)
            {
                BoxCollider? collider = go.GetComponent<BoxCollider>();
                if (collider != null && collider.isEnabled) colliders.Add(collider);
            }

            for (int i = 0; i < colliders.Count; i++)
            {
                for (int j = i + 1; j < colliders.Count; j++)
                {
                    BoxCollider a = colliders[i];
                    BoxCollider b = colliders[j];

                    if (Intersects(a, b))
                    {
                        if (a.isTrigger || b.isTrigger) continue;

                        var rbA = a.AttachedGameObject.GetComponent<Rigidbody>();
                        var rbB = b.AttachedGameObject.GetComponent<Rigidbody>();

                        bool aDynamic = rbA != null && rbA.IsSimulated;
                        bool bDynamic = rbB != null && rbB.IsSimulated;

                        if (aDynamic && !bDynamic)
                        {
                            ResolvePenetration(a, b, stepDelta);
                        }
                        else if (!aDynamic && bDynamic)
                        {
                            ResolvePenetration(b, a, stepDelta);
                        }
                        else if (aDynamic && bDynamic)
                        {
                            ResolvePenetration(a, b, stepDelta);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Resolves a penetration of two box colliders
    /// </summary>
    /// <param name="a">Collider 1</param>
    /// <param name="b">Collider 2</param>
    private static void ResolvePenetration(BoxCollider dynamicCol, BoxCollider staticCol, float dt)
    {
        var rb = dynamicCol.AttachedGameObject.GetComponent<Rigidbody>();
        var tA = dynamicCol.AttachedGameObject.GetComponent<Transform>();
        
        Vector2 bestCorrection = Vector2.Zero;
        float maxDepth = 0f;
        Vector2 contactPoint = Vector2.Zero;

        // --- PASS 1: Check if Dynamic Corners are inside Static Box ---
        var bAabb = staticCol.GetAabb();
        Vector2[] cornersA = GetRotatedCorners(dynamicCol);

        foreach (var p in cornersA)
        {
            if (p.X > bAabb.Min.X && p.X < bAabb.Max.X && 
                p.Y > bAabb.Min.Y && p.Y < bAabb.Max.Y)
            {
                float left = p.X - bAabb.Min.X;
                float right = bAabb.Max.X - p.X;
                float top = bAabb.Max.Y - p.Y;
                float bottom = p.Y - bAabb.Min.Y;

                float minX = MathF.Min(left, right);
                float minY = MathF.Min(top, bottom);
                
                if (minY < minX) 
                {
                    // Vertical push
                    float sign = (top < bottom) ? 1f : -1f; 
                    if (minY > maxDepth) { 
                        maxDepth = minY; 
                        bestCorrection = new Vector2(0, sign * minY); 
                        contactPoint = p; // Store contact for torque
                    }
                }
                else
                {
                    // Horizontal push
                    float sign = (right < left) ? 1f : -1f;
                    if (minX > maxDepth) { 
                        maxDepth = minX; 
                        bestCorrection = new Vector2(sign * minX, 0); 
                        contactPoint = p;
                    }
                }
            }
        }

        // --- PASS 2: Check if Static Corners are inside Dynamic Box ---
        Vector2 halfA = (dynamicCol.Size * tA.WorldScale) * 0.5f;
        Vector2[] cornersB = GetRotatedCorners(staticCol); 
        
        foreach (var p in cornersB)
        {
            Vector2 localP = RotatePoint(p - (tA.WorldPosition + dynamicCol.Offset), -tA.WorldRotation);
            
            if (MathF.Abs(localP.X) < halfA.X && MathF.Abs(localP.Y) < halfA.Y)
            {
                float left = localP.X - (-halfA.X);
                float right = halfA.X - localP.X;
                float bottom = localP.Y - (-halfA.Y);
                float top = halfA.Y - localP.Y;
                
                float minX = MathF.Min(left, right);
                float minY = MathF.Min(top, bottom);
                
                Vector2 localCorrection;
                if (minY < minX)
                {
                    float sign = (top < bottom) ? -1f : 1f; 
                    localCorrection = new Vector2(0, sign * minY);
                }
                else
                {
                    float sign = (right < left) ? -1f : 1f;
                    localCorrection = new Vector2(sign * minX, 0);
                }
                float depth = MathF.Min(minX, minY);
                if (depth > maxDepth)
                {
                    maxDepth = depth;
                    bestCorrection = RotatePoint(localCorrection, tA.WorldRotation);
                    contactPoint = p; 
                }
            }
        }

        // Apply Position Correction
        if (maxDepth > 0)
        {
            tA.WorldPosition += bestCorrection;

            if (rb != null)
            {
                // 1. Stop linear velocity on collision axis (Inelastic)
                if (MathF.Abs(bestCorrection.Y) > MathF.Abs(bestCorrection.X)) rb.Velocity.Y = 0;
                else rb.Velocity.X = 0;

                // 2. Check if supported on platform before applying torque
                // Get platform bounds
                var sT = staticCol.AttachedGameObject.GetComponent<Transform>();
                float halfWidth = (staticCol.Size.X * sT.WorldScale.X) * 0.5f;
                float platformLeft = sT.WorldPosition.X - halfWidth;
                float platformRight = sT.WorldPosition.X + halfWidth;
                
                // Get object COM
                float objectX = tA.WorldPosition.X;

                // Only apply torque if the center of mass is safely WITHIN the platform width.
                // If it's hanging over the edge, do NOT rotate (just let it sit flat/stable).
                bool isSafelyOnPlatform = objectX > platformLeft && objectX < platformRight;

                if (isSafelyOnPlatform)
                {
                    ApplySimpleTorque(rb, tA, contactPoint, dt);
                }
                else
                {
                    // If over the edge, kill angular velocity to stabilize it
                    rb.AngularVelocity *= 0.5f;
                }
            }
        }
    }

    private static void ApplySimpleTorque(Rigidbody rb, Transform t, Vector2 contactPoint, float dt)
    {
        float distanceX = contactPoint.X - t.WorldPosition.X;
        
        if (MathF.Abs(distanceX) < 0.1f) return;
        
        float supportForce = rb.Mass * 20f; 
        
        float torque = distanceX * supportForce;
        
        rb.AngularVelocity += (torque / rb.Inertia) * dt;
        
        rb.AngularVelocity *= 0.95f; 
    }

    private static Vector2 RotatePoint(Vector2 point, float angle)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        return new Vector2(point.X * cos - point.Y * sin, point.X * sin + point.Y * cos);
    }
    
    public static Vector2[] GetRotatedCorners(BoxCollider box)
    {
        var t = box.AttachedGameObject.GetComponent<Transform>();
        Vector2 half = (box.Size * t.WorldScale) * 0.5f;
        Vector2[] corners =
        {
            new(-half.X, -half.Y), new(half.X, -half.Y),
            new(-half.X, half.Y), new(-half.X, half.Y)
        };
        
        float cos = MathF.Cos(t.WorldRotation);
        float sin = MathF.Sin(t.WorldRotation);

        Vector2 centre = t.WorldPosition + box.Offset;

        for (int i = 0; i < 4; i++)
        {
            corners[i] = new Vector2(corners[i].X * cos - corners[i].Y * sin, corners[i].X * sin + corners[i].Y * cos) + centre;
        }

        return corners;
    }

    /// <summary>
    /// Checks if two AABBs intersect with each other
    /// </summary>
    /// <param name="a">First AABB</param>
    /// <param name="b">Second AABB</param>
    /// <returns>If it intersects</returns>
    static bool Intersects(BoxCollider a, BoxCollider b)
    {
        var aAabb = a.GetAabb();
        var bAabb = b.GetAabb();
        if (aAabb.Min.X > bAabb.Max.X || aAabb.Max.X < bAabb.Min.X || aAabb.Min.Y > bAabb.Max.Y || aAabb.Max.Y < bAabb.Min.Y) return false;
        return IsAnyCornerInside(a, b) || IsAnyCornerInside(b, a);
    }
    
    private static bool IsAnyCornerInside(BoxCollider a, BoxCollider b)
    {
        var bAabb = b.GetAabb();
        var corners = GetRotatedCorners(a);
        foreach (var p in corners)
        {
            if (p.X >= bAabb.Min.X && p.X <= bAabb.Max.X && 
                p.Y >= bAabb.Min.Y && p.Y <= bAabb.Max.Y) return true;
        }
        return false;
    }
    
    public static void ResetRigidbodyValues()
    {
        foreach (var rb in _rigidbodies)
        {
            rb.Velocity = Vector2.Zero;
        }
    }
}