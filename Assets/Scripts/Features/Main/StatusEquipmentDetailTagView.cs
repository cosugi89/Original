using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    public sealed class StatusEquipmentDetailTagView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;

        public void Setup(string tag)
        {
            labelText.text = tag;
        }
    }
}
