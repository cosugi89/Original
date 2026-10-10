using System.Collections.Generic;
using System.ComponentModel;

namespace Assets.Scripts.Systems.Save.Models
{
    public enum BossResolution
    {
        Unresolved = 0,
        Defeated = 1,
        Redeemed = 2,
    }

    public sealed class CycleProgressData
    {
        [Description("Resurgence 中かどうか。Resurgence 中だけ Remnant タブを表示する。")]
        public bool IsResurgence { get; set; }

        [Description("上位存在へ一度でも到達したかどうか。Remnant からの救済再挑戦の条件に使う。")]
        public bool HasReachedHigherBeing { get; set; }

        [Description("人間ボスごとの当該周回における結末と Insight の既読状態。")]
        public List<BossCycleResultData> BossResults { get; set; } = new();
    }

    public sealed class BossCycleResultData
    {
        [Description("BossInsightMasterData の StageId。")]
        public int StageId { get; set; }

        [Description("未遭遇、通常撃破、救済のいずれか。")]
        public BossResolution Resolution { get; set; }

        [Description("解放済み Insight を Progress 画面で閲覧済みかどうか。")]
        public bool IsInsightViewed { get; set; }
    }
}
