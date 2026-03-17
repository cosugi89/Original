using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Dialog
{
    public sealed class DialogBackgroundView : MonoBehaviour, IPointerClickHandler
    {
        private System.Action _onClick;

        public void Initialize(System.Action onClick)
        {
            _onClick = onClick;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick?.Invoke();
        }
    }
}
