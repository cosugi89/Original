using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Dialog
{
    internal sealed class DialogBackdropView : MonoBehaviour, IPointerClickHandler
    {
        private DialogHost _host;

        public void Initialize(DialogHost host)
        {
            _host = host;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _host.OnBackdropClicked();
        }
    }
}
