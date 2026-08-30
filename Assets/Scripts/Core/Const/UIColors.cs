using UnityEngine;

namespace Assets.Scripts.Core.Const
{
    public class UIColors
    {
        public static readonly Color Red = new Color(1, 0, 0);
        public static readonly Color Green = new Color(0, 1, 0);
        public static readonly Color Blue = new Color(0, 0, 1);
        public static readonly Color Yellow = new Color(1, 1, 0);
        public static readonly Color Cyan = new Color(0, 1, 1);
        public static readonly Color Magenta = new Color(1, 0, 1);
        public static readonly Color White = new Color(1, 1, 1);
        public static readonly Color Black = new Color(0, 0, 0);


        public interface IColorSet
        {
            Color Background { get; }
            Color InnerColor1 { get; }
            Color Glow { get; }
        }

        public sealed class PlumColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(143, 74, 189, 255);
            public Color InnerColor1 { get; } = new Color32(181, 93, 214, 255);
            public Color Glow { get; } = new Color32(123, 63, 164, 255);
        }

        public sealed class GreenColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(37, 124, 85, 255);
            public Color InnerColor1 { get; } = new Color32(57, 151, 90, 255);
            public Color Glow { get; } = new Color32(27, 94, 64, 255);
        }

        public sealed class GrayColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(99, 94, 90, 255);
            public Color InnerColor1 { get; } = new Color32(115, 113, 115, 255);
            public Color Glow { get; } = new Color32(67, 63, 60, 255);
        }

        public sealed class BlueColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(49, 97, 165, 255);
            public Color InnerColor1 { get; } = new Color32(57, 121, 206, 255);
            public Color Glow { get; } = new Color32(49, 69, 132, 255);
        }

        public sealed class YellowColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(202, 176, 70, 255);
            public Color InnerColor1 { get; } = new Color32(231, 211, 74, 255);
            public Color Glow { get; } = new Color32(183, 162, 61, 255);
        }

        public sealed class RedColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(206, 43, 42, 255);
            public Color InnerColor1 { get; } = new Color32(214, 55, 49, 255);
            public Color Glow { get; } = new Color32(164, 37, 36, 255);
        }
    }
}
