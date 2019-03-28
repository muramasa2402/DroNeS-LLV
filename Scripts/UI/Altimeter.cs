
using UnityEngine;
using System.Collections;

namespace Drones
{
    using Utils;
    using Utils.Extensions;
    using static SceneAttributes;
    public class Altimeter : MonoBehaviour
    {
        Vector2 startPosition;
        RectTransform rect;
        RectTransform parentRect;
        float mapToReal;
        float realToScale;
        float scaleHeight;

        IEnumerator Start()
        {
            rect = transform.ToRect();
            parentRect = transform.parent.ToRect();
            // 2232/2480/600 is based on image size
            // 2232 is scale height, 2480 is image height. 600 is the scale range (600 m)
            scaleHeight = 2 * 2232f / 2480f * parentRect.rect.height;
            mapToReal = Constants.realWorldTileSize / Constants.unityTileSize; // Map to Real
            realToScale = scaleHeight / 600f;
            Vector2 tmp = rect.offsetMax;
            rect.anchoredPosition = -rect.sizeDelta.x * 4 / 3 * Vector2.right;
            tmp.x = rect.offsetMax.x;
            rect.offsetMax = tmp;

            startPosition = rect.offsetMin;
            startPosition.y = -2232f / 2480f * parentRect.rect.height;
            rect.offsetMin = startPosition;

            while (true)
            {
                float currentHeight = CameraContainer.position.y * mapToReal;
                currentHeight = Mathf.Clamp(currentHeight, 0, 600f);

                rect.offsetMin = startPosition + currentHeight * realToScale * Vector2.up;
                yield return null;
            }
        }

    }
}
