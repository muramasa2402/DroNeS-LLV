using UnityEngine;

namespace Drones.UI
{
    using Utils.Extensions;
    public class VariableBarField : DataField
    {
        public float Value { get; private set; }

        [SerializeField]
        private RectTransform _Bar;
        private RectTransform Bar
        {
            get
            {
                if (_Bar == null)
                {
                    _Bar = Mask.GetChild(0).ToRect();
                }
                return _Bar;
            }
        }
        [SerializeField]
        private RectTransform _Mask;
        private RectTransform Mask
        {
            get
            {
                if (_Mask == null)
                {
                    _Mask = transform.GetChild(0).ToRect();
                }
                return _Mask;
            }
        }

        private Vector2 _OriginalOffset;

        protected override void Awake()
        {
            base.Awake();
            _OriginalOffset = Mask.offsetMax;
            color = new Color(1, 1, 1, 0);
            fontSize = 1;
        }

        // String MUST BE in fraction, i.e. 95% v = "0.95"
        public override void SetField(string v)
        {
            Value = float.Parse(v);

            SetText(v);

            SetLength();
        }

        private void SetLength()
        {
            float x = _OriginalOffset.x - (1 - Value) * Bar.sizeDelta.x;

            Vector2 size = new Vector2(x, _OriginalOffset.y);

            Mask.offsetMax = size;
        }
    }
}


