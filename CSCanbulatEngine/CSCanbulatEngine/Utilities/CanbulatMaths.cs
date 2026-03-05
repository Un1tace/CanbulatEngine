using Azure.Core;

namespace CSCanbulatEngine.Utilities;

public class PerlinNoise
{
    private readonly int[] _permutation = new int[512];
    public int seed = 0;

    /// <summary>
    /// Initialises a new Perlin Noise generator with a specific seed
    /// </summary>
    /// <param name="seed"></param>
    public PerlinNoise(int seed = 0)
    {
        this.seed = seed;
        Random rand = new Random(seed);

        int[] p = new int[256];

        for (int i = 0; i < 256; i++)
        {
            p[i] = i;
        }

        for (int i = 255; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (p[i], p[j]) = (p[j], p[i]);
        }

        for (int i = 0; i < 256; i++)
        {
            _permutation[i] = p[i];
            _permutation[256 + i] = p[i];
        }
    }

    public float Noise1D(float x)
    {
        int X = (int)MathF.Floor(x) & 255;
        
        x -= MathF.Floor(x);

        float u = Fade(x);
        
        int A = _permutation[X];
        int B = _permutation[X + 1];
        
        return Lerp(u, Grad1D(_permutation[A], x), Grad1D(_permutation[B], x - 1));
    }

    public float Noise2D(float x, float y)
    {
        // Find unit grid cell containing point
        int X = (int)MathF.Floor(x) & 255;
        int Y = (int)MathF.Floor(y) & 255;
        
        // Get relative xy coordinates of point within that cell
        x -= MathF.Floor(x);
        y -= MathF.Floor(y);

        // Compute fade curves x and y
        float u = Fade(x);
        float v = Fade(y);
        
        // Hash coordinates of the 4 square corners 
        int A = _permutation[X] + Y;
        int AA = _permutation[A];
        int AB = _permutation[A + 1];
        
        int B = _permutation[X + 1] + Y;
        int BA = _permutation[B];
        int BB = _permutation[B + 1];
        
        //blend
        float res = Lerp(v,
            Lerp(u, Grad2D(_permutation[AA], x, y), Grad2D(_permutation[BA], x - 1, y)),
            Lerp(u, Grad2D(_permutation[AB], x, y - 1), Grad2D(_permutation[BB], x - 1, y - 1)));
        
        return res;
    }
    
    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10);
    }

    private static float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    private static float Grad1D(int hash, float x)
    {
        return (hash & 1) == 0 ? x : -x;
    }

    private static float Grad2D(int hash, float x, float y)
    {
        int h = hash & 15; // Convert to low 4 bits of hash code
        float u = h < 8 ? x : y;
        float v = h < 4 ? y : (h == 12 || h == 14 ? x : 0f);
        
        return ((h & 1) == 0? u : -u) + ((h & 2) == 0? v : -v);
    }
}