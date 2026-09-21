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
            Color Light { get; }
            Color Highlight { get; }
            Color Gradient { get; }
            Color Shadow { get; }
            bool Button01InnerVisible { get; }
        }

        public sealed class PlumColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(136, 39, 243, 255);
            public Color InnerColor1 { get; } = new Color32(181, 69, 248, 255);
            public Color Glow { get; } = new Color32(106, 0, 222, 255);
            public Color Light { get; } = new Color32(181, 69, 248, 255);
            public Color Highlight { get; } = new Color32(198, 109, 255, 255);
            public Color Gradient { get; } = new Color32(160, 55, 249, 255);
            public Color Shadow { get; } = new Color32(106, 0, 222, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class GreenColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(110, 170, 60, 255);
            public Color InnerColor1 { get; } = new Color32(165, 206, 49, 255);
            public Color Glow { get; } = new Color32(82, 146, 32, 255);
            public Color Light { get; } = new Color32(165, 206, 49, 255);
            public Color Highlight { get; } = new Color32(204, 221, 90, 255);
            public Color Gradient { get; } = new Color32(132, 183, 53, 255);
            public Color Shadow { get; } = new Color32(82, 146, 32, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class GrayColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(82, 75, 74, 255);
            public Color InnerColor1 { get; } = new Color32(107, 94, 90, 255);
            public Color Glow { get; } = new Color32(57, 48, 49, 255);
            public Color Light { get; } = new Color32(107, 94, 90, 255);
            public Color Highlight { get; } = new Color32(123, 113, 108, 255);
            public Color Gradient { get; } = new Color32(99, 88, 82, 255);
            public Color Shadow { get; } = new Color32(57, 48, 49, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class BlueColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(9, 138, 211, 255);
            public Color InnerColor1 { get; } = new Color32(41, 173, 230, 255);
            public Color Glow { get; } = new Color32(6, 121, 190, 255);
            public Color Light { get; } = new Color32(41, 173, 230, 255);
            public Color Highlight { get; } = new Color32(66, 215, 255, 255);
            public Color Gradient { get; } = new Color32(30, 165, 222, 255);
            public Color Shadow { get; } = new Color32(6, 121, 190, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class BrownColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(165, 125, 66, 255);
            public Color InnerColor1 { get; } = new Color32(198, 154, 74, 255);
            public Color Glow { get; } = new Color32(123, 85, 49, 255);
            public Color Light { get; } = new Color32(198, 154, 74, 255);
            public Color Highlight { get; } = new Color32(225, 183, 110, 255);
            public Color Gradient { get; } = new Color32(186, 145, 75, 255);
            public Color Shadow { get; } = new Color32(123, 85, 49, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class DarkColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(45, 44, 45, 255);
            public Color InnerColor1 { get; } = new Color32(57, 60, 57, 255);
            public Color Glow { get; } = new Color32(33, 32, 33, 255);
            public Color Light { get; } = new Color32(57, 60, 57, 255);
            public Color Highlight { get; } = new Color32(82, 81, 82, 255);
            public Color Gradient { get; } = new Color32(54, 55, 54, 255);
            public Color Shadow { get; } = new Color32(33, 32, 33, 255);
            public bool Button01InnerVisible => false;
        }

        public sealed class MintColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(6, 171, 154, 255);
            public Color InnerColor1 { get; } = new Color32(16, 215, 156, 255);
            public Color Glow { get; } = new Color32(0, 138, 115, 255);
            public Color Light { get; } = new Color32(16, 215, 156, 255);
            public Color Highlight { get; } = new Color32(39, 231, 172, 255);
            public Color Gradient { get; } = new Color32(10, 190, 148, 255);
            public Color Shadow { get; } = new Color32(0, 138, 115, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class OffWhiteColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(214, 212, 211, 255);
            public Color InnerColor1 { get; } = Color.white;
            public Color Glow { get; } = new Color32(176, 169, 168, 255);
            public Color Light { get; } = Color.white;
            public Color Highlight { get; } = Color.white;
            public Color Gradient { get; } = new Color32(231, 231, 230, 255);
            public Color Shadow { get; } = new Color32(176, 169, 168, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class OrangeColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(248, 101, 58, 255);
            public Color InnerColor1 { get; } = new Color32(247, 158, 99, 255);
            public Color Glow { get; } = new Color32(214, 81, 49, 255);
            public Color Light { get; } = new Color32(247, 158, 99, 255);
            public Color Highlight { get; } = new Color32(255, 182, 115, 255);
            public Color Gradient { get; } = new Color32(252, 127, 66, 255);
            public Color Shadow { get; } = new Color32(214, 81, 49, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class PinkColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(189, 65, 181, 255);
            public Color InnerColor1 { get; } = new Color32(222, 101, 214, 255);
            public Color Glow { get; } = new Color32(148, 47, 156, 255);
            public Color Light { get; } = new Color32(222, 101, 214, 255);
            public Color Highlight { get; } = new Color32(239, 113, 231, 255);
            public Color Gradient { get; } = new Color32(206, 85, 197, 255);
            public Color Shadow { get; } = new Color32(148, 47, 156, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class YellowColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(251, 158, 0, 255);
            public Color InnerColor1 { get; } = new Color32(255, 207, 25, 255);
            public Color Glow { get; } = new Color32(214, 125, 6, 255);
            public Color Light { get; } = new Color32(255, 207, 25, 255);
            public Color Highlight { get; } = new Color32(255, 233, 40, 255);
            public Color Gradient { get; } = new Color32(255, 190, 17, 255);
            public Color Shadow { get; } = new Color32(214, 125, 6, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class RedColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(115, 44, 44, 255);
            public Color InnerColor1 { get; } = new Color32(136, 56, 57, 255);
            public Color Glow { get; } = new Color32(173, 77, 74, 255);
            public Color Light { get; } = new Color32(136, 56, 57, 255);
            public Color Highlight { get; } = new Color32(173, 77, 74, 255);
            public Color Gradient { get; } = new Color32(125, 50, 51, 255);
            public Color Shadow { get; } = new Color32(99, 32, 34, 255);
            public bool Button01InnerVisible => true;
        }

        public sealed class CloseButtonColorSet : IColorSet
        {
            public Color Background { get; } = new Color32(90, 43, 44, 255);
            public Color InnerColor1 { get; } = new Color32(110, 41, 42, 255);
            public Color Glow { get; } = new Color32(189, 163, 137, 255);
            public Color Light { get; } = new Color32(110, 41, 42, 255);
            public Color Highlight { get; } = Color.clear;
            public Color Gradient { get; } = Color.clear;
            public Color Shadow { get; } = Color.clear;
            public bool Button01InnerVisible => true;
        }

        public class TraitTree
        {
            public static readonly Color DisableIconColor = new Color32(128, 128, 128, 128);
            public static readonly Color DisableLineColor = new Color32(71, 69, 66, 255);
            public static readonly Color EnableLineColor = new Color32(211, 169, 107, 255);
        }
    }
}
