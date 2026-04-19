using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Stage Master Data", fileName = "BattleStageMasterData")]
    public class BattleStageMasterData : ScriptableObject
    {
        [field: SerializeField]
        [Description("ステージを一意に識別するID。保存データから参照されるキー。")]
        public string StageId { get; private set; } = "";

        [field: SerializeField]
        [Description("画面表示用のステージ名。")]
        public string DisplayName { get; private set; } = "";

        [field: SerializeField]
        [Description("ステージ選択画面などで補足表示に使う説明文。")]
        public string DescriptionText { get; private set; } = "";

        [field: SerializeField]
        [Description("一覧表示の並び順。数値が小さいほど前に出す。")]
        public int SortOrder { get; private set; }

        [field: SerializeField]
        [Description("新規セーブ時に最初から解放されているステージかどうか。")]
        public bool IsInitiallyUnlocked { get; private set; }

        [field: SerializeField]
        [Description("ステージボタンや詳細表示で使うプレビュー画像。")]
        public Sprite PreviewImage { get; private set; }
    }
}
