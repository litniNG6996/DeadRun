using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRun : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private float rotationSpeed = 720f;

    [Header("Input Actions (Player Input)")]
    [SerializeField] private string moveActionName = "Move";

    private Rigidbody rb;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private PlayerJumpDive jumpDive;

    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        jumpDive = GetComponent<PlayerJumpDive>();

        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null && playerInput.actions != null)
            moveAction = playerInput.actions[moveActionName];
    }

    void Update()
    {
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

        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(moveInput.x * speed, v.y, moveInput.z * speed);
    }
}

