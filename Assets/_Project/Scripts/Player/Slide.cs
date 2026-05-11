using UnityEngine;
using UnityEngine.InputSystem;

/// Слайд по C со старта с земли. Режим в инспекторе: одно нажатие (полный слайд) или удержание C.
/// Горизонтальная скорость за slideDuration гасится до нуля.
/// Выполняется после PlayerMovement.FixedUpdate (DefaultExecutionOrder).
[DefaultExecutionOrder(100)]
public class Slide : MonoBehaviour
{
    [Header("Слайд")]
    [Tooltip("Вкл: один раз нажал C — проигрывается слайд до конца фазы. Выкл: держишь C — слайд идёт, отпустил — сброс.")]
    [SerializeField] private bool slideOnSinglePress = false;
    [SerializeField] private float slideSpeed = 14f;
    [Tooltip("За это время горизонтальная скорость слайда плавно падает до нуля. В режиме одного нажатия за это же время заканчивается весь слайд.")]
    [SerializeField] private float slideDuration = 0.45f;

    [Header("Земля (как в PlayerMovement)")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;

    [Header("Коллайдер (необязательно)")]
    [SerializeField] private CapsuleCollider capsule;
    [Tooltip("Если включено — на время слайда уменьшаем капсулу (чтобы пролезать под препятствиями).")]
    [SerializeField] private bool resizeColliderDuringSlide = true;
    [SerializeField] private float slideHeight = 0.6f;
    [SerializeField] private Vector3 slideCenterOffset = new Vector3(0f, -0.2f, 0f);

    [Header("Наклон модели при слайде")]
    [Tooltip("Дочерний объект с мешем персонажа (не корень с Rigidbody). Если пусто — наклон не применяется.")]
    [SerializeField] private Transform visualModel;
    [Tooltip("Наклон вперёд вокруг локальной X (градусы). Подбери знак под свою модель.")]
    [Range(-90f, 90f)]
    [SerializeField] private float slideModelPitch = -42f;

    private Rigidbody rb;
    private bool slideActive;
    private Vector3 slideDir;
    private float slideSpeedPhaseElapsed;
    private float standCapsuleHeight;
    private float standCapsuleCenterY;
    private bool hasStandCapsuleData;
    private Quaternion standVisualLocalRotation;
    private Quaternion baseVisualLocalRotation;
    private bool visualOverridden;

    public bool IsSliding => slideActive;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (visualModel != null)
            standVisualLocalRotation = visualModel.localRotation;

        if (capsule != null)
        {
            standCapsuleHeight = capsule.height;
            standCapsuleCenterY = capsule.center.y;
            hasStandCapsuleData = true;
        }
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
            return;

        if (kb.cKey.wasPressedThisFrame && !slideActive && IsGrounded())
            BeginSlide();

        if (!slideOnSinglePress && slideActive && !kb.cKey.isPressed)
            EndSlide();
    }

    void FixedUpdate()
    {
        if (!slideActive)
            return;

        slideSpeedPhaseElapsed += Time.fixedDeltaTime;

        float denom = Mathf.Max(0.01f, slideDuration);
        float u = Mathf.Clamp01(slideSpeedPhaseElapsed / denom);
        float speed = Mathf.Lerp(slideSpeed, 0f, u);

        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(slideDir.x * speed, v.y, slideDir.z * speed);

        if (slideOnSinglePress && u >= 1f)
            EndSlide();
    }

    void LateUpdate()
    {
        if (visualModel == null)
            return;

        if (!slideActive)
        {
            if (visualOverridden)
            {
                visualModel.localRotation = baseVisualLocalRotation;
                visualOverridden = false;
            }
            return;
        }

        if (!visualOverridden)
        {
            baseVisualLocalRotation = visualModel.localRotation;
            visualOverridden = true;
        }

        visualModel.localRotation = baseVisualLocalRotation * Quaternion.Euler(slideModelPitch, 0f, 0f);
    }

    bool IsGrounded()
    {
        if (groundCheck == null || rb == null)
            return false;

        return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask)
               && rb.linearVelocity.y <= 0.15f;
    }

    void BeginSlide()
    {
        slideDir = transform.forward;
        slideDir.y = 0f;
        if (slideDir.sqrMagnitude < 0.0001f)
            slideDir = Vector3.forward;
        slideDir.Normalize();

        slideActive = true;
        slideSpeedPhaseElapsed = 0f;

        if (visualModel != null && !visualOverridden)
        {
            baseVisualLocalRotation = visualModel.localRotation;
            visualOverridden = true;
        }

        if (resizeColliderDuringSlide && capsule != null && hasStandCapsuleData)
        {
            float scaleY = Mathf.Max(0.01f, transform.lossyScale.y);
            capsule.height = slideHeight / scaleY;
            capsule.center = new Vector3(
                capsule.center.x,
                slideCenterOffset.y / scaleY,
                capsule.center.z);
        }
    }

    void EndSlide()
    {
        slideActive = false;
        slideSpeedPhaseElapsed = 0f;

        if (rb != null)
        {
            Vector3 v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(0f, v.y, 0f);
        }

        if (visualModel != null && visualOverridden)
        {
            visualModel.localRotation = baseVisualLocalRotation;
            visualOverridden = false;
        }

        if (resizeColliderDuringSlide && capsule != null && hasStandCapsuleData)
        {
            capsule.height = standCapsuleHeight;
            capsule.center = new Vector3(capsule.center.x, standCapsuleCenterY, capsule.center.z);
        }
    }

    void OnDisable()
    {
        if (slideActive)
            EndSlide();
        else if (visualModel != null && visualOverridden)
        {
            visualModel.localRotation = baseVisualLocalRotation;
            visualOverridden = false;
        }
    }
}
