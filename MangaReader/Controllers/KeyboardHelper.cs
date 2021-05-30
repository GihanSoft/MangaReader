﻿using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace MangaReader.Controllers
{
    internal static class KeyboardHelper
    {
        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        public static char GetCharFromKey(Key key)
        {
            char ch = '\0';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                case 0:
                    break;
                default:
                    ch = stringBuilder[0];
                    break;
            }
            return ch;
        }
    }
}
