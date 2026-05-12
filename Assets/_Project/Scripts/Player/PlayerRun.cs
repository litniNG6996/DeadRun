using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRun : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Бесконечный бег")]
    [Tooltip("Вкл: вперёд бежит сам, стик/клавиши только влево-вправо (ось X у Move). Прыжок и слайд — как отдельные скрипты.")]
    [SerializeField] private bool infiniteRun;
    [SerializeField] private float forwardRunSpeed = 10f;
    [SerializeField] private float lateralStrafeSpeed = 8f;
    [Tooltip("Направление автобега в мировых координатах (например 0,0,1 для бега по +Z).")]
    [SerializeField] private Vector3 runForwardWorld = new Vector3(0f, 0f, 1f);

    [Header("Input Actions (Player Input)")]
    [SerializeField] private string moveActionName = "Move";

    private Rigidbody rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private PlayerJumpDive jumpDive;
    private Slide slide;

    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        jumpDive = GetComponent<PlayerJumpDive>();
        slide = GetComponent<Slide>();

        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
            moveAction = playerInput.actions[moveActionName];
    }

    void Update()
    {
        if (infiniteRun)
        {
            if (jumpDive != null && jumpDive.IsRecovering) return;

            Vector3 fwd = PlanarForward(runForwardWorld);
            if (fwd.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(fwd);
            return;
        }

        if (jumpDive != null && jumpDive.IsRecovering) return;

        Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        moveInput = new Vector3(input.x, 0f, input.y).normalized;

        if (moveInput != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        if (jumpDive != null && jumpDive.IsRecovering) return;
        if (slide != null && slide.IsSliding) return;

        Vector3 v = rb.linearVelocity;

        if (infiniteRun)
        {
            Vector3 fwd = PlanarForward(runForwardWorld);
            if (fwd.sqrMagnitude < 0.0001f)
                fwd = Vector3.forward;

            Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
            float strafe = moveAction != null ? moveAction.ReadValue<Vector2>().x : 0f;
            Vector3 horizontal = fwd * forwardRunSpeed + right * (strafe * lateralStrafeSpeed);
            rb.linearVelocity = new Vector3(horizontal.x, v.y, horizontal.z);
            return;
        }

        rb.linearVelocity = new Vector3(moveInput.x * speed, v.y, moveInput.z * speed);
    }

    static Vector3 PlanarForward(Vector3 w)
    {
        Vector3 f = w;
        f.y = 0f;
        return f.sqrMagnitude > 0.0001f ? f.normalized : Vector3.zero;
    }
}

