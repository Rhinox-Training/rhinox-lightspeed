#if ENABLE_INPUT_SYSTEM && UNITY_INPUTSYSTEM
#define USE_NEW_INPUTSYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace Rhinox.Lightspeed
{
    public static partial class Utility
    {
        private static Dictionary<int, int> _newKeyByOldKey = new Dictionary<int, int>();
        
        public static bool IsAnyKeyDown(params KeyCode[] keycodes)
        {
            return keycodes.Any(Utility.GetKey);
        }
        
        public static Vector2 GetMousePosition()
        {
#if USE_NEW_INPUTSYSTEM
            return Mouse.current.position.ReadValue();
#else
            return Input.mousePosition;
#endif
        }
        
        public static bool GetMouseButton(int button)
        {
#if USE_NEW_INPUTSYSTEM
            return GetKey(_mouseButtons[button]);
#else
            return Input.GetMouseButton(button);
#endif
        }
        
        public static bool GetMouseButtonDown(int button)
        {
#if USE_NEW_INPUTSYSTEM
            return GetKeyDown(_mouseButtons[button]);
#else
            return Input.GetMouseButtonDown(button);
#endif
        }
        
        public static bool GetMouseButtonUp(int button)
        {
#if USE_NEW_INPUTSYSTEM
            return GetKeyUp(_mouseButtons[button]);
#else
            return Input.GetMouseButtonUp(button);
#endif
        }

        public static bool GetKey(KeyCode key)
        {
#if USE_NEW_INPUTSYSTEM
            var newKey = GetMappedKey(key);
            if (newKey == Key.None && IsMouseButton(key))
                return GetKey(GetMouseKey(key));
            return GetKey(newKey);
#else
            return Input.GetKey(key);
#endif
        }
        
        public static bool GetKeyDown(KeyCode key)
        {
#if USE_NEW_INPUTSYSTEM
            var newKey = GetMappedKey(key);
            if (newKey == Key.None && IsMouseButton(key))
                return GetKeyDown(GetMouseKey(key));
            return GetKeyDown(newKey);
#else
            return Input.GetKeyDown(key);
#endif
        }
        
        public static bool GetKeyUp(KeyCode key)
        {
#if USE_NEW_INPUTSYSTEM
            var newKey = GetMappedKey(key);
            if (newKey == Key.None && IsMouseButton(key))
                return GetKeyUp(GetMouseKey(key));
            return GetKeyUp(newKey);
#else
            return Input.GetKeyUp(key);
#endif
        }

        private static readonly KeyCode[] _mouseButtons = new[]
        {
            KeyCode.Mouse0,
            KeyCode.Mouse1,
            KeyCode.Mouse2,
            KeyCode.Mouse3,
            KeyCode.Mouse4,
            KeyCode.Mouse5,
            KeyCode.Mouse6
        };
        
        private static bool IsMouseButton(KeyCode key)
        {
            return Array.IndexOf(_mouseButtons, key) != -1;
        }
        
#if USE_NEW_INPUTSYSTEM
        public static bool GetKey(ButtonControl control)
        {
            return control.isPressed;
        }
        
        public static bool GetKeyDown(ButtonControl control)
        {
            return control.wasPressedThisFrame;
        }
        
        public static bool GetKeyUp(ButtonControl control)
        {
            return control.wasReleasedThisFrame;
        }
        
        public static bool GetKey(Key key)
        {
            return Keyboard.current[key].isPressed;
        }
        
        public static bool GetKeyDown(Key key)
        {
            return Keyboard.current[key].wasPressedThisFrame;
        }
        
        public static bool GetKeyUp(Key key)
        {
            return Keyboard.current[key].wasReleasedThisFrame;
        }

        private static Key GetMappedKey(KeyCode key)
        {
            CreateLegacyToInputSystemMapping();
            return (Key) _newKeyByOldKey.GetOrDefault((int)key);
        }

        private static ButtonControl GetMouseKey(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Mouse0: return Mouse.current.leftButton;
                case KeyCode.Mouse1: return Mouse.current.rightButton;
                case KeyCode.Mouse2: return Mouse.current.middleButton;
                case KeyCode.Mouse3: return Mouse.current.backButton;
                case KeyCode.Mouse4: return Mouse.current.forwardButton;
                // IDK what these are
                // case KeyCode.Mouse5: return ;
                // case KeyCode.Mouse6: return ;
                default: return null;
            }
        }
        
#region keyMapping
        private static void RegisterKey(KeyCode key1, Key key2)
        {
            var key = (int)key1;
            if (!_newKeyByOldKey.ContainsKey(key))
                _newKeyByOldKey.Add(key, (int)key2);
        }
        
        private static void CreateLegacyToInputSystemMapping()
        {
            if (_newKeyByOldKey.Count != 0)
                return;
            
            RegisterKey(KeyCode.None,           Key.None);
            RegisterKey(KeyCode.Space,          Key.Space);
            RegisterKey(KeyCode.Return,         Key.Enter);
            RegisterKey(KeyCode.Tab,            Key.Tab);
            RegisterKey(KeyCode.BackQuote,      Key.Backquote);
            RegisterKey(KeyCode.Quote,          Key.Quote);
            RegisterKey(KeyCode.Semicolon,      Key.Semicolon);
            RegisterKey(KeyCode.Comma,          Key.Comma);
            RegisterKey(KeyCode.Period,         Key.Period);
            RegisterKey(KeyCode.Slash,          Key.Slash);
            RegisterKey(KeyCode.Backslash,      Key.Backslash);
            RegisterKey(KeyCode.LeftBracket,    Key.LeftBracket);
            RegisterKey(KeyCode.RightBracket,   Key.RightBracket);
            RegisterKey(KeyCode.Minus,          Key.Minus);
            RegisterKey(KeyCode.Equals,         Key.Equals);
            RegisterKey(KeyCode.A,              Key.A);
            RegisterKey(KeyCode.B,              Key.B);
            RegisterKey(KeyCode.C,              Key.C);
            RegisterKey(KeyCode.D,              Key.D);
            RegisterKey(KeyCode.E,              Key.E);
            RegisterKey(KeyCode.F,              Key.F);
            RegisterKey(KeyCode.G,              Key.G);
            RegisterKey(KeyCode.H,              Key.H);
            RegisterKey(KeyCode.I,              Key.I);
            RegisterKey(KeyCode.J,              Key.J);
            RegisterKey(KeyCode.K,              Key.K);
            RegisterKey(KeyCode.L,              Key.L);
            RegisterKey(KeyCode.M,              Key.M);
            RegisterKey(KeyCode.N,              Key.N);
            RegisterKey(KeyCode.O,              Key.O);
            RegisterKey(KeyCode.P,              Key.P);
            RegisterKey(KeyCode.Q,              Key.Q);
            RegisterKey(KeyCode.R,              Key.R);
            RegisterKey(KeyCode.S,              Key.S);
            RegisterKey(KeyCode.T,              Key.T);
            RegisterKey(KeyCode.U,              Key.U);
            RegisterKey(KeyCode.V,              Key.V);
            RegisterKey(KeyCode.W,              Key.W);
            RegisterKey(KeyCode.X,              Key.X);
            RegisterKey(KeyCode.Y,              Key.Y);
            RegisterKey(KeyCode.Z,              Key.Z);
            RegisterKey(KeyCode.Alpha1,         Key.Digit1);
            RegisterKey(KeyCode.Alpha2,         Key.Digit2);
            RegisterKey(KeyCode.Alpha3,         Key.Digit3);
            RegisterKey(KeyCode.Alpha4,         Key.Digit4);
            RegisterKey(KeyCode.Alpha5,         Key.Digit5);
            RegisterKey(KeyCode.Alpha6,         Key.Digit6);
            RegisterKey(KeyCode.Alpha7,         Key.Digit7);
            RegisterKey(KeyCode.Alpha8,         Key.Digit8);
            RegisterKey(KeyCode.Alpha9,         Key.Digit9);
            RegisterKey(KeyCode.Alpha0,         Key.Digit0);
            RegisterKey(KeyCode.LeftShift,      Key.LeftShift);
            RegisterKey(KeyCode.RightShift,     Key.RightShift);
            RegisterKey(KeyCode.LeftAlt,        Key.LeftAlt);
            RegisterKey(KeyCode.AltGr,          Key.AltGr);
            RegisterKey(KeyCode.RightAlt,       Key.RightAlt);
            RegisterKey(KeyCode.LeftControl,    Key.LeftCtrl);
            RegisterKey(KeyCode.RightControl,   Key.RightCtrl);
            RegisterKey(KeyCode.LeftApple,      Key.LeftApple);
            RegisterKey(KeyCode.LeftCommand,    Key.LeftCommand);
            RegisterKey(KeyCode.LeftWindows,    Key.LeftWindows);
            RegisterKey(KeyCode.RightApple,     Key.RightApple);
            RegisterKey(KeyCode.RightCommand,   Key.RightCommand);
            RegisterKey(KeyCode.RightWindows,   Key.RightWindows);
            RegisterKey(KeyCode.Menu,           Key.ContextMenu);
            RegisterKey(KeyCode.Escape,         Key.Escape);
            RegisterKey(KeyCode.LeftArrow,      Key.LeftArrow);
            RegisterKey(KeyCode.RightArrow,     Key.RightArrow);
            RegisterKey(KeyCode.UpArrow,        Key.UpArrow);
            RegisterKey(KeyCode.DownArrow,      Key.DownArrow);
            RegisterKey(KeyCode.Backspace,      Key.Backspace);
            RegisterKey(KeyCode.PageDown,       Key.PageDown);
            RegisterKey(KeyCode.PageUp,         Key.PageUp);
            RegisterKey(KeyCode.Home,           Key.Home);
            RegisterKey(KeyCode.End,            Key.End);
            RegisterKey(KeyCode.Insert,         Key.Insert);
            RegisterKey(KeyCode.Delete,         Key.Delete);
            RegisterKey(KeyCode.CapsLock,       Key.CapsLock);
            RegisterKey(KeyCode.Numlock,        Key.NumLock);
            RegisterKey(KeyCode.Print,          Key.PrintScreen);
            RegisterKey(KeyCode.ScrollLock,     Key.ScrollLock);
            RegisterKey(KeyCode.Pause,          Key.Pause);
            RegisterKey(KeyCode.KeypadEnter,    Key.NumpadEnter);
            RegisterKey(KeyCode.KeypadDivide,   Key.NumpadDivide);
            RegisterKey(KeyCode.KeypadMultiply, Key.NumpadMultiply);
            RegisterKey(KeyCode.KeypadPlus,     Key.NumpadPlus);
            RegisterKey(KeyCode.KeypadMinus,    Key.NumpadMinus);
            RegisterKey(KeyCode.KeypadPeriod,   Key.NumpadPeriod);
            RegisterKey(KeyCode.KeypadEquals,   Key.NumpadEquals);
            RegisterKey(KeyCode.Keypad0,        Key.Numpad0);
            RegisterKey(KeyCode.Keypad1,        Key.Numpad1);
            RegisterKey(KeyCode.Keypad2,        Key.Numpad2);
            RegisterKey(KeyCode.Keypad3,        Key.Numpad3);
            RegisterKey(KeyCode.Keypad4,        Key.Numpad4);
            RegisterKey(KeyCode.Keypad5,        Key.Numpad5);
            RegisterKey(KeyCode.Keypad6,        Key.Numpad6);
            RegisterKey(KeyCode.Keypad7,        Key.Numpad7);
            RegisterKey(KeyCode.Keypad8,        Key.Numpad8);
            RegisterKey(KeyCode.Keypad9,        Key.Numpad9);
            RegisterKey(KeyCode.F1,             Key.F1);
            RegisterKey(KeyCode.F2,             Key.F2);
            RegisterKey(KeyCode.F3,             Key.F3);
            RegisterKey(KeyCode.F4,             Key.F4);
            RegisterKey(KeyCode.F5,             Key.F5);
            RegisterKey(KeyCode.F6,             Key.F6);
            RegisterKey(KeyCode.F7,             Key.F7);
            RegisterKey(KeyCode.F8,             Key.F8);
            RegisterKey(KeyCode.F9,             Key.F9);
            RegisterKey(KeyCode.F10,            Key.F10);
            RegisterKey(KeyCode.F11,            Key.F11);
            RegisterKey(KeyCode.F12,            Key.F12);
#if UNITY_2021_2_OR_NEWER
            RegisterKey(KeyCode.LeftMeta,       Key.LeftMeta);
            RegisterKey(KeyCode.RightMeta,      Key.RightMeta);
#endif
        }
#endregion keyMapping
#endif
    }
}