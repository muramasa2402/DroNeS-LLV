using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Drones.UI
{
    public class SaveLoadTuple : AbstractListElement
    {
        [SerializeField]
        private Button _Paster;
        private DataField[] _Data;
        private const float _ClickDelta = 0.35f;
        private bool _FirstClick;
        private float _ClickTime;

        public DataField[] Data
        {
            get
            {
                if (_Data == null)
                {
                    _Data = GetComponentsInChildren<DataField>();
                }
                return _Data;
            }
        }

        private Button Paster
        {
            get
            {
                if (_Paster == null)
                {
                    _Paster = GetComponent<Button>();
                }
                return _Paster;
            }
        }

        private void Awake()
        {
            Paster.onClick.AddListener(delegate
            {
                SaveLoadWindow.Instance.SetSaveName(Data[0].text);
                if (_FirstClick && Time.unscaledTime - _ClickTime > _ClickDelta)
                {
                    _FirstClick = false;
                }

                if (_FirstClick && Time.unscaledTime - _ClickTime <= _ClickDelta)
                {
                    SaveLoadWindow.Instance.OperateButton.onClick.Invoke();
                    _FirstClick = false;
                }
                else
                {
                    _FirstClick = true;
                    _ClickTime = Time.unscaledTime;
                }
            });
        }

    }
}
