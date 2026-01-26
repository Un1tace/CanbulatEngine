using System.Numerics;
using CSCanbulatEngine.GameObjectScripts;
using CSCanbulatEngine.UIHelperScripts;
using ImGuiNET;
using Silk.NET.Maths;

namespace CSCanbulatEngine.Utilities;

#if EDITOR
public enum GizmoAxis {None, X, Y, XY, Z}
public enum GizmoFunction {Position, Rotation, Scale}

/// <summary>
/// Visual handles for changing position, scale and rotation
/// </summary>
public class Gizmo
{
    private GizmoAxis _hoveredAxis = GizmoAxis.None;
    private GizmoAxis _draggedAxis = GizmoAxis.None;
    private static GizmoFunction _selectedFunction = GizmoFunction.Position;

    private Vector2 _dragStartMousePos;
    private Vector2 _dragStartObjectPos;
    private Vector2 _dragStartObjectSize;
    private float _dragStartObjectRotation;
    private float _dragStartAngle = 0f;

    private static float lastFrameAngle = 0f;

    private float rotCircleRadius = 100f;
    
    float handleSize = 5f; 

    public void UpdateAndRender(GameObject selectedObject, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix,
        Vector2 viewportPos, Vector2 viewportSize)
    {
        if (selectedObject == null) return;

        var drawList = ImGui.GetWindowDrawList();
        var io = ImGui.GetIO();

        //Turning the objects world position into the screen position in the viewport
        Vector2 objectScreenPos = WorldToScreen(selectedObject.GetComponent<Transform>().WorldPosition, viewMatrix,
            projectionMatrix, viewportPos, viewportSize);
        float rotation = selectedObject.GetComponent<Transform>().WorldRotation;
        
        float cos = MathF.Cos(-rotation);
        float sin = MathF.Sin(-rotation);

        var localXAxis = new Vector2(cos, sin);
        var localYAxis = new Vector2(sin, -cos);
        
        float gizmoSize = 50f; // Length of the gizmo arms in pixel

        Vector2 screenXAxis = new Vector2(cos, sin);
        Vector2 screenYAxis = new Vector2(sin, -cos);

        Vector2 xAxisEnd;
        Vector2 yAxisEnd;
        if (_selectedFunction == GizmoFunction.Scale)
        {
            xAxisEnd = objectScreenPos + screenXAxis * gizmoSize;
            yAxisEnd = objectScreenPos + screenYAxis * gizmoSize;
        }
        else
        {
            xAxisEnd = objectScreenPos + new Vector2(gizmoSize, 0);
            yAxisEnd = objectScreenPos - new Vector2(0, gizmoSize);
        }
        
        Vector2[] xyAxisVertices =
        [
            objectScreenPos - screenXAxis * handleSize - screenYAxis * handleSize,
            objectScreenPos + screenXAxis * handleSize - screenYAxis * handleSize,
            objectScreenPos + screenXAxis * handleSize + screenYAxis * handleSize,
            objectScreenPos - screenXAxis * handleSize + screenYAxis * handleSize
        ];

        //Check for hovering if not dragging anything
        if (_draggedAxis == GizmoAxis.None) _hoveredAxis = CheckHover(io.MousePos, objectScreenPos, xAxisEnd, yAxisEnd);

        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _hoveredAxis != GizmoAxis.None)
        {
            _draggedAxis = _hoveredAxis;
            _dragStartMousePos = io.MousePos;
            _dragStartObjectPos = selectedObject.GetComponent<Transform>().WorldPosition;
            _dragStartObjectSize = selectedObject.GetComponent<Transform>().WorldScale;
            _dragStartObjectRotation = selectedObject.GetComponent<Transform>().WorldRotation;

            if (_selectedFunction == GizmoFunction.Rotation)
            {
                Vector2 startVector = io.MousePos - objectScreenPos;
                _dragStartAngle = MathF.Atan2(startVector.Y, startVector.X);
            }
        }
        else if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            _draggedAxis = GizmoAxis.None;
        }

        if (ImGui.IsMouseDragging(ImGuiMouseButton.Left) && _draggedAxis != GizmoAxis.None)
        {
            //Handle Drag
            Vector2 mouseDelta = io.MousePos - _dragStartMousePos;

            Vector2 worldDelta = ScreenToWorldDelta(mouseDelta, projectionMatrix, viewportSize);

            if (_selectedFunction == GizmoFunction.Position)
            {
                if (_draggedAxis == GizmoAxis.XY)
                {
                    selectedObject.GetComponent<Transform>().WorldPosition =
                        new Vector2(_dragStartObjectPos.X + worldDelta.X, _dragStartObjectPos.Y + worldDelta.Y);
                }
                else if (_draggedAxis == GizmoAxis.Y)
                {
                    selectedObject.GetComponent<Transform>().WorldPosition =
                        new Vector2(_dragStartObjectPos.X, _dragStartObjectPos.Y + worldDelta.Y);
                }
                else if (_draggedAxis == GizmoAxis.X)
                {
                    selectedObject.GetComponent<Transform>().WorldPosition =
                        new Vector2(_dragStartObjectPos.X + worldDelta.X, _dragStartObjectPos.Y);
                }
            }
            else if (_selectedFunction == GizmoFunction.Scale)
            {
                if (_draggedAxis == GizmoAxis.XY)
                {
                    Vector2 newScale = new Vector2(_dragStartObjectSize.X + worldDelta.X, _dragStartObjectSize.Y + worldDelta.Y);

                    if (selectedObject.GetComponent<Transform>().ratioLocked)
                    {
                        if (newScale.X > newScale.Y)
                        {
                            Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>().ImageResolution;
                            newScale.Y = newScale.X * ((float)resolution.Y / (float)resolution.X);
                        }
                        else
                        {
                            Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>().ImageResolution;
                            newScale.X = newScale.Y * ((float)resolution.X / (float)resolution.Y);
                        }
                    }

                    selectedObject.GetComponent<Transform>().WorldScale = newScale;
                }
                else if (_draggedAxis == GizmoAxis.Y)
                {
                    float scaleAmount = Vector2.Dot(worldDelta, localYAxis);
                    Vector2 newScale = new Vector2(_dragStartObjectSize.X, _dragStartObjectSize.Y + scaleAmount);
                    
                        
                    if (selectedObject.GetComponent<Transform>().ratioLocked)
                    {
                        Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>().ImageResolution;
                        newScale.X = newScale.Y * ((float)resolution.X / (float)resolution.Y);
                    }

                    selectedObject.GetComponent<Transform>().WorldScale = newScale;
                }
                else if (_draggedAxis == GizmoAxis.X)
                {
                    float scaleAmount = Vector2.Dot(worldDelta, localXAxis);
                    Vector2 newScale = new Vector2(_dragStartObjectSize.X + scaleAmount, _dragStartObjectSize.Y);
                    
                    if (selectedObject.GetComponent<Transform>().ratioLocked)
                    {
                        Vector2D<int> resolution = Engine._selectedGameObject.gameObject.GetComponent<MeshRenderer>().ImageResolution;
                        newScale.Y = newScale.X * ((float)resolution.Y / (float)resolution.X);
                    }
                    
                    selectedObject.GetComponent<Transform>().WorldScale = newScale;

                }
            }
            else if (_selectedFunction == GizmoFunction.Rotation)
            {
                Vector2 currentVec = io.MousePos - objectScreenPos;
                
                float currentAngle = MathF.Atan2(currentVec.Y, currentVec.X);
                
                float deltaAngle = currentAngle - _dragStartAngle;
                
                selectedObject.GetComponent<Transform>().WorldRotation = _dragStartObjectRotation - deltaAngle;
                
                
                // selectedObject.GetComponent<Transform>().Rotation =
                //     _dragStartObjectRotation + CalcDistanceOnTangent(objectScreenPos, rotCircleRadius, _dragStartMousePos, worldDelta);
                // if (_draggedAxis == GizmoAxis.Z)
                // {
                //     
                // }
            }
        }

        //Drawing logic
        uint xAxisColor = GetAxisColor(GizmoAxis.X);
        uint yAxisColor = GetAxisColor(GizmoAxis.Y);
        uint xyAxisColor = GetAxisColor(GizmoAxis.XY);
        uint zAxisColor = GetAxisColor(GizmoAxis.Z);
        float thickness = 2.0f;

        if (_selectedFunction == GizmoFunction.Position || _selectedFunction == GizmoFunction.Scale)
        {
            drawList.AddLine(objectScreenPos, xAxisEnd, xAxisColor, thickness);
            drawList.AddLine(objectScreenPos, yAxisEnd, yAxisColor, thickness);
            
            if (_selectedFunction == GizmoFunction.Position)
            {
                drawList.AddTriangleFilled(xAxisEnd, xAxisEnd + new Vector2(-10, -5), xAxisEnd + new Vector2(-10, 5),
                    xAxisColor);
                drawList.AddTriangleFilled(yAxisEnd, yAxisEnd + new Vector2(-5, 10), yAxisEnd + new Vector2(5, 10), yAxisColor);
            }
            else if (_selectedFunction == GizmoFunction.Scale)
            {
                
                // X-axis handle
                drawList.AddQuadFilled(
                    xAxisEnd - screenXAxis * handleSize - screenYAxis * handleSize,
                    xAxisEnd + screenXAxis * handleSize - screenYAxis * handleSize,
                    xAxisEnd + screenXAxis * handleSize + screenYAxis * handleSize,
                    xAxisEnd - screenXAxis * handleSize + screenYAxis * handleSize,
                    xAxisColor);
                    
                // Y-axis handle
                drawList.AddQuadFilled(
                    yAxisEnd - screenXAxis * handleSize - screenYAxis * handleSize,
                    yAxisEnd + screenXAxis * handleSize - screenYAxis * handleSize,
                    yAxisEnd + screenXAxis * handleSize + screenYAxis * handleSize,
                    yAxisEnd - screenXAxis * handleSize + screenYAxis * handleSize,
                    yAxisColor);
            }
            
            //Draw Pivot
            drawList.AddQuadFilled(xyAxisVertices[0], xyAxisVertices[1], xyAxisVertices[2], xyAxisVertices[3],
                xyAxisColor);
        }
        else if (_selectedFunction == GizmoFunction.Rotation)
        {
            drawList.AddCircle(objectScreenPos, rotCircleRadius, zAxisColor, 30, thickness);

            // if (_draggedAxis == GizmoAxis.Z)
            // {
            //     Vector4 posDir = CalcTangentLine(objectScreenPos, rotCircleRadius, _dragStartMousePos);
            //
            //     // drawList.AddLine((new Vector2(posDir.Z, posDir.W)) + new Vector2(posDir.X, posDir.Y), (new Vector2(posDir.Z, posDir.W)) + new Vector2(posDir.X, posDir.Y), zAxisColor, thickness);
            //     drawList.AddCircleFilled(new Vector2(posDir.X + objectScreenPos.X, posDir.Y + objectScreenPos.Y), 5f,
            //         zAxisColor);
            //     drawList.AddLine((new Vector2(posDir.Z, -posDir.W) * 1000) + new Vector2(posDir.X + objectScreenPos.X, posDir.Y + objectScreenPos.Y), (new Vector2(posDir.Z, -posDir.W) * -1000) + new Vector2(posDir.X + objectScreenPos.X, posDir.Y + objectScreenPos.Y),
            //         zAxisColor, thickness);
            // }
        }
    }

    private uint GetAxisColor(GizmoAxis axis)
    {
        bool isHovered = _hoveredAxis == axis;
        bool isDragged =  _draggedAxis == axis;

        if (isDragged || isHovered) return ImGui.GetColorU32(new Vector4(1.0f, 1.0f, 0.0f, 1.0f)); // Yellow
        if (axis == GizmoAxis.X) return ImGui.GetColorU32(new Vector4(1.0f, 0.0f, 0.0f, 1.0f)); // Red
        if (axis == GizmoAxis.Y) return ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.0f, 1.0f)); // Green
        if (axis == GizmoAxis.XY) return ImGui.GetColorU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f)); // White
        if (axis == GizmoAxis.Z) return ImGui.GetColorU32(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
        return 0;
    }

    private GizmoAxis CheckHover(Vector2 mousePos, Vector2 origin, Vector2 xAxisEnd, Vector2 yAxisEnd)
    {
        float handleRadius = 8f; //Defining gow close mouse needs to be to axis

        if (DistanceToLineSegment(mousePos, origin, origin) < handleRadius) return GizmoAxis.XY;
        if (DistanceToLineSegment(mousePos, origin, xAxisEnd) < handleRadius) return GizmoAxis.X;
        if (DistanceToLineSegment(mousePos, origin, yAxisEnd) < handleRadius) return GizmoAxis.Y;
        if (DistanceToCircleSegment(mousePos, origin, rotCircleRadius) < handleRadius) return GizmoAxis.Z;
        
        return GizmoAxis.None;
    }

    private static Vector2 WorldToScreen(Vector2 worldPos, Matrix4x4 view, Matrix4x4 proj, Vector2 viewportPos,
        Vector2 viewportSize)
    {
        var clipPos = Vector4.Transform(new Vector4(worldPos.X, worldPos.Y, 0, 1.0f), view * proj);
        if (clipPos.W == 0) return new Vector2(-1, 1);

        var ndc = new Vector3(clipPos.X, clipPos.Y, clipPos.Z) / clipPos.W;

        float screenX = viewportPos.X + (ndc.X * 0.5f + 0.5f) * viewportSize.X;
        float screenY = viewportPos.Y + (1.0f - (ndc.Y * 0.5f + 0.5f)) * viewportSize.Y;
        
        return new Vector2(screenX, screenY);
    }

    private static Vector2 ScreenToWorldDelta(Vector2 screenDelta, Matrix4x4 projection, Vector2 viewportSize)
    {
        float worldWidth = 2.0f / projection.M11;
        float worldHeight = 2.0f / projection.M22;

        float worldDeltaX = (screenDelta.X / viewportSize.X) * worldWidth;
        float worldDeltaY = (-screenDelta.Y / viewportSize.Y) * worldHeight; // Y is inverted
        
        return new Vector2(worldDeltaX, worldDeltaY);
    }

    private static float DistanceToLineSegment(Vector2 pos, Vector2 start, Vector2 end)
    {
        float l2 = (end - start).LengthSquared();
        if (l2 == 0) return Vector2.Distance(pos, start);
        float t = Math.Max(0, MathF.Min(1, Vector2.Dot(pos - start, end - start) / l2));
        Vector2 projection = start + t * (end - start);
        return Vector2.Distance(pos, projection);
    }

    private static float DistanceToCircleSegment(Vector2 pos, Vector2 centre, float radius)
    {
        Vector2 closestPart = (Vector2.Normalize(pos - centre) * radius) + centre;
        float mouseDistance = Vector2.Distance(closestPart, pos);
        
        return mouseDistance;
    }
    
    private static Vector4 CalcTangentLine(Vector2 circleCentre, float circleRadius, Vector2 posStarted)
    {
        Vector2 closestPart = (Vector2.Normalize(posStarted - circleCentre) * circleRadius);
        float angle = MathF.Atan2(closestPart.Y, closestPart.X); // Finds angle from positive X axis
        
        float cos = MathF.Cos(angle); // Y axis value
        float sin = MathF.Sin(angle); // X axis Value
        
        return new Vector4(closestPart.X, closestPart.Y, sin, cos);
    }
    
    public static Vector2 RotateRadians(Vector2 v, float radians)
    {
        var ca = MathF.Cos(radians);
        var sa = MathF.Sin(radians);
        return new Vector2(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y);
    }

    public static void RenderToolbar()
    {

        float buttonSize = ImGui.GetContentRegionAvail().X - 5f;

        bool twoButtons = buttonSize >= 50f;

        if (twoButtons) buttonSize = (buttonSize / 2f) - 2.5f;;
        
        if (ImGui.ImageButton("Move", (IntPtr)LoadIcons.icons["Move.png"],
                new Vector2(buttonSize)))
        {
            _selectedFunction = GizmoFunction.Position;
        }

        if (twoButtons) ImGui.SameLine();
        
        if (ImGui.ImageButton("Scale", (IntPtr)LoadIcons.icons["Scale.png"],
                new Vector2(buttonSize)))
        {
            _selectedFunction = GizmoFunction.Scale;   
        }
        if (ImGui.ImageButton("Rotate", (IntPtr)LoadIcons.icons["Rotate.png"],
                new Vector2(buttonSize)))
        {
            _selectedFunction = GizmoFunction.Rotation;
        }
        
        Engine.RenderToolbar();
    }
}
#endif