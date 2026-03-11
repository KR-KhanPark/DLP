using UnityEngine;

namespace DLP.Player.Abilities
{
    public sealed class AbilityContext
    {
        public Rigidbody Rb { get; }
        public Transform GroundCheckOrigin { get; }
        public float GroundCheckRadius { get; }
        public LayerMask GroundMask { get; }

        public AbilityContext(Rigidbody rb, Transform groundCheckOrigin, float groundCheckRadius, LayerMask groundMask)
        {
            Rb = rb;
            GroundCheckOrigin = groundCheckOrigin;
            GroundCheckRadius = groundCheckRadius;
            GroundMask = groundMask;
        }

        public bool IsGrounded()
        {
            var pos = GroundCheckOrigin != null ? GroundCheckOrigin.position : (Rb.transform.position + Vector3.down * 0.9f);
            return Physics.CheckSphere(pos, GroundCheckRadius, GroundMask);
        }
    }
}