using UnityEngine;
using System.Collections;

namespace Drones
{
    using Utils;
    using Utils.Extensions;
    using static Singletons;
    public class Altimeter : MonoBehaviour
    {
        private Vector2 _StartPosition;
        private RectTransform _Rect;
        private RectTransform _ParentRect;
        private float _RealToScale;
        private float _ScaleHeight;

        IEnumerator Start()
        {
            _Rect = transform.ToRect();
            _ParentRect = transform.parent.ToRect();
            // 2232/2480/600 is based on image size
            // 2232 is scale height, 2480 is image height. 600 is the scale range (600 m)
            _ScaleHeight = 2 * 2232f / 2480f * _ParentRect.rect.height;
            _RealToScale = _ScaleHeight / 600f;
            Vector2 tmp = _Rect.offsetMax;
            _Rect.anchoredPosition = -_Rect.sizeDelta.x * 4 / 3 * Vector2.right;
            tmp.x = _Rect.offsetMax.x;
            _Rect.offsetMax = tmp;

            _StartPosition = _Rect.offsetMin;
            _StartPosition.y = -2232f / 2480f * _ParentRect.rect.height;
            _Rect.offsetMin = _StartPosition;
            var s = new WaitForSeconds(1 / 60);

            while (true)
            {
                float currentHeight = Functions.UnityToMetre(RTSCameraContainer.position.y);
                currentHeight = Mathf.Clamp(currentHeight, 0, 600f);

                _Rect.offsetMin = _StartPosition + currentHeight * _RealToScale * Vector2.up;
                yield return s;
            }
        }

    }
}
