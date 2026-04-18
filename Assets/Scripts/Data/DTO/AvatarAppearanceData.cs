using System;
using System.Collections.Generic;
using Assets.Scripts.Core;
using UnityEngine;

namespace Assets.Scripts.Data.DTO
{
    /// <summary>
    /// Serializable avatar appearance data.
    /// </summary>
    [Serializable]
    public class AvatarAppearanceData
    {
        public List<PartsEntry> parts = new();
        public List<ColorEntry> colors = new();
        public List<VisibilityEntry> visibility = new();

        [Serializable]
        public class PartsEntry
        {
            public PartsType type;
            public int index;
        }

        [Serializable]
        public class ColorEntry
        {
            public ColorTargetType target;
            public Color color;
        }

        [Serializable]
        public class VisibilityEntry
        {
            public PartsType type;
            public bool visible;
        }
    }
}
