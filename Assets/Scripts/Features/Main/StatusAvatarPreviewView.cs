using System.Collections.Generic;
using Assets.Scripts.Core;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.UI.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusAvatarPreviewView : MonoBehaviour
    {
        [SerializeField] private RawImage previewImage;

        private readonly List<AppearanceController> _targets = new();
        private GameObject _primaryRoot;

        public AppearanceController PrimaryTarget => _targets[0];

        public void Initialize(
            AppearanceController primaryTarget,
            IReadOnlyList<AppearanceController> additionalTargets,
            PreviewStage previewStage)
        {
            _targets.Clear();
            _targets.Add(primaryTarget);
            _primaryRoot = primaryTarget.transform.parent.gameObject;

            for (var i = 0; i < additionalTargets.Count; i++)
            {
                if (!_targets.Contains(additionalTargets[i]))
                {
                    _targets.Add(additionalTargets[i]);
                }
            }

            var previewCamera = previewStage.GetComponentInChildren<Camera>(true);
            previewImage.texture = previewCamera.targetTexture;
            previewImage.color = Color.white;
            previewImage.raycastTarget = false;
            ApplySavedAppearance();
        }

        public AppearanceController[] GetTargets()
        {
            return _targets.ToArray();
        }

        public void ApplySavedAppearance()
        {
            var renderService = AppearanceAdapter.EnsureInitialized();
            for (var i = 0; i < _targets.Count; i++)
            {
                renderService.ApplyTo(_targets[i]);
            }
        }

        public void SetPrimaryVisible(bool visible)
        {
            if (_primaryRoot.activeSelf != visible)
            {
                _primaryRoot.SetActive(visible);
            }

            if (previewImage.gameObject.activeSelf != visible)
            {
                previewImage.gameObject.SetActive(visible);
            }
        }
    }
}
