using System.Collections.Generic;

namespace Assets.Scripts.Features.Main.Settings
{
    /// <summary>
    /// Settings 内の Credits ページに表示する外部素材のクレジットを管理する。
    /// 実際の表示UIは後続実装でこの一覧を参照する。
    /// </summary>
    public sealed class CreditsPage
    {
        private static readonly CreditEntry[] EntriesInternal =
        {
            new(
                "Game Engine",
                "Unity",
                "Unity Technologies",
                "https://unity.com/",
                "ゲームエンジンとして使用。"),
            new(
                "Visual Assets",
                "Character Assets / UI Dark Theme",
                "Layer Lab",
                string.Empty,
                "キャラクター素材およびUIダークテーマに使用。"),
            new(
                "Music",
                "MUSEUM",
                "木原眼鏡 / STUDIO AGASHA",
                "https://studio-agasha.booth.pm/items/1683919",
                "購入済みBGM素材。商用利用可。著作権は作者に帰属します。"),
        };

        public IReadOnlyList<CreditEntry> Entries => EntriesInternal;

        public readonly struct CreditEntry
        {
            public CreditEntry(
                string category,
                string workTitle,
                string creator,
                string sourceUrl,
                string notice)
            {
                Category = category;
                WorkTitle = workTitle;
                Creator = creator;
                SourceUrl = sourceUrl;
                Notice = notice;
            }

            public string Category { get; }
            public string WorkTitle { get; }
            public string Creator { get; }
            public string SourceUrl { get; }
            public string Notice { get; }
        }
    }
}
