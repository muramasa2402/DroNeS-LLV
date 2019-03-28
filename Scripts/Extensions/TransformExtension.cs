using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils.Extensions
{
    public static class TransformExtension
    {
        public static RectTransform ToRect(this Transform transform)
        {
            return (RectTransform)transform;
        }
    }
}
