using UnityEngine;

namespace DLP.Player.Abilities
{
    public class JumpAbility : MonoBehaviour, IAbility
    {
        public int Priority => 0;

        [Header("Jump Counts")]
        [SerializeField] private int maxJumpCount = 2;

        [Header("Jump Impulse")]
        [SerializeField] private float firstJumpImpulse = 7.5f;
        [SerializeField] private float secondJumpImpulse = 5.8f;

        [Header("Short Jump (Release Cut)")]
        [SerializeField, Range(0.1f, 0.9f)]
        private float jumpCutMultiplier = 0.55f;

        [Header("Better Fall")]
        [SerializeField, Range(1.0f, 3.0f)]
        private float fallGravityMultiplier = 1.6f;

        private AbilityContext _ctx;

        private int _jumpCount;
        private bool _jumpPressed;
        private bool _jumpHeld;
        private bool _jumpReleased; // 릴리즈 순간을 잡기 위해 추가

        public void Initialize(AbilityContext ctx)
        {
            _ctx = ctx;
            _jumpCount = 0;
            _jumpPressed = false;
            _jumpHeld = false;
            _jumpReleased = false;
        }

        public void OnUpdate()
        {
            if (_ctx.IsGrounded())
                _jumpCount = 0;
        }

        public void OnFixedUpdate()
        {
            HandleJump();
            ApplyJumpCut();     // 1단 점프만 숏점프 처리
            ApplyBetterFall();  // 낙하만 가속
        }

        private void HandleJump()
        {
            if (!_jumpPressed) return;
            _jumpPressed = false;

            if (_jumpCount >= maxJumpCount) return;

            bool isFirstJump = (_jumpCount == 0);
            float impulse = isFirstJump ? firstJumpImpulse : secondJumpImpulse;

            Vector3 v = _ctx.Rb.linearVelocity;

            // 1단 점프: 일관성 위해 y=0
            if (isFirstJump)
            {
                v.y = 0f;
                _ctx.Rb.linearVelocity = v;
            }
            else
            {
                // 2단 점프: 보조 점프 느낌
                // "첫 점프 재실행" 느낌 줄이기 위해 y를 완전 리셋하지 않고,
                // 내려가는 중일 때만 바닥감 제거용으로 0으로 끌어올림
                if (v.y < 0f)
                {
                    v.y = 0f;
                    _ctx.Rb.linearVelocity = v;
                }
            }

            _ctx.Rb.AddForce(Vector3.up * impulse, ForceMode.Impulse);

            _jumpCount++;
            _jumpReleased = false; // 점프 시작 시 릴리즈 플래그 초기화
        }

        private void ApplyJumpCut()
        {
            // 첫 점프에서만 적용 (2단 점프는 고정 높이)
            if (_jumpCount != 1) { _jumpReleased = false; return; }

            // 릴리즈 순간(held가 true → false로 바뀐 프레임)에서만 컷
            if (!_jumpReleased) return;

            Vector3 v = _ctx.Rb.linearVelocity;

            // 상승 중일 때만 컷 (이미 떨어지는 중이면 손대지 않음)
            if (v.y > 0f)
            {
                v.y *= jumpCutMultiplier;
                _ctx.Rb.linearVelocity = v;
            }

            _jumpReleased = false;
        }

        private void ApplyBetterFall()
        {
            Vector3 v = _ctx.Rb.linearVelocity;

            if (v.y < 0f)
            {
                _ctx.Rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
            }
        }

        public void OnMoveInput(Vector2 move) { }

        public bool OnJumpPressed()
        {
            _jumpPressed = true;
            return true;
        }

        public void OnJumpHeld(bool held)
        {
            // held가 true → false로 바뀌는 순간을 기록
            if (_jumpHeld && !held)
                _jumpReleased = true;

            _jumpHeld = held;
        }
    }
}