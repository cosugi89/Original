using System.Collections.Generic;

namespace Assets.Scripts.Systems.Save.Models
{
    /// <summary>ゲーム進捗とは独立したチュートリアルの完了と画面訪問の記録。</summary>
    public sealed class TutorialProgressData
    {
        public List<int> CompletedTutorialIds { get; set; } = new();
        public List<string> VisitedScopeKeys { get; set; } = new();
    }
}
