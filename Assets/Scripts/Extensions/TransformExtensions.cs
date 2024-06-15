using JetBrains.Annotations;
using UnityEngine;

namespace Extensions
{
    public static class TransformExtensions
    {
        [CanBeNull]
        public static Transform FindChildRecursive(this Transform transform, string name)
        {
            if (transform.name == name)
            {
                return transform;
            }

            if (transform.childCount == 0)
            {
                return null;
            }

            foreach (Transform child in transform)
            {
                var found = child.FindChildRecursive(name);
                if (found)
                {
                    return found;
                }
            }

            return null;
        }
    }
}