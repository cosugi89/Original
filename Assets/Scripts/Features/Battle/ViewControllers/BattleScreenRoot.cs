using UnityEngine;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 戦闘画面全体の領域と主要 View 参照を束ねる。
    /// </summary>
    public sealed class BattleScreenRoot : MonoBehaviour
    {
        [Header("Portrait Layout")]
        [SerializeField] private bool applyPortraitLayout = true;
        [SerializeField, Range(0f, 0.15f)] private float bottomSafeAreaRatio = 0.02f;
        [SerializeField, Range(0.35f, 0.65f)] private float boardHeightRatio = 0.52f;
        [SerializeField, Range(0.08f, 0.18f)] private float commandHeightRatio = 0.12f;
        [SerializeField, Range(0.05f, 0.12f)] private float topHudHeightRatio = 0.07f;
        [SerializeField, Min(0f)] private float horizontalMargin = 32f;
        [SerializeField, Min(0f)] private float verticalGap = 8f;

        [Header("Layout Roots")]
        [SerializeField] private RectTransform topHudRoot;
        [SerializeField] private RectTransform presentationRoot;
        [SerializeField] private RectTransform infoCommandRoot;
        [SerializeField] private RectTransform boardAreaRoot;
        [SerializeField] private RectTransform overlayRoot;

        [SerializeField] private BattleTopHudView topHudView;
        [SerializeField] private BattlePresentationView presentationView;
        [SerializeField] private EnemyAreaView enemyAreaView;
        [SerializeField] private EnemyUnitView enemyUnitView;
        [SerializeField] private PlayerAreaView playerAreaView;
        [SerializeField] private SkillPaletteView skillPaletteView;
        [SerializeField] private BattleBoardView boardView;
        [SerializeField] private BattleEventNoticeView eventNoticeView;
        [SerializeField] private BattleResultView resultOverlayView;

        private bool _isApplyingLayout;

        public BattleTopHudView TopHudView
        {
            get
            {
                ResolveMissingReferences();
                return topHudView;
            }
        }

        public BattlePresentationView PresentationView
        {
            get
            {
                ResolveMissingReferences();
                return presentationView;
            }
        }

        public EnemyAreaView EnemyAreaView
        {
            get
            {
                ResolveMissingReferences();
                return enemyAreaView;
            }
        }

        public EnemyUnitView EnemyUnitView
        {
            get
            {
                ResolveMissingReferences();
                return enemyUnitView;
            }
        }

        public PlayerAreaView PlayerAreaView
        {
            get
            {
                ResolveMissingReferences();
                return playerAreaView;
            }
        }

        public SkillPaletteView SkillPaletteView
        {
            get
            {
                ResolveMissingReferences();
                return skillPaletteView;
            }
        }

        public BattleBoardView BoardView
        {
            get
            {
                ResolveMissingReferences();
                return boardView;
            }
        }

        public BattleEventNoticeView EventNoticeView
        {
            get
            {
                ResolveMissingReferences();
                return eventNoticeView;
            }
        }

        public BattleResultView ResultOverlayView
        {
            get
            {
                ResolveMissingReferences();
                return resultOverlayView;
            }
        }

        private void Awake()
        {
            ResolveMissingReferences();
            ApplyPortraitLayout();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (Application.isPlaying)
            {
                ApplyPortraitLayout();
            }
        }

        public void ResolveMissingReferences()
        {
            if (topHudView == null)
            {
                topHudView = GetComponentInChildren<BattleTopHudView>(true);
            }

            if (presentationView == null)
            {
                presentationView = GetComponentInChildren<BattlePresentationView>(true);
            }

            if (enemyAreaView == null)
            {
                enemyAreaView = GetComponentInChildren<EnemyAreaView>(true);
            }

            if (enemyUnitView == null)
            {
                enemyUnitView = GetComponentInChildren<EnemyUnitView>(true);
            }

            if (playerAreaView == null)
            {
                playerAreaView = GetComponentInChildren<PlayerAreaView>(true);
            }

            if (skillPaletteView == null)
            {
                skillPaletteView = GetComponentInChildren<SkillPaletteView>(true);
            }

            if (boardView == null)
            {
                boardView = GetComponentInChildren<BattleBoardView>(true);
            }

            if (eventNoticeView == null)
            {
                eventNoticeView = GetComponentInChildren<BattleEventNoticeView>(true);
            }

            if (resultOverlayView == null)
            {
                resultOverlayView = GetComponentInChildren<BattleResultView>(true);
            }

            ResolveLayoutReferences();
        }

        public void ApplyPortraitLayout()
        {
            if (!applyPortraitLayout || _isApplyingLayout)
            {
                return;
            }

            try
            {
                _isApplyingLayout = true;
                ResolveMissingReferences();

                var boardMinY = bottomSafeAreaRatio;
                var boardMaxY = Mathf.Clamp01(boardMinY + boardHeightRatio);
                var commandMinY = boardMaxY;
                var commandMaxY = Mathf.Clamp01(commandMinY + commandHeightRatio);
                var topHudMinY = Mathf.Clamp01(1f - topHudHeightRatio);
                var presentationMinY = commandMaxY;
                var presentationMaxY = Mathf.Max(presentationMinY, topHudMinY);

                ApplyBand(boardAreaRoot, boardMinY, boardMaxY, horizontalMargin, verticalGap);
                ApplyBand(infoCommandRoot, commandMinY, commandMaxY, horizontalMargin, verticalGap);
                ApplyBand(presentationRoot, presentationMinY, presentationMaxY, horizontalMargin, verticalGap);
                ApplyBand(topHudRoot, topHudMinY, 1f, horizontalMargin, verticalGap);
                ApplyBand(overlayRoot, 0f, 1f, 0f, 0f);

                ApplyLegacyBand("DamageFeedbackPanel", presentationMinY, presentationMaxY, horizontalMargin, verticalGap);
                ApplyLegacyBand("NarrativePanel", presentationMinY, presentationMaxY, horizontalMargin, verticalGap);
                ApplyLegacyBand("EnemyBattleTypePanel", presentationMinY, presentationMaxY, horizontalMargin, verticalGap);
                ApplyFullParent(boardView != null ? boardView.transform as RectTransform : null);
                ApplyFullParent(FindChildRect("TraceLineLayer"));
            }
            finally
            {
                _isApplyingLayout = false;
            }
        }

        private void ResolveLayoutReferences()
        {
            if (topHudRoot == null)
            {
                topHudRoot = topHudView != null ? topHudView.transform as RectTransform : FindChildRect("HudPanel");
            }

            if (presentationRoot == null)
            {
                presentationRoot = presentationView != null ? presentationView.transform as RectTransform : FindChildRect("Middle");
            }

            if (infoCommandRoot == null)
            {
                infoCommandRoot = skillPaletteView != null ? skillPaletteView.transform as RectTransform : FindChildRect("SkillSlotArea");
            }

            if (boardAreaRoot == null)
            {
                boardAreaRoot = boardView != null && boardView.transform.parent != null
                    ? boardView.transform.parent as RectTransform
                    : FindChildRect("BoardArea");
            }

            if (overlayRoot == null)
            {
                overlayRoot = resultOverlayView != null ? resultOverlayView.transform as RectTransform : FindChildRect("ResultPanel");
            }
        }

        private RectTransform FindChildRect(string childName)
        {
            if (string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            var child = FindChildRecursive(transform, childName);
            return child != null ? child as RectTransform : null;
        }

        private void ApplyLegacyBand(string childName, float minY, float maxY, float marginX, float gapY)
        {
            var target = FindChildRect(childName);
            if (target == null || target.parent != transform)
            {
                return;
            }

            ApplyBand(target, minY, maxY, marginX, gapY);
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (child != null && child.name == childName)
                {
                    return child;
                }

                var found = FindChildRecursive(child, childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void ApplyBand(RectTransform target, float minY, float maxY, float marginX, float gapY)
        {
            if (target == null)
            {
                return;
            }

            target.anchorMin = new Vector2(0f, Mathf.Clamp01(minY));
            target.anchorMax = new Vector2(1f, Mathf.Clamp01(maxY));
            target.pivot = new Vector2(0.5f, 0.5f);
            target.offsetMin = new Vector2(marginX, gapY);
            target.offsetMax = new Vector2(-marginX, -gapY);
            target.localScale = Vector3.one;
            target.localRotation = Quaternion.identity;
        }

        private static void ApplyFullParent(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.pivot = new Vector2(0.5f, 0.5f);
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
            target.localScale = Vector3.one;
            target.localRotation = Quaternion.identity;
        }
    }
}
