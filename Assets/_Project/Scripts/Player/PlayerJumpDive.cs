using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJumpDive : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float jumpForce = 13f;
    [SerializeField] private float diveForce = 7f;
    [SerializeField] private float diveLieTime = 0.5f;
    [SerializeField] private float diveMinAirTime = 0.15f;

    [Header("Физика")]
    [SerializeField] private Vector3 gravity = new Vector3(0f, -30f, 0f);

    [Header("Input Actions (Player Input)")]
    [SerializeField] private string jumpActionName = "Jump";

    [Header("Ссылки")]
    [SerializeField] private Transform visualModel; // Сюда перетащи модельку (Visual)
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckRadius = 0.3f;

    private Rigidbody rb;
    private PlayerInput playerInput;
    private InputAction jumpAction;

    private int jumpCount = 0;
    private bool isGrounded;
    private bool isRecovering = false;
    private float groundCheckDelay = 0f;
    private Coroutine diveRoutine;

    public bool IsRecovering => isRecovering;
    public bool IsGrounded => isGrounded;

    /// <summary>Телепорт / рестарт: сброс нырка и корутин.</summary>
    public void InterruptForTeleport()
    {
        if (diveRoutine != null)
        {
            StopCoroutine(diveRoutine);
            diveRoutine = null;
        }

        isRecovering = false;
        jumpCount = 0;
        groundCheckDelay = 0f;

        if (visualModel != null)
            visualModel.localRotation = Quaternion.identity;
    }

    void Start()
    {
        Physics.gravity = gravity;

        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
            jumpAction = playerInput.actions[jumpActionName];
    }

    void Update()
    {
        if (groundCheckDelay > 0f) groundCheckDelay -= Time.deltaTime;

        // Проверка земли
        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        else
            isGrounded = false;

        if (isGrounded && groundCheckDelay <= 0f && rb.linearVelocity.y <= 0.1f)
            jumpCount = 0;

        if (isRecovering) return;

        if (jumpAction != null && jumpAction.WasPressedThisFrame())
        {
            if (isGrounded) Jump();
            else if (jumpCount == 1) Dive();
        }
    }

    void Jump()
    {
        jumpCount = 1;
        isGrounded = false;
        groundCheckDelay = 0.2f;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void Dive()
    {
        jumpCount = 2;
        isRecovering = true;

        Vector3 diveDir = transform.forward;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce((diveDir + Vector3.up * 0.2f) * diveForce, ForceMode.Impulse);

        // Визуальный наклон модельки вперед
        if (visualModel) visualModel.localRotation = Quaternion.Euler(90f, 0f, 0f);

        groundCheckDelay = 0.2f;

        if (diveRoutine != null) StopCoroutine(diveRoutine);
        diveRoutine = StartCoroutine(DiveRecoverRoutine());
    }

    IEnumerator DiveRecoverRoutine()
    {
        if (diveMinAirTime > 0f)
            yield return new WaitForSeconds(diveMinAirTime);

        while (!(isGrounded && groundCheckDelay <= 0f && rb.linearVelocity.y <= 0.1f))
            yield return null;

        if (diveLieTime > 0f)
            yield return new WaitForSeconds(diveLieTime);

        ResetPlayer();
        diveRoutine = null;
    }

    void ResetPlayer()
    {
        isRecovering = false;
        if (visualModel) visualModel.localRotation = Quaternion.identity;
    }
}

