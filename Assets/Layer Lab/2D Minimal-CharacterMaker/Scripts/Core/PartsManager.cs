using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// キャラクターのパーツ（スプライト）を管理します。
    /// 装備、色の適用、プリセットのシリアライズなどを担当します。
    /// UIからの表示トグルは削除されていますが、
    /// 内部的な表示状態は関連パーツとの同期のために保持されています。
    /// </summary>
    public class PartsManager : MonoBehaviour
    {
        [SerializeField] private PartsCategory[] categories;
        [SerializeField] private Animator animator;

        [SerializeField] private SpriteRenderer[] skinRenderers;
        [SerializeField] private SpriteRenderer[] hairRenderers;
        [SerializeField] private SpriteRenderer[] eyeRenderers;
        [SerializeField] private SpriteRenderer[] beardRenderers;

        [Header("Themes")]
        [SerializeField] private List<ThemeType> selectedThemes = new();

        #region Properties

        /// <summary>
        /// PartsType ごとのアクティブな Sprite を index で管理 (-1 は未装備)
        /// </summary>
        public Dictionary<PartsType, int> ActiveIndices { get; private set; } = new();

        /// <summary>
        /// PartsType ごとの表示状態
        /// </summary>
        public Dictionary<PartsType, bool> Visibility { get; private set; } = new();

        /// <summary>
        /// ColorTargetType に設定された色 (対象: 肌, 髪, 目, ヒゲ)
        /// </summary>
        public Dictionary<ColorTargetType, Color> Colors { get; private set; } = new();

        /// <summary>
        /// Parts が装備または変更されたときに呼ばれるイベント
        /// </summary>
        public event Action<PartsType, int> OnPartsChanged;

        /// <summary>
        /// 色が変更されたときに呼ばれるイベント
        /// </summary>
        public event Action<ColorTargetType, Color> OnColorChanged;

        /// <summary>
        /// Parts ごとの情報を管理
        /// </summary>
        private Dictionary<PartsType, PartsCategory> categoryMap;

        #endregion Properties

        #region Initialization

        /// <summary>
        /// すべてのカテゴリ、インデックス、表示状態、色を初期化し、デフォルトの Sprite を適用する
        /// </summary>
        public void Init()
        {
            // Dictionary 初期化
            categoryMap = new Dictionary<PartsType, PartsCategory>();
            ActiveIndices.Clear();
            Visibility.Clear();
            Colors.Clear();

            if (categories == null) return;

            // 各カテゴリを登録し、デフォルトのインデックスと表示状態を設定
            foreach (var cat in categories)
            {
                categoryMap[cat.Type] = cat;
                ActiveIndices[cat.Type] = 0;
                Visibility[cat.Type] = cat.DefaultVisible;


                // MEMO: 初期設定値を管理できるようになれば不要になる想定
                if (cat.Type == PartsType.Beard && !cat.CanChangeColor)
                {
                    cat.CanChangeColor = true;
                    cat.ColorTarget = ColorTargetType.Beard;
                }

                // MEMO: 初期設定値を管理できるようになれば不要になる想定
                if (cat.Type == PartsType.Eye)
                    cat.CanChangeColor = false;
            }

            // 手の装備は 1つの装備中の PartsType を残して全て非表示にする
            foreach (PartsExclusiveGroup group in Enum.GetValues(typeof(PartsExclusiveGroup)))
            {
                if (group == PartsExclusiveGroup.None) continue;

                var members = GetExclusiveGroupMembers(group);
                for (int i = 0; i < members.Length; i++)
                {
                    Visibility[members[i].Type] = i == 0;
                }
            }

            // MEMO: 初期設定値を管理できるようになれば不要になる想定
            Colors[ColorTargetType.Skin] = Color.white;
            Colors[ColorTargetType.Hair] = Color.white;
            Colors[ColorTargetType.Eye] = Color.white;
            Colors[ColorTargetType.Beard] = Color.white;

            // Inspector で未設定の場合はカラー用レンダラーを自動検出
            AutoMapColorRenderers();

            // すべてのカテゴリに初期スプライトと表示状態を適用
            foreach (var cat in categories)
            {
                ApplySprites(cat, 0);

                // 表示状態に応じて GameObject を有効 / 無効化
                bool visible = Visibility.TryGetValue(cat.Type, out var v) && v;
                SetRenderersActive(cat, visible);
            }

            // Helmet と HelmetHair の初期連動
            SyncHelmetHairVisibility();

            // Bow / Crossbow と Arrow の初期連動
            SyncArrowVisibility();
        }

        /// <summary>
        /// 子オブジェクトから Skin、Hair、Eye、Beard 用の SpriteRenderer を自動検出して割り当てます。
        /// Inspector で未設定の場合のみ実行されます。
        /// </summary>
        private void AutoMapColorRenderers()
        {
            if (skinRenderers != null && skinRenderers.Length > 0 &&
                hairRenderers != null && hairRenderers.Length > 0 &&
                eyeRenderers != null && eyeRenderers.Length > 0 &&
                beardRenderers != null && beardRenderers.Length > 0)
                return;

            var allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

            if (skinRenderers == null || skinRenderers.Length == 0)
                skinRenderers = FindRenderersByName(allRenderers, "Body", "Head");
            if (hairRenderers == null || hairRenderers.Length == 0)
                hairRenderers = FindRenderersByName(allRenderers, "Hair", "Hair_Helmet");
            if (eyeRenderers == null || eyeRenderers.Length == 0)
                eyeRenderers = FindRenderersByName(allRenderers, "Eye");
            if (beardRenderers == null || beardRenderers.Length == 0)
                beardRenderers = FindRenderersByName(allRenderers, "Beard");
        }

        #endregion Initialization

        #region Parts Utilities

        /// <summary>指定した PartsType で利用可能な Sprite 数を返す</summary>
        public int GetPartsCount(PartsType type)
        {
            var cat = GetCategory(type);
            if (cat == null) return 0;
            return cat.SpriteCount > 0 ? cat.SpriteCount : cat.ThumbnailCount;
        }

        /// <summary>
        /// 指定した PartsType の Active な Sprite Index を返す
        /// なければ0 (未装備状態ではない)
        /// </summary>
        public int GetActiveIndex(PartsType type)
        {
            return ActiveIndices.TryGetValue(type, out var index) ? index : 0;
        }

        /// <summary>
        /// 指定した PartsType Index に対応する Thumbnail Sprite を返す
        /// なければ最初の Renderers のスプライトを返す
        /// </summary>
        public Sprite GetThumbnail(PartsType type, int index)
        {
            var cat = GetCategory(type);
            if (cat == null) return null;

            if (cat.Thumbnails != null && cat.Thumbnails.Length > 0)
            {
                if (index < 0 || index >= cat.Thumbnails.Length) return null;
                return cat.Thumbnails[index];
            }

            if (cat.Renderers == null || cat.Renderers.Length == 0) return null;
            var sprites = cat.Renderers[0].Sprites;
            if (sprites == null || index < 0 || index >= sprites.Length) return null;
            return sprites[index];
        }

        /// <summary>指定した PartsType が現在表示されているかどうか</summary>
        public bool IsPartsVisible(PartsType type)
        {
            return Visibility.TryGetValue(type, out var visible) && visible;
        }

        /// <summary>指定した PartsType が現在装備されているかどうか</summary>
        public bool IsEquipped(PartsType type)
        {
            return ActiveIndices.TryGetValue(type, out var index) && index >= 0;
        }

        /// <summary>
        /// 指定した PartsType に特定の Sprite Index を装備させる
        /// OnPartsChanged を発火させる
        /// </summary>
        public void EquipParts(PartsType type, int index)
        {
            var cat = GetCategory(type);
            if (cat == null) return;

            int count = GetPartsCount(type);
            if (count == 0) return;

            index = Mathf.Clamp(index, 0, count - 1);
            ActiveIndices[type] = index;
            ApplySprites(cat, index);

            // 以前に未装備だった場合は表示状態を復元する
            if (Visibility.TryGetValue(type, out var vis) && !vis)
            {
                Visibility[type] = true;
                SetRenderersActive(cat, true);
            }

            OnPartsChanged?.Invoke(type, index);

            // 右手武器が装備されたら Arrow の表示状態を同期する
            if (IsHandRightWeapon(type))
                SyncArrowVisibility();

            // Hair が変更されたら HelmetHair も同期する
            if (type == PartsType.Hair)
            {
                var helmetHairCat = GetCategory(PartsType.HelmetHair);
                if (helmetHairCat != null && index < helmetHairCat.SpriteCount)
                {
                    ActiveIndices[PartsType.HelmetHair] = index;
                    ApplySprites(helmetHairCat, index);
                }
            }

            // Helmet が装備されたら HelmetHair の表示状態を同期する
            if (type == PartsType.Helmet)
                SyncHelmetHairVisibility();
        }

        /// <summary>指定した PartsType を未装備状態にする</summary>
        public void UnequipParts(PartsType type)
        {
            var cat = GetCategory(type);
            if (cat == null) return;

            ActiveIndices[type] = -1;
            Visibility[type] = false;
            SetRenderersActive(cat, false);

            OnPartsChanged?.Invoke(type, -1);

            // Bow または Crossbow を外したときは Arrow を同期する
            if (type == PartsType.Bow || type == PartsType.Crossbow)
                SyncArrowVisibility();

            // Helmet を外したときは HelmetHair を同期する
            if (type == PartsType.Helmet)
                SyncHelmetHairVisibility();
        }

        /// <summary>UICategory 内で表示する PartsType を設定し、他を非表示にさせる</summary>
        public void SetGroupActiveType(UICategory category, PartsType visibleType)
        {
            var group = ToExclusiveGroup(category);
            if (group == PartsExclusiveGroup.None) return;

            var members = GetExclusiveGroupMembers(group);
            if (members.Length == 0) return;

            foreach (var cat in members)
            {
                bool isVisible = cat.Type == visibleType;
                Visibility[cat.Type] = isVisible;
                SetRenderersActive(cat, isVisible);
            }

            // 選ばれたパーツが未装備状態なら 0 を入れて最低限表示可能にする
            if (ActiveIndices.TryGetValue(visibleType, out int index) && index < 0)
            {
                EquipParts(visibleType, 0);
            }

            SyncArrowVisibility();
        }

        /// <summary>
        /// 指定した PartsType の次の Sprite を装備する
        /// 末尾まで行くと先頭に戻る
        // TODO: 削除予定の機能
        /// </summary>
        public void NextParts(PartsType type)
        {
            int count = GetPartsCount(type);
            if (count == 0) return;

            int current = GetActiveIndex(type);
            int next = (current + 1) % count;
            EquipParts(type, next);
        }

        /// <summary>
        /// 指定した PartsType の前の Sprite を装備する
        /// 先頭より前に行くと末尾に戻る
        // TODO: 削除予定の機能
        /// </summary>
        public void PrevParts(PartsType type)
        {
            int count = GetPartsCount(type);
            if (count == 0) return;

            int current = GetActiveIndex(type);
            int prev = (current - 1 + count) % count;
            EquipParts(type, prev);
        }

        /// <summary>指定したカラー対象の現在の色を返す</summary>
        public Color GetColor(ColorTargetType target)
        {
            return Colors.TryGetValue(target, out var color) ? color : Color.white;
        }

        /// <summary>
        /// 指定したカラー対象の色を設定し、対応する Renderer に適用する
        /// OnColorChangedを発火させる
        /// </summary>
        public void SetColor(ColorTargetType target, Color color)
        {
            Colors[target] = color;
            ApplyColor(target, color);
            OnColorChanged?.Invoke(target, color);
        }

        #endregion Parts Utilities

        #region Parts Helper

        /// <summary>
        /// 指定した名前のいずれかに一致する GameObject を持つ SpriteRenderer を検索します。
        /// </summary>
        private SpriteRenderer[] FindRenderersByName(SpriteRenderer[] allRenderers, params string[] names)
        {
            var result = new List<SpriteRenderer>();
            foreach (var sr in allRenderers)
            {
                foreach (var n in names)
                {
                    if (sr.gameObject.name == n)
                    {
                        result.Add(sr);
                        break;
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>PartsType に対応する PartsCategory を返す</summary>
        private PartsCategory GetCategory(PartsType type)
        {
            if (categoryMap == null || !categoryMap.TryGetValue(type, out var cat)) return null;
            return cat;
        }

        /// <summary>指定した色を、対応するカラー対象のすべての SpriteRenderer に適用する</summary>
        private void ApplyColor(ColorTargetType target, Color color)
        {
            SpriteRenderer[] renderers = target switch
            {
                ColorTargetType.Skin => skinRenderers,
                ColorTargetType.Hair => hairRenderers,
                ColorTargetType.Eye => eyeRenderers,
                ColorTargetType.Beard => beardRenderers,
                _ => null
            };

            if (renderers == null) return;

            foreach (var sr in renderers)
            {
                if (sr != null)
                    sr.color = color;
            }
        }

        /// <summary>指定した PartsType が右手武器（Sword、Axe、Bow など）かどうかを返す</summary>
        private static bool IsHandRightWeapon(PartsType type)
        {
            return type == PartsType.Sword || type == PartsType.Axe ||
                   type == PartsType.Bow || type == PartsType.Wand ||
                   type == PartsType.Staff || type == PartsType.Spear ||
                   type == PartsType.Blunt || type == PartsType.Crossbow;
        }

        /// <summary>
        /// Bow / Crossbow の表示状態に応じて Arrow / Bolt の表示を切り替えます。
        /// Arrow は Bow 表示時のみ、Bolt は Crossbow 表示時のみ表示されます。
        /// </summary>
        private void SyncArrowVisibility()
        {
            var arrowCat = GetCategory(PartsType.Arrow);
            if (arrowCat == null || arrowCat.Renderers == null) return;

            bool bowVisible = Visibility.TryGetValue(PartsType.Bow, out var bv) && bv;
            bool crossbowVisible = Visibility.TryGetValue(PartsType.Crossbow, out var cv) && cv;

            foreach (var pr in arrowCat.Renderers)
            {
                if (pr.Renderer == null) continue;
                string name = pr.Renderer.gameObject.name;

                if (name == "Bolt")
                    pr.Renderer.gameObject.SetActive(crossbowVisible);
                else
                    pr.Renderer.gameObject.SetActive(bowVisible);
            }

            bool showAny = bowVisible || crossbowVisible;
            Visibility[PartsType.Arrow] = showAny;
        }

        /// <summary>
        /// Helmet の表示状態と Hair の表示希望状態に応じて、
        /// HelmetHair と Hair の表示を切り替えます。
        /// Helmet が表示中で Hair を表示したい場合は Hair を隠し HelmetHair を表示します。
        /// Helmet が非表示なら Hair を表示し HelmetHair を隠します。
        /// Hair 自体が非表示なら両方とも隠します。
        /// </summary>
        private void SyncHelmetHairVisibility()
        {
            var helmetHairCat = GetCategory(PartsType.HelmetHair);
            if (helmetHairCat == null) return;

            var hairCat = GetCategory(PartsType.Hair);
            if (hairCat == null) return;

            bool hairWanted = Visibility.TryGetValue(PartsType.Hair, out var hv) && hv;
            bool helmetVisible = Visibility.TryGetValue(PartsType.Helmet, out var hmv) && hmv;

            if (hairWanted && helmetVisible)
            {
                // Helmet が ON かつ Hair を表示したい場合 → Hair を隠し HelmetHair を表示
                SetRenderersActive(hairCat, false);
                SetRenderersActive(helmetHairCat, true);
                Visibility[PartsType.HelmetHair] = true;
            }
            else if (hairWanted && !helmetVisible)
            {
                // Helmet が OFF かつ Hair を表示したい場合 → Hair を表示し HelmetHair を隠す
                SetRenderersActive(hairCat, true);
                SetRenderersActive(helmetHairCat, false);
                Visibility[PartsType.HelmetHair] = false;
            }
            else
            {
                // Hair を表示したくない場合 → 両方とも隠す
                SetRenderersActive(hairCat, false);
                SetRenderersActive(helmetHairCat, false);
                Visibility[PartsType.HelmetHair] = false;
            }
        }

        /// <summary>
        /// 指定したカテゴリに含まれるすべてのレンダラーの GameObject を有効 / 無効化します。
        /// </summary>
        private void SetRenderersActive(PartsCategory cat, bool active)
        {
            if (cat?.Renderers == null) return;
            foreach (var pr in cat.Renderers)
            {
                if (pr.Renderer != null)
                    pr.Renderer.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 指定したインデックスのスプライトを、カテゴリ内のすべてのレンダラーへ割り当てます。
        /// </summary>
        private void ApplySprites(PartsCategory cat, int index)
        {
            if (cat.Renderers == null) return;

            foreach (var pr in cat.Renderers)
            {
                if (pr.Renderer == null || pr.Sprites == null) continue;
                if (index < 0 || index >= pr.Sprites.Length) continue;
                pr.Renderer.sprite = pr.Sprites[index];
            }
        }

        #endregion Parts Helper

        #region Animation Utilities

        /// <summary>指定した Animation を再生させる</summary>
        // TODO: string でなく enum で管理するようにしたい
        public void PlayAnimation(string animName)
        {
            if (animator != null)
                animator.Play(animName);
        }

        /// <summary>現在再生中の Animation Clip 名を返す</summary>
        // TODO: string でなく enum で管理するようにしたい
        public string GetCurrentAnimation()
        {
            if (animator == null) return string.Empty;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
                return clipInfo[0].clip.name;

            return string.Empty;
        }

        /// <summary>Animator Controller 内で利用可能なすべての Animation Clip 名を返す</summary>
        // TODO: string でなく enum で管理するようにしたい
        public string[] GetAnimationNames()
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                return Array.Empty<string>();

            return animator.runtimeAnimatorController.animationClips
                .Select(clip => clip.name)
                .ToArray();
        }

        #endregion Animation Utilities

        /// <summary>
        /// すべてのパーツをランダムに装備します。
        /// </summary>
        public void RandomizeAll()
        {
            if (categories == null) return;

            // 排他的グループごとにランダムで 1 つの PartsType を選ぶ
            var groupPicks = new Dictionary<PartsExclusiveGroup, PartsType>();

            foreach (PartsExclusiveGroup group in Enum.GetValues(typeof(PartsExclusiveGroup)))
            {
                if (group == PartsExclusiveGroup.None) continue;

                var members = GetExclusiveGroupMembers(group);

                // スプライトを持つカテゴリのみ候補にする
                var candidates = new List<PartsType>();
                foreach (var cat in members)
                {
                    if (GetPartsCount(cat.Type) > 0)
                        candidates.Add(cat.Type);
                }

                if (candidates.Count > 0)
                    groupPicks[group] = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            }

            foreach (var cat in categories)
            {
                // HelmetHair と Arrow は自動同期されるためスキップ
                if (cat.Type == PartsType.HelmetHair || cat.Type == PartsType.Arrow)
                    continue;

                int count = GetPartsCount(cat.Type);
                if (count == 0)
                    continue;

                // Beard と Helmet は 50% の確率で未装備にする
                if (cat.Type == PartsType.Beard || cat.Type == PartsType.Helmet)
                {
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        UnequipParts(cat.Type);
                        continue;
                    }

                    // 以前の UnequipParts で非表示になっている可能性があるため復元する
                    if (Visibility.TryGetValue(cat.Type, out var visible) && !visible)
                    {
                        Visibility[cat.Type] = true;
                        SetRenderersActive(cat, true);
                    }
                }

                // 排他的グループ所属なら、選ばれたものだけ表示
                if (cat.ExclusiveGroup != PartsExclusiveGroup.None)
                {
                    bool picked = groupPicks.TryGetValue(cat.ExclusiveGroup, out var pickedType)
                        && pickedType == cat.Type;

                    Visibility[cat.Type] = picked;
                    SetRenderersActive(cat, picked);

                    if (picked)
                        EquipParts(cat.Type, UnityEngine.Random.Range(0, count));

                    continue;
                }

                // 通常カテゴリはそのままランダム装備
                EquipParts(cat.Type, UnityEngine.Random.Range(0, count));
            }

            // 連動パーツの表示を同期
            SyncHelmetHairVisibility();
            SyncArrowVisibility();

            // 色もランダム化する（Skin、Hair、Beard）
            RandomizeColors();
        }

        /// <summary>
        /// カラー対象の色をランダム化します。
        /// Eye は対象外です。
        /// </summary>
        private void RandomizeColors()
        {
            foreach (ColorTargetType target in Enum.GetValues(typeof(ColorTargetType)))
            {
                if (target == ColorTargetType.Eye) continue;

                float h = UnityEngine.Random.Range(0f, 1f);
                float s = UnityEngine.Random.Range(0.4f, 1f);
                float v = UnityEngine.Random.Range(0.5f, 1f);
                SetColor(target, Color.HSVToRGB(h, s, v));
            }
        }

        /// <summary>
        /// 別の PartsManager から、すべてのパーツインデックス、表示状態、色をコピーします。
        /// </summary>
        /// <param name="other">コピー元の PartsManager。</param>
        public void CopyFrom(PartsManager other)
        {
            if (other == null) return;

            foreach (var kvp in other.ActiveIndices)
            {
                if (kvp.Value < 0)
                    UnequipParts(kvp.Key);
                else
                    EquipParts(kvp.Key, kvp.Value);
            }

            foreach (var kvp in other.Visibility)
            {
                var cat = GetCategory(kvp.Key);
                if (cat == null) continue;

                Visibility[kvp.Key] = kvp.Value;
                SetRenderersActive(cat, kvp.Value);
            }

            foreach (var kvp in other.Colors)
                SetColor(kvp.Key, kvp.Value);

            SyncArrowVisibility();
            SyncHelmetHairVisibility();
        }

        /// <summary>
        /// 保存済みのプリセットをこのキャラクターに適用し、
        /// パーツ、色、表示状態を復元します。
        /// </summary>
        /// <param name="item">適用するプリセットデータ。</param>
        public void ApplyPresetItem(PresetData.PresetItem item)
        {
            if (item == null || item.isEmpty) return;

            foreach (var entry in item.parts)
            {
                if (entry.index < 0)
                    UnequipParts(entry.type);
                else
                    EquipParts(entry.type, entry.index);
            }

            foreach (var entry in item.colors)
                SetColor(entry.target, entry.color);

            foreach (var entry in item.visibility)
            {
                // Arrow と HelmetHair は自動同期されるため、プリセットから直接設定しない
                if (entry.type == PartsType.Arrow || entry.type == PartsType.HelmetHair)
                    continue;

                var cat = GetCategory(entry.type);
                if (cat == null) continue;
                Visibility[entry.type] = entry.visible;
                SetRenderersActive(cat, entry.visible);
            }

            // プリセット適用後に同期を再実行
            SyncArrowVisibility();
            SyncHelmetHairVisibility();
        }

        /// <summary>
        /// 現在のキャラクター状態をプリセットデータとしてシリアライズします。
        /// </summary>
        /// <returns>現在の状態を格納した新しい <see cref="PresetData.PresetItem"/>。</returns>
        public PresetData.PresetItem ToPresetItem()
        {
            var item = new PresetData.PresetItem { isEmpty = false };

            foreach (var kvp in ActiveIndices)
                item.parts.Add(new PresetData.PartsEntry { type = kvp.Key, index = kvp.Value });

            foreach (var kvp in Colors)
                item.colors.Add(new PresetData.ColorEntry { target = kvp.Key, color = kvp.Value });

            foreach (var kvp in Visibility)
                item.visibility.Add(new PresetData.VisibilityEntry { type = kvp.Key, visible = kvp.Value });

            return item;
        }

        #region Category Accessors

        /// <summary>
        /// categories 配列に登録されているすべてのパーツタイプを返します。
        /// </summary>
        /// <returns><see cref="PartsType"/> の配列。</returns>
        public PartsType[] GetAllPartsTypes()
        {
            if (categories == null) return Array.Empty<PartsType>();
            return categories.Select(c => c.Type).ToArray();
        }

        /// <summary>
        /// 指定したパーツタイプがカラー変更に対応しているかを返します。
        /// </summary>
        /// <param name="type">確認対象のパーツタイプ。</param>
        /// <returns>カラー変更可能なら true。</returns>
        public bool CanChangeColor(PartsType type)
        {
            var cat = GetCategory(type);
            return cat != null && cat.CanChangeColor;
        }

        /// <summary>
        /// 指定したパーツタイプに対応するカラー対象タイプを返します。
        /// </summary>
        /// <param name="type">取得対象のパーツタイプ。</param>
        /// <returns>対応する <see cref="ColorTargetType"/>。見つからない場合は Skin。</returns>
        public ColorTargetType GetColorTarget(PartsType type)
        {
            var cat = GetCategory(type);
            return cat?.ColorTarget ?? ColorTargetType.Skin;
        }

        /// <summary>
        /// 指定したパーツタイプの表示名を返します。
        /// </summary>
        /// <param name="type">取得対象のパーツタイプ。</param>
        /// <returns>表示名。未設定の場合は enum 名。</returns>
        public string GetDisplayName(PartsType type)
        {
            var cat = GetCategory(type);
            return cat?.DisplayName ?? type.ToString();
        }

        #endregion

        private PartsType[] GetPartsTypesByCategory(UICategory category)
        {
            if (categories == null) return Array.Empty<PartsType>();

            return categories
                .Where(c => c.UICategory == category)
                .Select(c => c.Type)
                .ToArray();
        }

        private bool IsGroupCategory(UICategory category)
        {
            return GetPartsTypesByCategory(category).Length > 1;
        }

        private PartsCategory[] GetExclusiveGroupMembers(PartsExclusiveGroup group)
        {
            if (categories == null || group == PartsExclusiveGroup.None)
                return Array.Empty<PartsCategory>();

            return categories
                .Where(c => c.ExclusiveGroup == group)
                .ToArray();
        }

        private PartsExclusiveGroup ToExclusiveGroup(UICategory category)
        {
            return category switch
            {
                UICategory.HandRight => PartsExclusiveGroup.HandRight,
                UICategory.HandLeft => PartsExclusiveGroup.HandLeft,
                _ => PartsExclusiveGroup.None
            };
        }

        private bool IsExclusivePartsType(PartsType type)
        {
            var cat = GetCategory(type);
            return cat != null && cat.ExclusiveGroup != PartsExclusiveGroup.None;
        }
    }
}
