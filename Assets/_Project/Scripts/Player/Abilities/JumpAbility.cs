using UnityEngine;

namespace DLP.Player.Abilities
{
    public class JumpAbility : MonoBehaviour, IAbility
    {
        public int Priority => 0;

        [Header("Jump Power")]
        [SerializeField] private float firstJumpImpulse = 7.5f;
        [SerializeField] private float airJumpTargetYSpeed = 4.5f;

        [Header("Short Jump (Release Cut)")]
        [SerializeField, Range(0.1f, 0.9f)]
        private float jumpCutMultiplier = 0.55f;

        [Header("Better Fall")]
        [SerializeField, Range(1.0f, 3.0f)]
        private float fallGravityMultiplier = 1.6f;

        private AbilityContext _ctx;

        private bool _jumpQueued;
        private bool _jumpHeld;
        private bool _jumpReleasedThisFrame;

        // 첫 점프에만 숏점프 허용
        private bool _canApplyJumpCut;

        private bool _wasGrounded;

        // 바닥에서 출발한 뒤 공중 추가점프 1회만 허용
        private bool _airJumpAvailable;

        public void Initialize(AbilityContext ctx)
        {
            _ctx = ctx;

            _jumpQueued = false;
            _jumpHeld = false;
            _jumpReleasedThisFrame = false;
            _canApplyJumpCut = false;

            _wasGrounded = _ctx.IsGrounded();
            _airJumpAvailable = true;
        }

        public void OnUpdate()
        {
            bool isGrounded = _ctx.IsGrounded();

            // 착지한 순간 리셋
            if (isGrounded && !_wasGrounded)
            {
                _airJumpAvailable = true;
                _canApplyJumpCut = false;
                _jumpReleasedThisFrame = false;
            }

            _wasGrounded = isGrounded;
        }

        public void OnFixedUpdate()
        {
            HandleJump();
            ApplyJumpCut();
            ApplyBetterFall();
        }

        private void HandleJump()
        {
            if (!_jumpQueued)
                return;

            _jumpQueued = false;

            bool isGrounded = _ctx.IsGrounded();
            Vector3 v = _ctx.Rb.linearVelocity;

            if (isGrounded)
            {
                // 첫 점프
                v.y = 0f;
                _ctx.Rb.linearVelocity = v;

                _ctx.Rb.AddForce(Vector3.up * firstJumpImpulse, ForceMode.Impulse);

                _canApplyJumpCut = true;
                _jumpReleasedThisFrame = false;
                return;
            }

            if (_airJumpAvailable)
            {
                // 공중 추가점프 1회
                float currentY = v.y;
                float neededBoost = airJumpTargetYSpeed - currentY;

                if (neededBoost > 0f)
                {
                    _ctx.Rb.AddForce(Vector3.up * neededBoost, ForceMode.VelocityChange);
                }

                _airJumpAvailable = false;
                _canApplyJumpCut = false;
                _jumpReleasedThisFrame = false;
            }
        }

        private void ApplyJumpCut()
        {
            if (!_canApplyJumpCut)
                return;

            if (!_jumpReleasedThisFrame)
                return;

            Vector3 v = _ctx.Rb.linearVelocity;

            if (v.y > 0f)
            {
                v.y *= jumpCutMultiplier;
                _ctx.Rb.linearVelocity = v;
            }

            _jumpReleasedThisFrame = false;
            _canApplyJumpCut = false;
        }

        private void ApplyBetterFall()
        {
            Vector3 v = _ctx.Rb.linearVelocity;

            if (v.y < 0f)
            {
                _ctx.Rb.AddForce(
                    Physics.gravity * (fallGravityMultiplier - 1f),
                    ForceMode.Acceleration
                );
            }
        }

        public void OnMoveInput(Vector2 move) { }

        public bool OnJumpPressed()
        {
            bool isGrounded = _ctx.IsGrounded();

            if (!isGrounded && !_airJumpAvailable)
                return false;

            _jumpQueued = true;
            return true;
        }

        public void OnJumpHeld(bool held)
        {
            if (_jumpHeld && !held)
                _jumpReleasedThisFrame = true;

            _jumpHeld = held;
        }
    }
}