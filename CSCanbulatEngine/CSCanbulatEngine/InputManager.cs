using System.Net.Http.Headers;
using ImGuiNET;

namespace CSCanbulatEngine;

using Silk.NET.Input;
using System.Collections.Generic;

public class InputManager
{
    private static IKeyboard _primaryKeyboard;
    
    // Hashsets are fast, dont keep order and random stuff, just efficient for removing, adding and checking elements
    private static HashSet<Key> _keysDown = new HashSet<Key>();
    private static HashSet<Key> _keysPressedThisFrame = new HashSet<Key>();
    private static HashSet<Key> _keysReleasedThisFrame = new HashSet<Key>();

    public static void Initialize(IKeyboard keyboard)
    {
        _primaryKeyboard = keyboard;
        _primaryKeyboard.KeyDown += OnKeyDown;
        _primaryKeyboard.KeyUp += OnKeyUp;
    }

    //!!!Function needs to be at end of update for refreshing pressed and released!!!//
    public static void LateUpdate()
    {
        _keysPressedThisFrame.Clear();
        _keysReleasedThisFrame.Clear();
    }

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

        _keysDown.Add(key);
    }
    
    private static void OnKeyUp(IKeyboard keyboard, Key key, int arg3)
    {
        _keysDown.Remove(key);
        _keysReleasedThisFrame.Add(key);
    }
}