using System.Collections;
using Assets.Scripts.Core.Const;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Components
{
    public class PartsTraitLine : MonoBehaviour
    {
        [SerializeField] private Image centerLine;
        [SerializeField] private Image leftLine;
        [SerializeField] private Image rightLine;
        [Header("Reveal Animation")]
        [SerializeField, Min(0f)] private float enableAnimationDuration = 0.3f;
        [SerializeField] private AnimationCurve enableAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private static readonly int RevealProgressId = Shader.PropertyToID("_RevealProgress");
        private static readonly int DisabledColorId = Shader.PropertyToID("_DisabledColor");
        private static readonly int EnabledColorId = Shader.PropertyToID("_EnabledColor");

        private Material _centerMaterial;
        private Material _leftMaterial;
        private Material _rightMaterial;
        private Coroutine _centerAnimationCoroutine;
        private Coroutine _leftAnimationCoroutine;
        private Coroutine _rightAnimationCoroutine;

        private void Awake()
        {
            _centerMaterial = CreateRuntimeMaterial(centerLine);
            _leftMaterial = CreateRuntimeMaterial(leftLine);
            _rightMaterial = CreateRuntimeMaterial(rightLine);
        }

        public void SetupCenter(bool isEnabled)
        {
            SetupLine(centerLine, _centerMaterial, ref _centerAnimationCoroutine, isEnabled);
        }

        public void SetupLeft(bool isEnabled)
        {
            SetupLine(leftLine, _leftMaterial, ref _leftAnimationCoroutine, isEnabled);
        }

        public void SetupRight(bool isEnabled)
        {
            SetupLine(rightLine, _rightMaterial, ref _rightAnimationCoroutine, isEnabled);
        }

        public void PlayCenterEnableAnimation()
        {
            PlayEnableAnimation(centerLine, _centerMaterial, ref _centerAnimationCoroutine);
        }

        public void PlayLeftEnableAnimation()
        {
            PlayEnableAnimation(leftLine, _leftMaterial, ref _leftAnimationCoroutine);
        }

        public void PlayRightEnableAnimation()
        {
            PlayEnableAnimation(rightLine, _rightMaterial, ref _rightAnimationCoroutine);
        }

        private Material CreateRuntimeMaterial(Image line)
        {
            if (line == null)
            {
                return null;
            }

            var runtimeMaterial = new Material(line.material);
            runtimeMaterial.SetColor(DisabledColorId, UIColors.TraitTree.DisableLineColor);
            runtimeMaterial.SetColor(EnabledColorId, UIColors.TraitTree.EnableLineColor);
            line.material = runtimeMaterial;
            line.color = UIColors.White;
            return runtimeMaterial;
        }

        private void SetupLine(Image line, Material material, ref Coroutine animationCoroutine, bool isEnabled)
        {
            if (line == null)
            {
                return;
            }

            StopEnableAnimation(ref animationCoroutine);
            line.gameObject.SetActive(true);
            if (isEnabled)
            {
                line.transform.SetAsLastSibling();
            }

            material.SetFloat(RevealProgressId, isEnabled ? 1f : -0.001f);
        }

        private void PlayEnableAnimation(Image line, Material material, ref Coroutine animationCoroutine)
        {
            if (line == null)
            {
                return;
            }

            StopEnableAnimation(ref animationCoroutine);
            line.gameObject.SetActive(true);
            line.transform.SetAsLastSibling();
            material.SetFloat(RevealProgressId, -0.001f);
            animationCoroutine = StartCoroutine(PlayEnableAnimationRoutine(material));
        }

        private IEnumerator PlayEnableAnimationRoutine(Material material)
        {
            if (enableAnimationDuration <= 0f)
            {
                material.SetFloat(RevealProgressId, 1f);
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < enableAnimationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var normalizedTime = Mathf.Clamp01(elapsed / enableAnimationDuration);
                material.SetFloat(RevealProgressId, enableAnimationCurve.Evaluate(normalizedTime));
                yield return null;
            }

            material.SetFloat(RevealProgressId, 1f);
        }

        private void StopEnableAnimation(ref Coroutine animationCoroutine)
        {
            if (animationCoroutine == null)
            {
                return;
            }

            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        private void OnDestroy()
        {
            Destroy(_centerMaterial);
            Destroy(_leftMaterial);
            Destroy(_rightMaterial);
        }
    }
}
