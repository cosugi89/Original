namespace Assets.Scripts.Systems.Scene
{
    /// <summary>
    /// Build Settings に登録されているシーン名を集約する定数クラス。
    /// SceneManager.LoadScene に文字列リテラルを散らさず、ここで一元管理する。
    /// </summary>
    public static class SceneNames
    {
        /// <summary>エントリポイント。アバター生成・HomeScreen 表示を担う。</summary>
        public const string Main = "MainScene";

        /// <summary>バトル画面。BattleSceneTransitionState 経由で StageId を受け取る。</summary>
        public const string Battle = "BattleScene";
    }
}
