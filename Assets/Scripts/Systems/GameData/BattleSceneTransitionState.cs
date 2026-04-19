using System.ComponentModel;
using Unity.VisualScripting;

namespace Assets.Scripts.Systems.GameData
{
    public static class BattleSceneTransitionState
    {
        [Description("選択されたステージID")]
        public static int SelectedStageId { get; set; }

        /// <summary>値を取り出してリセットする（一度だけ使う用途）</summary>
        public static int ConsumeSelectedStageId()
        {
            var result = SelectedStageId;
            SelectedStageId = default;
            return result;
        }
    }
}
