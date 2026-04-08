using UnityEngine;

namespace DLP.Player.Abilities
{
    public class WallJumpAbility : MonoBehaviour, IAbility
    {
        public int Priority => 200;

        [Header("Wall Jump")]
        [SerializeField] private float wallJumpHorizontalSpeed = 7.5f;
        [SerializeField] private float wallJumpVerticalSpeed = 6.5f;
        [SerializeField] private float inputThreshold = 0.1f;
        [SerializeField] private float controlLockDuration = 0.15f;

        private AbilityContext _ctx;
        private Vector2 _moveInput;
        private PlayerController _controller;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

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
            int wallSide = GetWallSideForJump();
            if (wallSide == 0)
                return false;

            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = 0f;
            v.y = 0f;
            _ctx.Rb.linearVelocity = v;

            Vector3 jumpVelocity = new Vector3(
                -wallSide * wallJumpHorizontalSpeed,
                wallJumpVerticalSpeed,
                0f
            );

            _ctx.Rb.AddForce(jumpVelocity, ForceMode.VelocityChange);

            if (_controller != null)
            {
                _controller.LockHorizontalControl(controlLockDuration, -wallSide);
            }

            return true;
        }

        public void OnJumpHeld(bool held) { }

        public void OnUpdate() { }

        public void OnFixedUpdate() { }

        private int GetWallSideForJump()
        {
            if (_ctx.IsGrounded())
                return 0;

            bool touchingLeftWall = _ctx.IsTouchingLeftWall();
            bool touchingRightWall = _ctx.IsTouchingRightWall();

            bool pressingRight = _moveInput.x > inputThreshold;
            bool pressingLeft = _moveInput.x < -inputThreshold;

            // 왼쪽 벽에 붙어 있고, 오른쪽 입력이면 벽점프 가능
            if (touchingLeftWall && pressingRight)
                return -1;

            // 오른쪽 벽에 붙어 있고, 왼쪽 입력이면 벽점프 가능
            if (touchingRightWall && pressingLeft)
                return 1;

            return 0;
        }
    }
}