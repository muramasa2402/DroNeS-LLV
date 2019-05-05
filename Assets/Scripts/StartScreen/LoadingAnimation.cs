using UnityEngine;

namespace Drones.StartScreen
{
    using System.Collections;
    using Utils.Extensions;

    public class LoadingAnimation : MonoBehaviour
    {
        [SerializeField]
        RectTransform[] _Rotors;
        public RectTransform[] Rotors
        {
            get
            {
                if (_Rotors == null)
                {
                    _Rotors = new RectTransform[transform.childCount];
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        _Rotors[i] = transform.GetChild(i).ToRect();
                    }
                }
                return _Rotors;
            }
        }

        private void OnEnable() => StartCoroutine(Spin());

        private void OnDisable() => StopCoroutine(Spin());

        IEnumerator Spin()
        {
            foreach (var rotor in Rotors)
            {
                rotor.eulerAngles = Vector3.forward * Random.value * 360;
            }

            while (true)
            {
                foreach (var rotor in Rotors)
                {
                    rotor.eulerAngles += Vector3.forward * 30;
                }

                yield return null;
            }

        }
    }
}
