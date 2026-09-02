namespace Assets.Scripts.Core.Const
{
    public static class TextLocalization
    {
        public static string Get(string key)
        {
            return key ?? string.Empty;
        }
    }
}
