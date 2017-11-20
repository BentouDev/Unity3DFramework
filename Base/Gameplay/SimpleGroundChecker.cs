using UnityEngine;

namespace Framework
{
    public class SimpleGroundChecker : GroundChecker
    {
        [Header("Ground Checking")]
        public bool DrawHit;
        public LayerMask RaycastMask;
        public Vector3 RaycastOrigin;
        public float RaycastRadius = 1;
        public float RaycastLength = 1;
        protected RaycastHit LastGroundHit;

        Vector3[] GetRaycastOffsets()
        {
            return new []
            {
                transform.rotation * (Vector3.forward * RaycastRadius),
                transform.rotation * (Vector3.back    * RaycastRadius),
                transform.rotation * (Vector3.right   * RaycastRadius),
                transform.rotation * (Vector3.left    * RaycastRadius)
            };
        }

        protected override bool OnCheckGround()
        {
            var orign = transform.TransformPoint(RaycastOrigin);

            bool _isGrounded = Physics.Raycast(orign, Vector3.down, out LastGroundHit, RaycastLength, RaycastMask);
            if (_isGrounded && DrawHit)
            {
                Debug.DrawLine(LastGroundHit.point, LastGroundHit.point + LastGroundHit.normal, Color.cyan, 5.0f);
            }
            else
            {
                foreach (Vector3 offset in GetRaycastOffsets())
                {
                    _isGrounded |= Physics.Raycast(orign + offset, Vector3.down, out LastGroundHit, RaycastLength, RaycastMask);
                    if (_isGrounded)
                        break;
                }
            }

            return _isGrounded;
        }
    }
}