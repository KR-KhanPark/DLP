using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 7f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckOrigin;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody rb;

    // New Input System에서 Move는 Vector2로 들어오므로 저장해두고 물리에서 사용
    private Vector2 moveInput;
    // 점프는 "눌림"을 1회 처리하기 위해 bool 래치
    private bool jumpPressed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// PlayerInput(Invoke Unity Events)에서 Move 액션이 호출될 때 들어오는 콜백.
    /// - context.ReadValue<Vector2>()는 (x,y) 입력 값을 가져옴
    /// - 우리는 2.5D라 x만 사용
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// Jump 액션 콜백. performed는 "버튼이 눌린 순간"에 true가 됨.
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            jumpPressed = true;
    }

    private void FixedUpdate()
    {
        // ===== 이동 =====
        // linearVelocity의 x만 갱신하고 y는 중력/점프에 맡김
        Vector3 v = rb.linearVelocity;
        v.x = moveInput.x * moveSpeed;
        rb.linearVelocity = v;

        // ===== 점프 =====
        if (!jumpPressed) return;
        jumpPressed = false;

        if (!IsGrounded()) return;

        // 점프 높이를 일정하게 만들기 위해 y속도 리셋 후 임펄스
        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private bool IsGrounded()
    {
        if (groundCheckOrigin == null)
            return Physics.CheckSphere(transform.position + Vector3.down * 0.9f, groundCheckRadius, groundMask);

        return Physics.CheckSphere(groundCheckOrigin.position, groundCheckRadius, groundMask);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 pos = groundCheckOrigin != null ? groundCheckOrigin.position : transform.position + Vector3.down * 0.9f;
        Gizmos.DrawWireSphere(pos, groundCheckRadius);
    }
#endif
}