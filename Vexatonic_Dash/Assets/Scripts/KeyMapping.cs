using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyMapping
{
    private static List<Tuple<KeyCode, int>> _keyMappingList = new()
    {
        new(KeyCode.None,              0),
        new(KeyCode.Backspace,         8),
        new(KeyCode.Tab,               9),
        new(KeyCode.CapsLock,         20),
        new(KeyCode.Space,            32),
        new(KeyCode.PageUp,           33),
        new(KeyCode.PageDown,         34),
        new(KeyCode.End,              35),
        new(KeyCode.Home,             36),
        new(KeyCode.LeftArrow,        37),
        new(KeyCode.UpArrow,          38),
        new(KeyCode.RightArrow,       39),
        new(KeyCode.DownArrow,        40),
        new(KeyCode.Insert,           45),
        new(KeyCode.Delete,           46),
        new(KeyCode.Alpha0,           48),
        new(KeyCode.Alpha1,           49),
        new(KeyCode.Alpha2,           50),
        new(KeyCode.Alpha3,           51),
        new(KeyCode.Alpha4,           52),
        new(KeyCode.Alpha5,           53),
        new(KeyCode.Alpha6,           54),
        new(KeyCode.Alpha7,           55),
        new(KeyCode.Alpha8,           56),
        new(KeyCode.Alpha9,           57),
        new(KeyCode.A,                65),
        new(KeyCode.B,                66),
        new(KeyCode.C,                67),
        new(KeyCode.D,                68),
        new(KeyCode.E,                69),
        new(KeyCode.F,                70),
        new(KeyCode.G,                71),
        new(KeyCode.H,                72),
        new(KeyCode.I,                73),
        new(KeyCode.J,                74),
        new(KeyCode.K,                75),
        new(KeyCode.L,                76),
        new(KeyCode.M,                77),
        new(KeyCode.N,                78),
        new(KeyCode.O,                79),
        new(KeyCode.P,                80),
        new(KeyCode.Q,                81),
        new(KeyCode.R,                82),
        new(KeyCode.S,                83),
        new(KeyCode.T,                84),
        new(KeyCode.U,                85),
        new(KeyCode.V,                86),
        new(KeyCode.W,                87),
        new(KeyCode.X,                88),
        new(KeyCode.Y,                89),
        new(KeyCode.Z,                90),
        new(KeyCode.LeftWindows,      91),
        new(KeyCode.RightWindows,     92),
        new(KeyCode.Keypad0,          96),
        new(KeyCode.Keypad1,          97),
        new(KeyCode.Keypad2,          98),
        new(KeyCode.Keypad3,          99),
        new(KeyCode.Keypad4,         100),
        new(KeyCode.Keypad5,         101),
        new(KeyCode.Keypad6,         102),
        new(KeyCode.Keypad7,         103),
        new(KeyCode.Keypad8,         104),
        new(KeyCode.Keypad9,         105),
        new(KeyCode.KeypadMultiply,  106),
        new(KeyCode.KeypadPlus,      107),
        new(KeyCode.KeypadEnter,     108),
        new(KeyCode.KeypadMinus,     109),
        new(KeyCode.KeypadPeriod,    110),
        new(KeyCode.KeypadDivide,    111),
        new(KeyCode.F1,              112),
        new(KeyCode.F2,              113),
        new(KeyCode.F3,              114),
        new(KeyCode.F4,              115),
        new(KeyCode.F5,              116),
        new(KeyCode.F6,              117),
        new(KeyCode.F7,              118),
        new(KeyCode.F8,              119),
        new(KeyCode.F9,              120),
        new(KeyCode.F10,             121),
        new(KeyCode.F11,             122),
        new(KeyCode.F12,             123),
        new(KeyCode.Numlock,         144),
        new(KeyCode.ScrollLock,      145),
        new(KeyCode.LeftShift,       160),
        new(KeyCode.RightShift,      161),
        new(KeyCode.LeftControl,     162),
        new(KeyCode.RightControl,    163),
        new(KeyCode.LeftAlt,         164),
        new(KeyCode.RightAlt,        165),
        new(KeyCode.Semicolon,       186),
        new(KeyCode.Equals,          187),
        new(KeyCode.Comma,           188),
        new(KeyCode.Minus,           189),
        new(KeyCode.Period,          190),
        new(KeyCode.Slash,           191),
        new(KeyCode.BackQuote,       192),
        new(KeyCode.LeftBracket,     219),
        new(KeyCode.Backslash,       220),
        new(KeyCode.RightBracket,    221),
        new(KeyCode.Quote,           222),
    };

    public static int UnityToSystem(KeyCode keyCode)
    {
        Tuple<KeyCode, int> findResult = _keyMappingList.Find(pair => pair.Item1 == keyCode);

        if (findResult.Equals(default)) return 0;
        else return findResult.Item2;
    }

    public static KeyCode SystemToUnity(int keyCode)
    {
        Tuple<KeyCode, int> findResult = _keyMappingList.Find(pair => pair.Item2 == keyCode);

        if (findResult.Equals(default)) return KeyCode.None;
        else return findResult.Item1;
    }
}