using System.Net.Http.Headers;
using System.Numerics;
using CSCanbulatEngine.Circuits;
using ImGuiNET;

namespace CSCanbulatEngine;

using Silk.NET.Input;
using System.Collections.Generic;

/// <summary>
/// Manages and executes information for inputs from default keyboard and mouse
/// </summary>
public class InputManager
{
    private static IKeyboard _primaryKeyboard;
    private static IMouse _primaryMouse;
    
    // Hashsets are fast, dont keep order and random stuff, just efficient for removing, adding and checking elements
    private static HashSet<Key> _keysDown = new HashSet<Key>();
    private static HashSet<Key> _keysPressedThisFrame = new HashSet<Key>();
    private static HashSet<Key> _keysReleasedThisFrame = new HashSet<Key>();

    private static HashSet<MouseButton> _mouseButtonsDown = new();
    private static HashSet<MouseButton> _mouseButtonsPressedThisFrame = new();
    private static HashSet<MouseButton> _mouseButtonsReleasedThisFrame = new();

    public static void InitializeKeyboard(IKeyboard keyboard)
    {
        //Keyboard
        _primaryKeyboard = keyboard;
        _primaryKeyboard.KeyDown += OnKeyDown;
        _primaryKeyboard.KeyUp += OnKeyUp;
    }

    public static void InitializeMouse(IMouse mouse)
    {
        //Mouse
        _primaryMouse = mouse;
        _primaryMouse.MouseDown += OnMouseDown;
        _primaryMouse.MouseUp += OnMouseUp;
        _primaryMouse.Scroll += OnMouseScroll;
        _primaryMouse.DoubleClick += OnMouseDoubleClick;
        _primaryMouse.Click += OnMouseClick;   
    }

    //!!!Function needs to be at end of update for refreshing pressed and released!!!//
    public static void LateUpdate()
    {
        _keysPressedThisFrame.Clear();
        _keysReleasedThisFrame.Clear();
        
        _mouseButtonsReleasedThisFrame.Clear();
        _mouseButtonsPressedThisFrame.Clear();
    }
    
    //Keyboard
    public static bool IsKeyDown(Key key)
    {
        return _keysDown.Contains(key);
    }

    public static bool IsKeyPressed(Key key)
    {
        return _keysPressedThisFrame.Contains(key);
    }

    public static bool IsKeyReleased(Key key)
    {
        return _keysReleasedThisFrame.Contains(key);
    }

    private static void OnKeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        if (!_keysDown.Contains(key))
        {
            _keysPressedThisFrame.Add(key);
        }
        
        #if EDITOR
        bool viewportFocused = Engine._isViewportFocused;
        #elif GAME
        bool viewportFocused = true;
        #endif

        if (viewportFocused)
        {
            var keyEvent = EventManager.RegisteredEvents.Find(e => e.EventName == "OnKeyPressed");
            EventValues eventValues = new EventValues();
            eventValues.Keys["Key"] = key;
            EventManager.Trigger(keyEvent, eventValues);
        }

