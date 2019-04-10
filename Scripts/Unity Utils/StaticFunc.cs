using Mapbox.Utils;
using UnityEngine;

namespace Drones.Utils
{
    using Drones.UI;
    using static Singletons;
    public static class StaticFunc
    {
        public static float UnityToMetre(float unity)
        {
            return unity / Manhattan.WorldRelativeScale;
        }

        public static float MetreToUnity(float metre)
        {
            return metre * Manhattan.WorldRelativeScale;
        }

        public static float Tanh(float x)
        {
            // Truncation
            if (x >= 3.65f) return 1;
            if (x < -3.65f) return -1;

            return (Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x));
        }

        public static float CoordDistance(Vector2 a, Vector2 b)
        {
            var PIdeg = Mathf.PI / 180;
            var diff = (b - a) * PIdeg;
            var tmp = Mathf.Sin(diff.x / 2) * Mathf.Sin(diff.x / 2) +
            Mathf.Cos(a.x * PIdeg) * Mathf.Cos(b.x * PIdeg) *
            Mathf.Sin(diff.y / 2) * Mathf.Sin(diff.y / 2);

            var c = 2 * Mathf.Atan2(Mathf.Sqrt(tmp), Mathf.Sqrt(1 - tmp));
            var d = Constants.R * c;
            return d * 1000; // meters
        }

        public static string CoordString(Vector2 a)
        {
            return "(" + a.x.ToString("0.00000") + ", " + a.y.ToString("0.00000") + ")";
        }

        public static Vector2 UnityToCoord(Vector3 v)
        {
            Vector2d c = Manhattan.WorldToGeoPosition(v);

            return new Vector2((float)c.x, (float)c.y);
        }

        public static Color EditorSet(Transform transform)
        {
            return (transform.GetSiblingIndex() % 2 == 1) ? AbstractListElement.ListItemOdd : AbstractListElement.ListItemEven;
        }

        public static void HighlightPosition(Vector3 position)
        {
            if (CurrentPosition != null)
            {
                CurrentPosition.GetComponent<Animation>().Play();
            }
            else
            {
                CurrentPosition = Object.Instantiate(PositionHighlightTemplate);
                CurrentPosition.name = "Current Position";
            }
            CurrentPosition.transform.position = position;
            CurrentPosition.transform.position += Vector3.up * CurrentPosition.transform.lossyScale.y;
        }

        public static void LookHere(Vector3 position)
        {
            HighlightPosition(position);
            var back = -CameraTransform.forward;
            RTS.transform.position = position + back * RTS.transform.position.y / back.y;

            position.y = EagleEye.transform.position.y;
            EagleEye.transform.position = position;
        }

    }
}
