using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ReformSim
{
    public class ImageShaderKeyboardInput : MonoBehaviour
    {
        public RenderTexture m_keyboardInputRT;
        protected Texture2D m_keyboardDataTex;
        protected FilterMode m_texFilterMode = FilterMode.Point;

        // Row 0: contain the current state of the 256 keys. 
        // Row 1: contains Keypress.
        // Row 2: contains a toggle for every key.
        // Texel positions correspond to ASCII codes.
        protected Color[] m_keyboardData0 = new Color[256];
        protected Color[] m_keyboardData1 = new Color[256];
        protected Color[] m_keyboardData2 = new Color[256];

        protected void Start()
        {
            if (m_keyboardInputRT == null)
            {
                Debug.LogError("Error: Please assign the 'KeyboardInput' RenderTexture to the 'Keyboard Input RT' first!", this);
            }

            m_keyboardDataTex = new Texture2D(256, 3, TextureFormat.R8, false, true);
            m_keyboardDataTex.filterMode = m_texFilterMode;
        }

        protected void Update()
        {
            for (int i = 0; i < m_keyboardData0.Length; i++)
            {
                KeyCode keyCode = AsciiInJavascriptToKeyCode(i);
                if (keyCode == KeyCode.None)
                {
                    continue;
                }

#if ENABLE_INPUT_SYSTEM
                Key key = KeyCodeToKey(keyCode);
                if (key != Key.None)
                {
                    UpdateKeyboardData(i, key);
                }

                //UpdateKeyboardData(37, Key.LeftArrow);
                //UpdateKeyboardData(38, Key.UpArrow);
                //UpdateKeyboardData(39, Key.RightArrow);
                //UpdateKeyboardData(40, Key.DownArrow);
#else

                UpdateKeyboardData(i, keyCode);

                //UpdateKeyboardData(37, KeyCode.LeftArrow);
                //UpdateKeyboardData(38, KeyCode.UpArrow);
                //UpdateKeyboardData(39, KeyCode.RightArrow);
                //UpdateKeyboardData(40, KeyCode.DownArrow);
#endif
            }

            m_keyboardDataTex.SetPixels(0, 0, m_keyboardData0.Length, 1, m_keyboardData0, 0);
            m_keyboardDataTex.SetPixels(0, 1, m_keyboardData1.Length, 1, m_keyboardData1, 0);
            m_keyboardDataTex.SetPixels(0, 2, m_keyboardData2.Length, 1, m_keyboardData2, 0);
            
            m_keyboardDataTex.Apply();

            Graphics.Blit(m_keyboardDataTex, m_keyboardInputRT);
        }

        public static KeyCode AsciiInJavascriptToKeyCode(int asciiCodeInJavascript, KeyCode unknownKey = KeyCode.None)
        {
            switch (asciiCodeInJavascript)
            {
                case 0: return KeyCode.None;
                case 8: return KeyCode.Backspace;
                case 9: return KeyCode.Tab;
                case 13: return KeyCode.Return;
                case 16: return KeyCode.LeftShift;
                //case 16: return KeyCode.RightShift;
                case 17: return KeyCode.LeftControl;
                //case 17: return KeyCode.RightControl;
                case 18: return KeyCode.LeftAlt;
                //case 18: return KeyCode.RightAlt;
                case 19: return KeyCode.Pause;
                case 20: return KeyCode.CapsLock;
                case 27: return KeyCode.Escape;
                case 32: return KeyCode.Space;
                case 33: return KeyCode.PageUp;
                case 34: return KeyCode.PageDown;
                case 35: return KeyCode.End;
                case 36: return KeyCode.Home;
                case 37: return KeyCode.LeftArrow;
                case 38: return KeyCode.UpArrow;
                case 39: return KeyCode.RightArrow;
                case 40: return KeyCode.DownArrow;
                case 44: return KeyCode.Print;
                case 45: return KeyCode.Insert;
                case 46: return KeyCode.Delete;
                
                case 48: return KeyCode.Alpha0;
                case 49: return KeyCode.Alpha1;
                case 50: return KeyCode.Alpha2;
                case 51: return KeyCode.Alpha3;
                case 52: return KeyCode.Alpha4;
                case 53: return KeyCode.Alpha5;
                case 54: return KeyCode.Alpha6;
                case 55: return KeyCode.Alpha7;
                case 56: return KeyCode.Alpha8;
                case 57: return KeyCode.Alpha9;
                
                case 65: return KeyCode.A;
                case 66: return KeyCode.B;
                case 67: return KeyCode.C;
                case 68: return KeyCode.D;
                case 69: return KeyCode.E;
                case 70: return KeyCode.F;
                case 71: return KeyCode.G;
                case 72: return KeyCode.H;
                case 73: return KeyCode.I;
                case 74: return KeyCode.J;
                case 75: return KeyCode.K;
                case 76: return KeyCode.L;
                case 77: return KeyCode.M;
                case 78: return KeyCode.N;
                case 79: return KeyCode.O;
                case 80: return KeyCode.P;
                case 81: return KeyCode.Q;
                case 82: return KeyCode.R;
                case 83: return KeyCode.S;
                case 84: return KeyCode.T;
                case 85: return KeyCode.U;
                case 86: return KeyCode.V;
                case 87: return KeyCode.W;
                case 88: return KeyCode.X;
                case 89: return KeyCode.Y;
                case 90: return KeyCode.Z;

                case 96: return KeyCode.Keypad0;
                case 97: return KeyCode.Keypad1;
                case 98: return KeyCode.Keypad2;
                case 99: return KeyCode.Keypad3;
                case 100: return KeyCode.Keypad4;
                case 101: return KeyCode.Keypad5;
                case 102: return KeyCode.Keypad6;
                case 103: return KeyCode.Keypad7;
                case 104: return KeyCode.Keypad8;
                case 105: return KeyCode.Keypad9;

                case 106: return KeyCode.KeypadMultiply;
                case 107: return KeyCode.KeypadPlus;
                case 109: return KeyCode.KeypadMinus;
                case 110: return KeyCode.KeypadPeriod;
                case 111: return KeyCode.KeypadDivide;

                case 112: return KeyCode.F1;
                case 113: return KeyCode.F2;
                case 114: return KeyCode.F3;
                case 115: return KeyCode.F4;
                case 116: return KeyCode.F5;
                case 117: return KeyCode.F6;
                case 118: return KeyCode.F7;
                case 119: return KeyCode.F8;
                case 120: return KeyCode.F9;
                case 121: return KeyCode.F10;
                case 122: return KeyCode.F11;
                case 123: return KeyCode.F12;

                case 144: return KeyCode.Numlock;
                case 145: return KeyCode.ScrollLock;

                case 186: return KeyCode.Semicolon;
                case 187: return KeyCode.Equals;
                case 188: return KeyCode.Comma;
                case 189: return KeyCode.Minus;
                case 190: return KeyCode.Period;
                case 191: return KeyCode.Slash;
                case 192: return KeyCode.BackQuote;
                
                case 219: return KeyCode.LeftBracket;
                case 220: return KeyCode.Backslash;
                case 221: return KeyCode.RightBracket;
                case 222: return KeyCode.Quote;
                default:
                    return unknownKey;
            }
        }

        protected void UpdateKeyboardData(int asciiCode, KeyCode keyCode)
        {
            if (Input.GetKey(keyCode))
            {
                m_keyboardData0[asciiCode] = Color.red;
            }
            else
            {
                m_keyboardData0[asciiCode] = Color.black;
            }

            if (Input.GetKeyDown(keyCode))
            {
                m_keyboardData1[asciiCode] = Color.red;
            }
            else
            {
                m_keyboardData1[asciiCode] = Color.black;
            }

            if (Input.GetKeyUp(keyCode))
            {
                m_keyboardData2[asciiCode] = m_keyboardData2[asciiCode] == Color.red ? Color.black : Color.red;
            }
        }

#if ENABLE_INPUT_SYSTEM
        public static Key KeyCodeToKey(KeyCode keyCode,
            Key unknownKey = Key.None,
            Key mouseKey = Key.None,
            Key joystickKey = Key.None)
        {
            switch (keyCode)
            {
                case KeyCode.None:              return Key.None;
                case KeyCode.Backspace:         return Key.Backspace;
                case KeyCode.Delete:            return Key.Delete;
                case KeyCode.Tab:               return Key.Tab;
                case KeyCode.Clear:             return unknownKey; // Conversion unknown.
                case KeyCode.Return:            return Key.Enter;
                case KeyCode.Pause:             return Key.Pause;
                case KeyCode.Escape:            return Key.Escape;
                case KeyCode.Space:             return Key.Space;
                case KeyCode.Keypad0:           return Key.Numpad0;
                case KeyCode.Keypad1:           return Key.Numpad1;
                case KeyCode.Keypad2:           return Key.Numpad2;
                case KeyCode.Keypad3:           return Key.Numpad3;
                case KeyCode.Keypad4:           return Key.Numpad4;
                case KeyCode.Keypad5:           return Key.Numpad5;
                case KeyCode.Keypad6:           return Key.Numpad6;
                case KeyCode.Keypad7:           return Key.Numpad7;
                case KeyCode.Keypad8:           return Key.Numpad8;
                case KeyCode.Keypad9:           return Key.Numpad9;
                case KeyCode.KeypadPeriod:      return Key.NumpadPeriod;
                case KeyCode.KeypadDivide:      return Key.NumpadDivide;
                case KeyCode.KeypadMultiply:    return Key.NumpadMultiply;
                case KeyCode.KeypadMinus:       return Key.NumpadMinus;
                case KeyCode.KeypadPlus:        return Key.NumpadPlus;
                case KeyCode.KeypadEnter:       return Key.NumpadEnter;
                case KeyCode.KeypadEquals:      return Key.NumpadEquals;
                case KeyCode.UpArrow:           return Key.UpArrow;
                case KeyCode.DownArrow:         return Key.DownArrow;
                case KeyCode.RightArrow:        return Key.RightArrow;
                case KeyCode.LeftArrow:         return Key.LeftArrow;
                case KeyCode.Insert:            return Key.Insert;
                case KeyCode.Home:              return Key.Home;
                case KeyCode.End:               return Key.End;
                case KeyCode.PageUp:            return Key.PageUp;
                case KeyCode.PageDown:          return Key.PageDown;
                case KeyCode.F1:                return Key.F1;
                case KeyCode.F2:                return Key.F2;
                case KeyCode.F3:                return Key.F3;
                case KeyCode.F4:                return Key.F4;
                case KeyCode.F5:                return Key.F5;
                case KeyCode.F6:                return Key.F6;
                case KeyCode.F7:                return Key.F7;
                case KeyCode.F8:                return Key.F8;
                case KeyCode.F9:                return Key.F9;
                case KeyCode.F10:               return Key.F10;
                case KeyCode.F11:               return Key.F11;
                case KeyCode.F12:               return Key.F12;
                case KeyCode.F13:               return unknownKey; // Conversion unknown.
                case KeyCode.F14:               return unknownKey; // Conversion unknown.
                case KeyCode.F15:               return unknownKey; // Conversion unknown.
                case KeyCode.Alpha0:            return Key.Digit0;
                case KeyCode.Alpha1:            return Key.Digit1;
                case KeyCode.Alpha2:            return Key.Digit2;
                case KeyCode.Alpha3:            return Key.Digit3;
                case KeyCode.Alpha4:            return Key.Digit4;
                case KeyCode.Alpha5:            return Key.Digit5;
                case KeyCode.Alpha6:            return Key.Digit6;
                case KeyCode.Alpha7:            return Key.Digit7;
                case KeyCode.Alpha8:            return Key.Digit8;
                case KeyCode.Alpha9:            return Key.Digit9;
                case KeyCode.Exclaim:           return unknownKey; // Conversion unknown.
                case KeyCode.DoubleQuote:       return unknownKey; // Conversion unknown.
                case KeyCode.Hash:              return unknownKey; // Conversion unknown.
                case KeyCode.Dollar:            return unknownKey; // Conversion unknown.
                case KeyCode.Percent:           return unknownKey; // Conversion unknown.
                case KeyCode.Ampersand:         return unknownKey; // Conversion unknown.
                case KeyCode.Quote:             return Key.Quote;
                case KeyCode.LeftParen:         return unknownKey; // Conversion unknown.
                case KeyCode.RightParen:        return unknownKey; // Conversion unknown.
                case KeyCode.Asterisk:          return unknownKey; // Conversion unknown.
                case KeyCode.Plus:              return Key.None; // TODO
                case KeyCode.Comma:             return Key.Comma;
                case KeyCode.Minus:             return Key.Minus;
                case KeyCode.Period:            return Key.Period;
                case KeyCode.Slash:             return Key.Slash;
                case KeyCode.Colon:             return unknownKey; // Conversion unknown.
                case KeyCode.Semicolon:         return Key.Semicolon;
                case KeyCode.Less:              return Key.None;
                case KeyCode.Equals:            return Key.Equals;
                case KeyCode.Greater:           return unknownKey; // Conversion unknown.
                case KeyCode.Question:          return unknownKey; // Conversion unknown.
                case KeyCode.At:                return unknownKey; // Conversion unknown.
                case KeyCode.LeftBracket:       return Key.LeftBracket;
                case KeyCode.Backslash:         return Key.Backslash;
                case KeyCode.RightBracket:      return Key.RightBracket;
                case KeyCode.Caret:             return Key.None; // TODO
                case KeyCode.Underscore:        return unknownKey; // Conversion unknown.
                case KeyCode.BackQuote:         return Key.Backquote;
                case KeyCode.A:                 return Key.A;
                case KeyCode.B:                 return Key.B;
                case KeyCode.C:                 return Key.C;
                case KeyCode.D:                 return Key.D;
                case KeyCode.E:                 return Key.E;
                case KeyCode.F:                 return Key.F;
                case KeyCode.G:                 return Key.G;
                case KeyCode.H:                 return Key.H;
                case KeyCode.I:                 return Key.I;
                case KeyCode.J:                 return Key.J;
                case KeyCode.K:                 return Key.K;
                case KeyCode.L:                 return Key.L;
                case KeyCode.M:                 return Key.M;
                case KeyCode.N:                 return Key.N;
                case KeyCode.O:                 return Key.O;
                case KeyCode.P:                 return Key.P;
                case KeyCode.Q:                 return Key.Q;
                case KeyCode.R:                 return Key.R;
                case KeyCode.S:                 return Key.S;
                case KeyCode.T:                 return Key.T;
                case KeyCode.U:                 return Key.U;
                case KeyCode.V:                 return Key.V;
                case KeyCode.W:                 return Key.W;
                case KeyCode.X:                 return Key.X;             
                case KeyCode.Y:                 return Key.Y;
                case KeyCode.Z:                 return Key.Z;
                case KeyCode.LeftCurlyBracket:  return unknownKey; // Conversion unknown.
                case KeyCode.Pipe:              return unknownKey; // Conversion unknown.
                case KeyCode.RightCurlyBracket: return unknownKey; // Conversion unknown.
                case KeyCode.Tilde:             return unknownKey; // Conversion unknown.
                case KeyCode.Numlock:           return Key.NumLock;
                case KeyCode.CapsLock:          return Key.CapsLock;
                case KeyCode.ScrollLock:        return Key.ScrollLock;
                case KeyCode.RightShift:        return Key.RightShift;
                case KeyCode.LeftShift:         return Key.LeftShift;
                case KeyCode.RightControl:      return Key.RightCtrl;
                case KeyCode.LeftControl:       return Key.LeftCtrl;
                case KeyCode.RightAlt:          return Key.RightAlt;
                case KeyCode.LeftAlt:           return Key.LeftAlt;
                case KeyCode.LeftCommand:       return Key.LeftCommand;
                  // case KeyCode.LeftApple: (same as LeftCommand)
                case KeyCode.LeftWindows:       return Key.LeftWindows;
                case KeyCode.RightCommand:      return Key.RightCommand;
                  // case KeyCode.RightApple: (same as RightCommand)
                case KeyCode.RightWindows:      return Key.RightWindows;
                case KeyCode.AltGr:             return Key.AltGr;
                case KeyCode.Help:              return unknownKey; // Conversion unknown.
                case KeyCode.Print:             return Key.PrintScreen;
                case KeyCode.SysReq:            return unknownKey; // Conversion unknown.
                case KeyCode.Break:             return unknownKey; // Conversion unknown.
                case KeyCode.Menu:              return Key.ContextMenu;
                case KeyCode.Mouse0:
                case KeyCode.Mouse1:
                case KeyCode.Mouse2:
                case KeyCode.Mouse3:
                case KeyCode.Mouse4:
                case KeyCode.Mouse5:
                case KeyCode.Mouse6:
                    return mouseKey; // Not supported anymore.

                // All other keys are joystick keys which do not
                // exist anymore in the new input system.
                default:
                    return joystickKey; // Not supported anymore.
            }
        }

        protected void UpdateKeyboardData(int asciiCode, Key key)
        {
            if (Keyboard.current != null && Keyboard.current[key].isPressed)
            {
                m_keyboardData0[asciiCode] = Color.red;
            }
            else
            {
                m_keyboardData0[asciiCode] = Color.black;
            }

            if (Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame)
            {
                m_keyboardData1[asciiCode] = Color.red;
            }
            else
            {
                m_keyboardData1[asciiCode] = Color.black;
            }

            if (Keyboard.current != null && Keyboard.current[key].wasReleasedThisFrame)
            {
                m_keyboardData2[asciiCode] = m_keyboardData2[asciiCode] == Color.red ? Color.black : Color.red;
            }
        }
#endif

//#if UNITY_EDITOR
//        public bool m_debugShowRenderTextures = false;

//        protected void OnGUI()
//        {
//            ShowRenderTextures(m_debugShowRenderTextures, m_keyboardInputRT);
//        }
//#endif

        public void ShowRenderTextures(bool showRenderTexture, Texture texture)
        {
            if (showRenderTexture)
            {
                if (texture != null)
                {
                    GUI.DrawTexture(new Rect(0, 100, texture.width*2, texture.height*100), texture, ScaleMode.StretchToFill, false);
                }
            }
        }

        protected void OnDestroy()
        {
            Destroy(m_keyboardDataTex);
        }
    }
}