namespace Assets.Scripts.Systems.Save
{
    /// <summary>
    /// UserDataManager への移行用の旧入口。
    /// 新規コードでは UserDataManager を直接使う。
    /// </summary>
    [System.Obsolete("Use UserDataManager instead.")]
    public static class GameSession
    {
        public static UserDataManager Instance => UserDataManager.Instance;
    }
}
