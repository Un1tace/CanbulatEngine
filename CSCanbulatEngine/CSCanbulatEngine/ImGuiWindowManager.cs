using System.Numerics;
using ImGuiNET;

namespace CSCanbulatEngine;

#if EDITOR
// Entire script manages how the windows are placed in the application
public class ImGuiWindowManager
{
    //Values for engine.cs to take
    public static List<Vector2> windowPosition = new List<Vector2>();
    public static List<Vector2> windowSize = new List<Vector2>();
    
    //Percentages
    public static List<Vector2> windowPosPercentage =
        new List<Vector2>();
    public static List<Vector2> windowSizPercentage = new List<Vector2>();

    public static float menuBarHeight;

    public static void InitialiseDefaults()
    {
        windowPosPercentage.Clear();
        windowSizPercentage.Clear();
        windowPosition.Clear();
        windowSize.Clear();
        
        //Viewport - Index 0
        windowPosPercentage.Add(new Vector2(0.25f, 0));
        windowSizPercentage.Add(new Vector2(0.5f, 0.6f));
        windowPosition.Add(new Vector2(0, 0));
        windowSize.Add(new Vector2(0, 0));
        
        CalculatePositionFromPercentage(0);
        CalculateSizeFromPercentage(0);
        //Inspector - Index 1
        windowPosPercentage.Add(new Vector2(0.75f, 0));
        windowSizPercentage.Add(new Vector2(0.25f, 1));
        windowPosition.Add(new Vector2(0, 0));
        windowSize.Add(new Vector2(0, 0));
        
        CalculatePositionFromPercentage(1);
        CalculateSizeFromPercentage(1);
        
        //Hierarchy - Index 2
        windowPosPercentage.Add(new Vector2(0f, 0));
        windowSizPercentage.Add(new Vector2(0.25f, 0.6f));
        windowPosition.Add(new Vector2(0, 0));
        windowSize.Add(new Vector2(0, 0));
        
        CalculatePositionFromPercentage(2);
        CalculateSizeFromPercentage(2);
        
        //Project File Manager - Index 3
        windowPosPercentage.Add(new Vector2(0f, 0.6f));
        windowSizPercentage.Add(new Vector2(0.75f, 0.4f));
        windowPosition.Add(new Vector2(0, 0));
        windowSize.Add(new Vector2(0, 0));
        
        CalculatePositionFromPercentage(3);
        CalculateSizeFromPercentage(3);
    }

    public static void CalculatePositionFromPercentage(int index)
    {
        var displaySize = ImGui.GetIO().DisplaySize;
        displaySize = new Vector2(displaySize.X, displaySize.Y - menuBarHeight);
        
        float percentageX = windowPosPercentage[index].X;
        float percentageY = windowPosPercentage[index].Y;

        windowPosition[index] = new Vector2(percentageX * displaySize.X, (percentageY * displaySize.Y)+menuBarHeight);
    }

    public static void CalculateSizeFromPercentage(int index)
    {
        var displaySize = ImGui.GetIO().DisplaySize;
        displaySize = new Vector2(displaySize.X, displaySize.Y - menuBarHeight);
        
        float percentageX = windowSizPercentage[index].X;
        float percentageY = windowSizPercentage[index].Y;
        
        windowSize[index] = new Vector2(percentageX * displaySize.X, (percentageY * displaySize.Y));
    }

    public static void CalcViewportHeightPercentage()
    {
        var viewportSizePercentage = windowSizPercentage[0];
        windowSizPercentage[0] = new Vector2(viewportSizePercentage.X, (viewportSizePercentage.X/16)*9);
    }

    public static void OnFrameBufferResize()
    {
        InitialiseDefaults();
    }
}
#endif