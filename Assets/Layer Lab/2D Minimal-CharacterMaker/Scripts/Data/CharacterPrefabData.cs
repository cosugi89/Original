using UnityEngine;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// Stores character customization data (parts, colors, visibility) on a prefab.
    /// On Start, applies the stored configuration to the associated PartsManager.
    /// </summary>
    public class CharacterPrefabData : MonoBehaviour
    {
        [SerializeField] private AvatarAppearanceData appearanceData = new();

        /// <summary>
        /// Sets the character data from avatar appearance data.
        /// </summary>
        /// <param name="data">The appearance data containing parts, colors, and visibility data.</param>
        public void SetData(AvatarAppearanceData data)
        {
            appearanceData = data ?? new AvatarAppearanceData();
        }

        private void Start()
        {
            var pm = GetComponent<PartsManager>();
            if (pm == null) pm = GetComponentInChildren<PartsManager>();
            if (pm == null) return;

            pm.Init();
            pm.ApplyAppearanceData(appearanceData);
        }
    }
}
