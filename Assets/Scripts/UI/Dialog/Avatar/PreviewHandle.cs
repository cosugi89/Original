using UnityEngine;

namespace Assets.Scripts.UI.Dialog
{
    /// <summary>
    /// AvatarPreviewRenderer が払い出すスポーン結果のハンドル。
    /// 呼び出し側はこのハンドルを保持することで、該当プレビューの参照取得・個別破棄が行える。
    /// </summary>
    public sealed class PreviewHandle
    {
        internal PreviewHandle(string id, GameObject instance)
        {
            Id = id;
            Instance = instance;
        }

        /// <summary>スポーンごとに一意な識別子。</summary>
        public string Id { get; }

        /// <summary>生成された GameObject。破棄されると null になる。</summary>
        public GameObject Instance { get; internal set; }

        /// <summary>インスタンスが生存しているか（null or Destroyed で false）。</summary>
        public bool IsAlive => Instance != null;

        /// <summary>
        /// スポーンしたインスタンスから型付きで Component を取得する。
        /// 代表例: handle.Get&lt;PartsManager&gt;()
        /// </summary>
        public T Get<T>() where T : Component
        {
            return Instance != null ? Instance.GetComponent<T>() : null;
        }
    }
}
