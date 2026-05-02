using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 720f; // Скорость поворота
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float diveForce = 7f;

    [Header("Настройки земли")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody rb;
    private Vector3 moveInput;
    private int jumpCount = 0; 
    private bool isGrounded;
    private bool isRecovering = false;
    private float groundCheckDelay = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (groundCheckDelay > 0) groundCheckDelay -= Time.deltaTime;

        bool feelGround = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (feelGround && groundCheckDelay <= 0 && rb.velocity.y <= 0.1f)
        {
            isGrounded = true;
            jumpCount = 0;
            if (isRecovering) 
            {
                StopAllCoroutines();
                ResetRotation();
            }
        }
        else
        {
            isGrounded = false;
        }

        if (isRecovering) return;

        // Считываем ввод
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        moveInput = new Vector3(moveX, 0, moveZ).normalized; // .normalized, чтобы по диагонали не бегал быстрее

        // ПОВОРОТ ПЕРСОНАЖА
        if (moveInput != Vector3.zero)
        {
            // Вычисляем, куда нужно смотреть
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            // Плавно поворачиваемся туда
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space))
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
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void Dive()
    {
        jumpCount = 2;
        StartCoroutine(DiveRoutine());
    }

    IEnumerator DiveRoutine()
    {
        isRecovering = true;
        
        // Берем направление "вперед" именно в момент нажатия на нырок
        Vector3 diveDir = transform.forward;

        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce((diveDir + Vector3.up * 0.2f) * diveForce, ForceMode.Impulse);

        // Визуальный наклон
        transform.rotation = Quaternion.LookRotation(diveDir) * Quaternion.Euler(90, 0, 0);

        yield return new WaitForSeconds(0.7f);

        ResetRotation();
    }

    void ResetRotation()
    {
        isRecovering = false;
        // Возвращаем персонажа в вертикальное положение, сохраняя направление взгляда
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
    }

    void FixedUpdate()
    {
        if (isRecovering) return;
        rb.velocity = new Vector3(moveInput.x * speed, rb.velocity.y, moveInput.z * speed);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}