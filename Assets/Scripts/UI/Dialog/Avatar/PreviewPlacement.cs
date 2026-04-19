using UnityEngine;

namespace Assets.Scripts.UI.Dialog
{
    /// <summary>
    /// AvatarPreviewRenderer 上に生成するプレビューの配置情報。
    /// previewRoot のローカル空間での位置・回転・スケールを指定する。
    /// </summary>
    public readonly struct PreviewPlacement
    {
        public Vector3 LocalPosition { get; }
        public Quaternion LocalRotation { get; }
        public Vector3 LocalScale { get; }

        public PreviewPlacement(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            LocalPosition = localPosition;
            LocalRotation = localRotation;
            LocalScale = localScale;
        }

        /// <summary>原点・無回転・等倍。placement を省略した場合の既定値。</summary>
        public static PreviewPlacement Default =>
            new(Vector3.zero, Quaternion.identity, Vector3.one);

        /// <summary>位置と一様スケールだけ指定したい場合の簡易コンストラクタ。</summary>
        public static PreviewPlacement At(Vector3 localPosition, float uniformScale = 1f) =>
            new(localPosition, Quaternion.identity, Vector3.one * uniformScale);

        /// <summary>位置と3軸スケールを指定。</summary>
        public static PreviewPlacement At(Vector3 localPosition, Vector3 localScale) =>
            new(localPosition, Quaternion.identity, localScale);
    }
}
