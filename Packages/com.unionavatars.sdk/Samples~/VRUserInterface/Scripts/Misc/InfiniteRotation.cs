using UnityEngine;

namespace UnionAvatars.VRUI
{
    public class InfiniteRotation : MonoBehaviour
    {
        public float speed;

        private void Update()
        {
            transform.Rotate(Vector3.forward, speed * Time.deltaTime);
        }
    }
}
