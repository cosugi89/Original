using System;
using UnityEngine;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// Defines a character parts category containing sprite renderers, thumbnails,
    /// and configuration for toggling and color changes.
    /// </summary>
    [Serializable]
    public class PartsCategory
    {
        /// <summary>The parts type this category represents.</summary>
        public PartsType type;

        /// <summary>The human-readable display name shown in the UI.</summary>
        public string displayName;

        /// <summary>Array of part renderers, each mapping a SpriteRenderer to its available sprites.</summary>
        public PartRenderer[] renderers;

        /// <summary>Whether this category can be toggled on/off (equipped/unequipped).</summary>
        public bool canToggle = true;

        /// <summary>Whether this category supports color customization.</summary>
        public bool canChangeColor;

        /// <summary>The color target type used when applying color changes.</summary>
        public ColorTargetType colorTarget;

        /// <summary>Whether this category is shared across all themes.</summary>
        public bool isCommon;

        /// <summary>Optional dedicated thumbnail sprites for the parts list UI.</summary>
        public Sprite[] thumbnails;

        /// <summary>
        /// Returns the number of available sprites from the first renderer.
        /// Returns 0 if renderers or sprites are null or empty.
        /// </summary>
        public int SpriteCount => renderers != null && renderers.Length > 0 && renderers[0].sprites != null
            ? renderers[0].sprites.Length
            : 0;

        /// <summary>
        /// Returns the number of available thumbnail sprites.
        /// </summary>
        public int ThumbnailCount => thumbnails != null ? thumbnails.Length : 0;
    }

    /// <summary>
    /// Maps a <see cref="SpriteRenderer"/> to an array of interchangeable sprites
    /// representing different visual options for a character part.
    /// </summary>
    [Serializable]
    public class PartRenderer
    {
        /// <summary>The target SpriteRenderer component on the character.</summary>
        public SpriteRenderer renderer;

        /// <summary>Array of sprite options that can be assigned to the renderer.</summary>
        public Sprite[] sprites;
    }
}
