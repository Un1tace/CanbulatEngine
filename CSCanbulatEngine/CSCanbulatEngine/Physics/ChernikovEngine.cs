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

                var aAabb = a.GetAabb();
                var bAabb = b.GetAabb();

                if (Intersects(a, b))
                {
                    if (a.isTrigger || b.isTrigger) continue;

                    var rbA = a.AttachedGameObject.GetComponent<Rigidbody>();
                    var rbB = b.AttachedGameObject.GetComponent<Rigidbody>();

                    bool aDynamic = rbA != null && rbA.IsSimulated;
                    bool bDynamic = rbB != null && rbB.IsSimulated;
            
                    if (aDynamic && !bDynamic)
                    {
                        var bT = b.AttachedGameObject.GetComponent<Transform>();
                        float platformRight = bT.WorldPosition.X + (b.Size.X * bT.WorldScale.X * 0.5f);
                        float platformLeft = bT.WorldPosition.X - (b.Size.X * bT.WorldScale.X * 0.5f);
                
                        Vector2 com = a.GetWorldCentreOfMass();
                        bool isSupported = (com.X < platformRight && com.X > platformLeft);
                        
                        ResolvePenetration(a, b);
                
                        ApplyTippingTorque(a, b, deltaTime);
                    }
                    else if (!aDynamic && bDynamic)
                    {
                        var aT = a.AttachedGameObject.GetComponent<Transform>();
                        float platformRight = aT.WorldPosition.X + (a.Size.X * aT.WorldScale.X * 0.5f);
                        float platformLeft = aT.WorldPosition.X - (a.Size.X * aT.WorldScale.X * 0.5f);
                
                        Vector2 com = b.GetWorldCentreOfMass();
                        bool isSupported = (com.X < platformRight && com.X > platformLeft);
                        
                        ResolvePenetration(b, a);
                
                        ApplyTippingTorque(b, a, deltaTime);
                    }
                    else
                    {
                        ResolvePenetration(a, b);
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
    private static void ResolvePenetration(BoxCollider a, BoxCollider b)
    {
        var rbA = a.AttachedGameObject.GetComponent<Rigidbody>();
        var aT = a.AttachedGameObject.GetComponent<Transform>();
        var bAabb = b.GetAabb();
        
        Vector2 half = (a.Size * aT.WorldScale) * 0.5f;
        Vector2[] corners = { new(-half.X, -half.Y), new(half.X, -half.Y), new(half.X, half.Y), new(-half.X, half.Y) };
        float cos = MathF.Cos(aT.WorldRotation);
        float sin = MathF.Sin(aT.WorldRotation);

        float maxOverlapY = 0, maxOverlapX = 0;
        float lowestY = float.MaxValue;
        
        foreach (var c in corners) {
            Vector2 world = new Vector2(c.X * cos - c.Y * sin, c.X * sin + c.Y * cos) + (aT.WorldPosition + a.Offset);
            if (world.Y < lowestY) lowestY = world.Y;
            
            if (world.X > bAabb.Min.X && world.X < bAabb.Max.X) {
                float depth = bAabb.Max.Y - world.Y;
                if (depth > maxOverlapY) maxOverlapY = depth;
            }
        }

        foreach (var c in corners) {
            Vector2 world = new Vector2(c.X * cos - c.Y * sin, c.X * sin + c.Y * cos) + (aT.WorldPosition + a.Offset);
        
            
            float ox = MathF.Min(world.X - bAabb.Min.X, bAabb.Max.X - world.X);
            float oy = MathF.Min(world.Y - bAabb.Min.Y, bAabb.Max.Y - world.Y);
    
            if (world.X > bAabb.Min.X && world.X < bAabb.Max.X && world.Y > bAabb.Min.Y && world.Y < bAabb.Max.Y) {
                if (oy > maxOverlapY) maxOverlapY = oy;
                if (ox > maxOverlapX) maxOverlapX = ox;
            }
        }
        
        bool isOnTop = lowestY > bAabb.Max.Y - 0.5f; 

        if (maxOverlapY > 0 && isOnTop)
        {
            float percent = 0.8f; 
            float slop = 0.01f;
            float correction = MathF.Max(maxOverlapY - slop, 0.0f) * percent;
        
            aT.WorldPosition += new Vector2(0, correction);
            
            if (rbA != null && rbA.Velocity.Y < 0) {
                rbA.Velocity.Y *= 0.1f;
            }
        }
        else if (maxOverlapX > 0) 
        {
            float dir = aT.WorldPosition.X < b.AttachedGameObject.GetComponent<Transform>().WorldPosition.X ? -1f : 1f;
            aT.WorldPosition += new Vector2(dir * maxOverlapX, 0);
            if (rbA != null) rbA.Velocity.X = 0;
        }
    }
    
    private static void ResolveVertical(BoxCollider falling, BoxCollider support, Rigidbody? rbFalling)
    {
        if (falling == null || support == null) return;

        var fAabb = falling.GetAabb();
        var sAabb = support.GetAabb();
        
        float overlapY = sAabb.Max.Y - fAabb.Min.Y;
        if (overlapY <= 0f) return;

        var fT = falling.AttachedGameObject.GetComponent<Transform>();
        var sT = support.AttachedGameObject.GetComponent<Transform>();
    Vector2 half = (falling.Size * fT.WorldScale) * 0.5f;
    Vector2[] localCorners =
    {
        new(-half.X, -half.Y),
        new( half.X, -half.Y),
        new( half.X,  half.Y),
        new(-half.X,  half.Y)
    };

    float cos = MathF.Cos(fT.WorldRotation);
    float sin = MathF.Sin(fT.WorldRotation);

    float lowestY = float.MaxValue;
    Vector2 lowestCorner = Vector2.Zero;

        foreach (var c in localCorners)
        {
            Vector2 world = new(
                c.X * cos - c.Y * sin,
                c.X * sin + c.Y * cos
            );
            world += fT.WorldPosition + falling.Offset;

            if (world.Y < lowestY)
            {
                lowestY = world.Y;
                lowestCorner = world;
            }
        }
        
        float platformHalfWidth = support.Size.X * sT.WorldScale.X * 0.5f;
        float platformLeft = sT.WorldPosition.X - platformHalfWidth;
        float platformRight = sT.WorldPosition.X + platformHalfWidth;

        bool cornerSupported = lowestCorner.X >= platformLeft && lowestCorner.X <= platformRight;
        if (!cornerSupported) return; 
        
    float correctionY = sAabb.Max.Y - lowestY;
        if (correctionY > 0f)
        {
            fT.WorldPosition += new Vector2(0, correctionY);
            
            if (rbFalling != null && MathF.Abs(MathF.Sin(fT.WorldRotation)) < 0.3f)
                rbFalling.Velocity.Y = 0;
        }
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
        
        if (aAabb.Min.X > bAabb.Max.X || aAabb.Max.X < bAabb.Min.X || 
            aAabb.Min.Y > bAabb.Max.Y || aAabb.Max.Y < bAabb.Min.Y) return false;
        
        return  IsAnyCornerInside(a, b) || IsAnyCornerInside(b, a);
    }

    private static bool IsAnyCornerInside(BoxCollider a, BoxCollider b)
    {
        var bAabb = b.GetAabb();
        var t = a.AttachedGameObject.GetComponent<Transform>();
        Vector2 worldScale = t.WorldScale;
        float rotation = t.WorldRotation;

        Vector2 half = (a.Size * worldScale) * 0.5f;
        
        Vector2[] localCorners = {
            new Vector2(-half.X, -half.Y), new Vector2(half.X, -half.Y),
            new Vector2(half.X, half.Y), new Vector2(-half.X, half.Y)
        };
        
        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        foreach (var corner in localCorners)
        {
            Vector2 worldCorner = new Vector2(corner.X * cos - corner.Y * sin, corner.X * sin + corner.Y * cos) + (t.WorldPosition + a.Offset);

            if (worldCorner.X >= bAabb.Min.X && worldCorner.X <= bAabb.Max.X && worldCorner.Y >= bAabb.Min.Y &&
                worldCorner.Y <= bAabb.Max.Y)
            {
                return true;
            }
        }
        return false;
    }

    static void ApplyTippingTorque(BoxCollider box, BoxCollider support, float dt)
    {
        var rb = box.AttachedGameObject.GetComponent<Rigidbody>();
        if (rb == null || !rb.IsSimulated) return;

        Vector2 com = box.GetWorldCentreOfMass();
        var sAabb = support.GetAabb();
        var sT = support.AttachedGameObject.GetComponent<Transform>();
        
        float halfWidth = (support.Size.X * sT.WorldScale.X) * 0.5f;
        float edgeLeft = sT.WorldPosition.X - halfWidth;
        float edgeRight = sT.WorldPosition.X + halfWidth;
        float edgeTop = sAabb.Max.Y;

        Vector2 pivot;
        
        if (com.X > edgeRight) {
            pivot = new Vector2(edgeRight, edgeTop);
        }
        else if (com.X < edgeLeft) {
            pivot = new Vector2(edgeLeft, edgeTop);
        }
        else {
            pivot = new Vector2(com.X, edgeTop);
        }
        
        Vector2 r = com - pivot;
        
        Vector2 F = rb.Mass * ProjectSettings.Gravity;
        float torque = (r.X * F.Y) - (r.Y * F.X);
    
        rb.AngularVelocity += (torque / rb.Inertia) * dt;
        
        if (com.X > edgeRight || com.X < edgeLeft) {
            float tangentialVelocityX = -rb.AngularVelocity * r.Y;
            rb.Velocity.X = Single.Lerp(rb.Velocity.X, tangentialVelocityX, 10f * dt);
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