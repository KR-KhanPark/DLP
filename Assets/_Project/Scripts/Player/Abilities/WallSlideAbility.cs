using UnityEngine;

namespace DLP.Player.Abilities
{
    public class WallSlideAbility : MonoBehaviour, IAbility
    {
        public int Priority => 100;

        [SerializeField] private float slideSpeed = 2.0f;
        [SerializeField] private float wallInputThreshold = 0.1f;

        private AbilityContext _ctx;
        private Vector2 _moveInput;

        public void Initialize(AbilityContext ctx)
        {
            _ctx = ctx;
        }

        public void OnMoveInput(Vector2 move)
        {
            _moveInput = move;
        }

        public bool OnJumpPressed()
        {
            return false;
        }

        public void OnJumpHeld(bool held) { }

        public void OnUpdate() { }

        public void OnFixedUpdate()
        {
            if (!CanWallSlide())
                return;

            Vector3 v = _ctx.Rb.linearVelocity;
            v.y = -slideSpeed;
            _ctx.Rb.linearVelocity = v;
        }

        private bool CanWallSlide()
        {
            if (_ctx.IsGrounded())
                return false;

            bool touchingLeftWall = _ctx.IsTouchingLeftWall();
            bool touchingRightWall = _ctx.IsTouchingRightWall();

            bool pressingTowardLeftWall = touchingLeftWall && _moveInput.x < -wallInputThreshold;
            bool pressingTowardRightWall = touchingRightWall && _moveInput.x > wallInputThreshold;

            if (!pressingTowardLeftWall && !pressingTowardRightWall)
                return false;

            if (_ctx.Rb.linearVelocity.y > 0f)
                return false;

            return true;
        }
    }
}