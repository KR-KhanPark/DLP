using UnityEngine;

namespace DLP.Player.Abilities
{
    public sealed class AbilityContext
    {
        public Rigidbody Rb { get; }
        public Transform GroundCheckOrigin { get; }
        public float GroundCheckRadius { get; }
        public LayerMask GroundMask { get; }

        public Transform WallCheckLeft { get; }
        public Transform WallCheckRight { get; }
        public float WallCheckRadius { get; }
        public LayerMask WallMask { get; }

        public AbilityContext(
            Rigidbody rb,
            Transform groundCheckOrigin,
            float groundCheckRadius,
            LayerMask groundMask,
            Transform wallCheckLeft,
            Transform wallCheckRight,
            float wallCheckRadius,
            LayerMask wallMask)
        {
            Rb = rb;
            GroundCheckOrigin = groundCheckOrigin;
            GroundCheckRadius = groundCheckRadius;
            GroundMask = groundMask;

            WallCheckLeft = wallCheckLeft;
            WallCheckRight = wallCheckRight;
            WallCheckRadius = wallCheckRadius;
            WallMask = wallMask;
        }

        public bool IsGrounded()
        {
            Vector3 origin = GroundCheckOrigin != null
                ? GroundCheckOrigin.position
                : (Rb.transform.position + Vector3.down * 0.9f);

            return Physics.CheckSphere(GroundCheckOrigin.position, GroundCheckRadius, GroundMask);
        }

        public bool IsTouchingLeftWall()
        {
            if (WallCheckLeft == null) return false;

            return Physics.CheckSphere(
                WallCheckLeft.position,
                WallCheckRadius,
                WallMask
            );
        }

        public bool IsTouchingRightWall()
        {
            if (WallCheckRight == null) return false;

            return Physics.CheckSphere(
                WallCheckRight.position,
                WallCheckRadius,
                WallMask
            );
        }
    }
}