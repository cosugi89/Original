using System;

namespace Assets.Scripts.UI.Dialog
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class DialogAddressAttribute : Attribute
    {
        public DialogAddressAttribute(string address)
        {
            Address = address;
        }

        public string Address { get; }
    }
}
