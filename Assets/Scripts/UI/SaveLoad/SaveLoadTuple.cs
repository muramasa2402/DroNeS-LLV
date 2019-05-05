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
            });
        }

    }
}
