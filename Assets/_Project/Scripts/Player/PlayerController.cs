using UnityEngine;
using UnityEngine.InputSystem;
using DLP.Player.Abilities;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 6f;

    // 지상에서 목표 속도까지 가속/감속하는 “가속도” 개념 (단위: m/s^2 느낌)
    [SerializeField] private float groundAcceleration = 40f;
    [SerializeField] private float groundDeceleration = 55f;

    // 공중 제어는 보통 약하게(관성 느낌을 주기 위해)
    [Range(0f, 1f)]
    [SerializeField] private float airControl = 0.6f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckOrigin;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private float wallCheckRadius = 0.15f;
    [SerializeField] private LayerMask wallMask;

    [Header("Ability System")]
    [SerializeField] private AbilityRunner abilityRunner;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float fallRespawnY = -10f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool jumpHeld; // 점프 버튼을 누르고 있는지(짧은 점프/긴 점프)

    private float horizontalControlLockTimer;
    private float forcedMoveInputX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        if (abilityRunner == null)
            abilityRunner = GetComponent<AbilityRunner>();

        if (abilityRunner != null)
        {
            var ctx = new AbilityContext(
                rb,
                groundCheckOrigin,
                groundCheckRadius,
                groundMask,
                wallCheckLeft,
                wallCheckRight,
                wallCheckRadius,
                wallMask
            );

            abilityRunner.Initialize(ctx);
        }
    }

    private void Update()
    {
        if (transform.position.y < fallRespawnY)
        {
            Respawn();
        }
    }

    // New Input System 이벤트 콜백
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        abilityRunner?.ForwardMoveInput(moveInput);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // 버튼이 눌린 순간
        if (context.performed)
        {
            abilityRunner?.ForwardJumpPressed();
        }

        // 버튼을 누르고 있는 동안/뗐을 때 상태 추적
        // context.ReadValueAsButton()은 누르고 있으면 true
        jumpHeld = context.ReadValueAsButton();

        abilityRunner?.ForwardJumpHeld(jumpHeld);
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();

        if (horizontalControlLockTimer > 0f)
            horizontalControlLockTimer -= Time.fixedDeltaTime;

        ApplyHorizontalMovement(grounded);
    }

    private void ApplyHorizontalMovement(bool grounded)
    {
        float inputX = horizontalControlLockTimer > 0f ? forcedMoveInputX : moveInput.x;
        float targetX = inputX * maxSpeed;

        // 현재 속도
        Vector3 v = rb.linearVelocity;

        // 지상/공중 가속도 설정
        // 공중은 airControl만큼만 반영해서 관성 느낌 유지
        float accel = grounded ? groundAcceleration : groundAcceleration * airControl;
        float decel = grounded ? groundDeceleration : groundDeceleration * airControl;

        // 입력이 있을 땐 가속, 입력이 없으면 감속(마찰 느낌)
        float rate = Mathf.Abs(targetX) > 0.01f ? accel : decel;

        // 현재 속도를 목표 속도로 서서히 이동 → “관성”
        v.x = Mathf.MoveTowards(v.x, targetX, rate * Time.fixedDeltaTime);

        rb.linearVelocity = v;
    }

    private bool IsGrounded()
    {
        Vector3 pos = groundCheckOrigin != null
            ? groundCheckOrigin.position
            : transform.position + Vector3.down * 0.9f;

        return Physics.CheckSphere(pos, groundCheckRadius, groundMask);
    }
    public void LockHorizontalControl(float duration, float forcedInputX)
    {
        horizontalControlLockTimer = duration;
        forcedMoveInputX = Mathf.Clamp(forcedInputX, -1f, 1f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 pos = groundCheckOrigin != null ? groundCheckOrigin.position : transform.position + Vector3.down * 0.9f;
        Gizmos.DrawWireSphere(pos, groundCheckRadius);
    }
#endif
    private void Respawn()
    {
        Vector3 targetPosition = respawnPoint != null ? respawnPoint.position : Vector3.zero;

        rb.linearVelocity = Vector3.zero;
        transform.position = targetPosition;
    }
}