using System.Collections.Generic;
using UnityEngine;

namespace UnionAvatars.Utils
{
    public static class TransformUtility
    {
        public static bool TryFindBFS(this Transform parent, string name, out Transform child)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == name)
                {
                    child = c;
                    return true;
                }
                foreach(Transform t in c)
                    queue.Enqueue(t);
            }
            child = null;
            return false;
        } 

        public static Transform FindBFS(this Transform parent, string name)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == name)
                {
                    return c;
                }
                foreach(Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        } 
    }
}