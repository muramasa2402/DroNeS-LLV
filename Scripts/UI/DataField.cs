using System;
using TMPro;

namespace Drones.UI
{
    public class DataField : TextMeshProUGUI
    {
        public virtual void SetField(string v)
        {
            SetText(v);
        }
    }

}