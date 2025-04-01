using UnityEngine;

namespace UnionAvatars.Utils
{
    public static class LayerUtility
    {
        public static void SetLayer<T>
                   (this GameObject gameobject, int layer, bool includeChildren = false)
                    where T : Component
        {
            gameobject.layer = layer;
            if (includeChildren == false) return;
 
            var arr = gameobject.GetComponentsInChildren<T>(true);
            for (int i = 0; i < arr.Length; i++)
                arr[i].gameObject.layer = layer;
        }
    }
}