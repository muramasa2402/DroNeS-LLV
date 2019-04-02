using UnityEngine;
using UnityEngine.UI;
using Mapbox.Utils;

namespace Drones.Utils.Extensions
{
    public static class Extensions
    {
        public static void ScrollToTop(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
        public static void ScrollToBottom(this ScrollRect scrollRect)
        {
            scrollRect.normalizedPosition = new Vector2(0, 0);
        }

        public static RectTransform ToRect(this Transform transform)
        {
            return (RectTransform)transform;
        }

        public static float[] ToArray(this Vector3 vector)
        {
            float[] value = { vector.x, vector.y, vector.z };
            return value;
        }
        public static Point ToPoint(this Vector3 source)
        {
            return new Point(source.x, source.y, source.z);
        }

        public static Vector2d ToCoordinates(this Vector3 v)
        {
            Vector2d c = Singletons.Manhattan.WorldToGeoPosition(v);

            return new Vector2d(c.y, c.x);
        }

        public static Vector3 ToUnity(this Vector2d v)
        {
            Vector3 pos = Singletons.Manhattan.GeoToWorldPosition(v);
            return pos;
        }

        public static Transform FindChildWithTag(this Transform t, string tag)
        {
            foreach (Transform child in t)
            {
                if (child.CompareTag(tag)) 
                { 
                    return child; 
                }

                if (child.childCount > 0)
                {
                    var grandchild = FindChildWithTag(child, tag);
                    if (grandchild != null)
                    {
                        return grandchild;
                    }
                }
            }
            return null;
        }
    }
}

