using UnityEngine;

/// Переключает Idle / Run по параметру bool isRunning в Animator.
/// Повесь на игрока (корень) или на объект с Animator (например Visual).
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator")]
    [Tooltip("Если пусто — Animator на этом объекте или в дочерних.")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isRunningParam = "isRunning";

    [Header("Когда считать «бег»")]
    [Tooltip("Горизонтальная скорость выше порога → Run (режим без бесконечного бега).")]
    [SerializeField] private float runSpeedThreshold = 0.5f;
    [Tooltip("В режиме бесконечного бега (PlayerRun) — всегда Run, кроме нырка/слайда.")]
    [SerializeField] private bool useInfiniteRunFromPlayerRun = true;

    private Rigidbody rb;
    private PlayerRun playerRun;
    private PlayerJumpDive jumpDive;
    private Slide slide;
    private int isRunningHash;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        isRunningHash = Animator.StringToHash(isRunningParam);

        Transform root = transform;
        if (animator != null && animator.transform != transform)
            root = animator.transform.root;

        rb = root.GetComponent<Rigidbody>();
        playerRun = root.GetComponent<PlayerRun>();
        jumpDive = root.GetComponent<PlayerJumpDive>();
        slide = root.GetComponent<Slide>();
    }

    void Update()
    {
        if (animator == null)
            return;

        bool running = ShouldRun();
        animator.SetBool(isRunningHash, running);
    }

    bool ShouldRun()
    {
        if (jumpDive != null && jumpDive.IsRecovering)
            return false;

        if (slide != null && slide.IsSliding)
            return false;

        if (useInfiniteRunFromPlayerRun && playerRun != null && playerRun.InfiniteRunEnabled)
            return true;

        if (rb == null)
            return false;

        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        return v.sqrMagnitude > runSpeedThreshold * runSpeedThreshold;
    }
}
