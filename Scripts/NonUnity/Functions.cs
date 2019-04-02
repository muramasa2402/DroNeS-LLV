using Mapbox.Utils;
using UnityEngine;

namespace Drones.Utils
{
    using static Singletons;
    public static class Functions
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

        public static float Metre(Vector2d a, Vector2d b)
        {
            var PIdeg = Mathd.PI / 180;
            var diff = (b - a) * PIdeg;
            var tmp = Mathd.Sin(diff.x / 2) * Mathd.Sin(diff.x / 2) +
            Mathd.Cos(a.x * PIdeg) * Mathd.Cos(b.x * PIdeg) *
            Mathd.Sin(diff.y / 2) * Mathd.Sin(diff.y / 2);

            var c = 2 * Mathd.Atan2(Mathd.Sqrt(tmp), Mathd.Sqrt(1 - tmp));
            var d = Constants.R * c;
            return (float)d * 1000; // meters
        }

        public static Color EditorSet(Transform transform)
        {
            return (transform.GetSiblingIndex() % 2 == 1) ? ListItemOdd : ListItemEven;
        }

        public static void HighlightPosition(Vector3 position)
        {
            if (CurrentPosition != null)
            {
                Object.Destroy(CurrentPosition);
            }
            CurrentPosition = Object.Instantiate(PositionHighlightTemplate);
            CurrentPosition.name = "Current Position";
            CurrentPosition.transform.position = position;
            CurrentPosition.transform.position += Vector3.up * CurrentPosition.transform.lossyScale.y;
        }

        public static void LookHere(Vector3 position)
        {
            var back = -CamTrans.forward;
            RTSCameraContainer.position = position + back * RTSCameraContainer.position.y / back.y;
        }

    }
}
