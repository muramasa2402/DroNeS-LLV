using Drones.Utils.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Drones.StartScreen
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        Button _Play;
        [SerializeField]
        Button _Options;
        [SerializeField]
        Button _Quit;

        public Button Play
        {
            get
            {
                if (_Play == null)
                {
                    _Play = transform.FindDescendent("Play").GetComponent<Button>();
                }
                return _Play;
            }
        }



    }
}