        _keysDown.Add(key);
    }
    
    private static void OnKeyUp(IKeyboard keyboard, Key key, int arg3)
    {
        _keysDown.Remove(key);
        _keysReleasedThisFrame.Add(key);
        
#if EDITOR
        bool viewportFocused = Engine._isViewportFocused;
#elif GAME
        bool viewportFocused = true;
#endif

        if (viewportFocused)
        {
            var keyEvent = EventManager.RegisteredEvents.Find(e => e.EventName == "OnKeyReleased");
            EventValues eventValues = new EventValues();
            eventValues.Keys["Key"] = key;
            EventManager.Trigger(keyEvent, eventValues);
        }
    }
    
    // Mouse Items

    public static bool IsMouseButtonDown(MouseButton button)
    {
        return _mouseButtonsDown.Contains(button);
    }

    public static bool IsMouseButtonPressed(MouseButton button)
    {
        return _mouseButtonsPressedThisFrame.Contains(button);
    }

    public static bool IsMouseButtonReleased(MouseButton button)
    {
        return _mouseButtonsReleasedThisFrame.Contains(button);
    }

    private static void OnMouseDown(IMouse mouse, MouseButton button)
    {
        if (!_mouseButtonsDown.Contains(button))
        {
            _mouseButtonsPressedThisFrame.Add(button);
        }

        _mouseButtonsDown.Add(button);
        
#if EDITOR
        bool viewportFocused = Engine._isViewportFocused;
#elif GAME
        bool viewportFocused = true;
#endif

        if (viewportFocused)
        {
            var mouseEvent = EventManager.RegisteredEvents.Find(e => e.EventName == "OnMouseButtonPressed");
            EventValues eventValues = new EventValues();
            eventValues.MouseButtons["MouseButton"] = button;
            EventManager.Trigger(mouseEvent, eventValues);
        }
    }

    private static void OnMouseUp(IMouse mouse, MouseButton button)
    {
        _mouseButtonsReleasedThisFrame.Add(button);
        _mouseButtonsDown.Remove(button);
        
#if EDITOR
        bool viewportFocused = Engine._isViewportFocused;
#elif GAME
        bool viewportFocused = true;
#endif
        
        if (viewportFocused)
        {
            var mouseEvent = EventManager.RegisteredEvents.Find(e => e.EventName == "OnMouseButtonReleased");
            EventValues eventValues = new EventValues();
            eventValues.MouseButtons["MouseButton"] = button;
            EventManager.Trigger(mouseEvent, eventValues);
        }
    }

    private static void OnMouseScroll(IMouse mouse, ScrollWheel scrollWheel)
    {
        
#if EDITOR
        bool viewportFocused = Engine._isViewportFocused;
#elif GAME
        bool viewportFocused = true;
#endif
        
        if (viewportFocused)
        {
            var mouseEvent = EventManager.RegisteredEvents.Find(e => e.EventName == "OnMouseScrolled");
            EventManager.Trigger(mouseEvent, new());
        }
    }

    private static void OnMouseDoubleClick(IMouse mouse, MouseButton button, Vector2 pos)
    {
#if EDITOR
        bool viewportFocused = Engine._isViewportFocused;
#elif GAME
        bool viewportFocused = true;
#endif
        
        if (viewportFocused)
        {
            var mouseEvent = EventManager.RegisteredEvents.Find(e => e.EventName == "OnMouseButtonDoubleClicked");
            var eventValues = new EventValues();
            eventValues.Vector2s["Position"] = pos;
            EventManager.Trigger(mouseEvent, eventValues);
        }
    }

    private static void OnMouseClick(IMouse mouse, MouseButton button, Vector2 pos)
    {
#if EDITOR
        bool viewportFocused = Engine._isViewportFocused;
#elif GAME
        bool viewportFocused = true;
#endif
        
        if (viewportFocused)
        {
            var mouseEvent = EventManager.RegisteredEvents.Find(e => e.EventName == "OnMouseButtonClicked");
            var eventValues = new EventValues();
            eventValues.Vector2s["Position"] = pos;
            EventManager.Trigger(mouseEvent, eventValues);
        }
    }


    private static List<Key>? allKeys = null;
    public static Key[] GetAllKeys()
    {
        if (allKeys == null)
        {
            allKeys = (List<Key>)Enum.GetValues(typeof(Key)).Cast<Key>().ToList();

            int x = 0;
            while (true)
            {

                if (allKeys.Count(e => e == allKeys[x]) > 1)
                {
                    allKeys.RemoveAt(x);
                }
                else x++;

                if (x >= allKeys.Count())
                {
                    break;
                }
            }
        }

        return allKeys.ToArray();
    }
    
    private static List<MouseButton>? allMouseButtons = null;
    public static MouseButton[] GetAllMouseButtons()
    {
        if (allMouseButtons == null)
        {
            allMouseButtons = (List<MouseButton>)Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().ToList();

            int x = 0;
            while (true)
            {

                if (allMouseButtons.Count(e => e == allMouseButtons[x]) > 1)
                {
                    allMouseButtons.RemoveAt(x);
                }
                else x++;

                if (x >= allMouseButtons.Count())
                {
                    break;
                }
            }
        }

        return allMouseButtons.ToArray();
    }
}