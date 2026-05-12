using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// Рестарт с начала и респаун на последней контрольной точке.
/// Повесь на пустой объект в сцене, укажи игрока и (опционально) точку старта.
public class RunRestartManager : MonoBehaviour
{
    [Header("Игрок")]
    [SerializeField] private Transform player;
    [Tooltip("Если пусто — при Awake запоминается позиция/поворот игрока как старт уровня.")]
    [SerializeField] private Transform startSpawnPoint;
    [Tooltip("При старте сцены сразу перенести игрока на Start Spawn Point (если он задан).")]
    [SerializeField] private bool movePlayerToStartOnPlay = true;

    [Header("Ввод (опционально)")]
    [SerializeField] private bool restartKeyEnabled = true;
    [SerializeField] private Key restartKey = Key.R;
    [SerializeField] private Key respawnAtCheckpointKey = Key.T;

    [Header("События")]
    public UnityEvent onRestartFromStart;
    public UnityEvent onRespawnAtCheckpoint;

    private Rigidbody playerRb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private int lastCheckpointOrder = -1;
    private Transform lastCheckpointTransform;

    void Awake()
    {
        if (player == null)
        {
            Debug.LogWarning($"{nameof(RunRestartManager)}: не назначен Player.", this);
            return;
        }

        playerRb = player.GetComponent<Rigidbody>();
        if (playerRb == null)
            Debug.LogWarning($"{nameof(RunRestartManager)}: у игрока нет Rigidbody.", this);

        if (startSpawnPoint != null)
        {
            initialPosition = startSpawnPoint.position;
            initialRotation = startSpawnPoint.rotation;
        }
        else
        {
            initialPosition = player.position;
            initialRotation = player.rotation;
        }
    }

    void Start()
    {
        if (player == null)
            return;

        if (startSpawnPoint != null && movePlayerToStartOnPlay)
            TeleportPlayer(startSpawnPoint.position, startSpawnPoint.rotation);
    }

    void Update()
    {
        if (!restartKeyEnabled || Keyboard.current == null)
            return;

        if (Keyboard.current[restartKey].wasPressedThisFrame)
            RestartFromBeginning();

        if (Keyboard.current[respawnAtCheckpointKey].wasPressedThisFrame)
            RespawnAtLastCheckpoint();
    }

    /// <summary>Игрок прошёл контрольную точку (вызывается из Checkpoint).</summary>
    public void NotifyCheckpointPassed(Checkpoint checkpoint)
    {
        if (checkpoint == null || player == null)
            return;

        int order = checkpoint.OrderIndex;
        if (order <= lastCheckpointOrder)
            return;

        lastCheckpointOrder = order;
        lastCheckpointTransform = checkpoint.transform;
    }

    public void RestartFromBeginning()
    {
        lastCheckpointOrder = -1;
        lastCheckpointTransform = null;

        if (startSpawnPoint != null)
            TeleportPlayer(startSpawnPoint.position, startSpawnPoint.rotation);
        else
            TeleportPlayer(initialPosition, initialRotation);

        onRestartFromStart?.Invoke();
    }

    public void RespawnAtLastCheckpoint()
    {
        if (lastCheckpointTransform != null)
            TeleportPlayer(lastCheckpointTransform.position, lastCheckpointTransform.rotation);
        else
            TeleportPlayer(startSpawnPoint != null ? startSpawnPoint.position : initialPosition,
                startSpawnPoint != null ? startSpawnPoint.rotation : initialRotation);

        onRespawnAtCheckpoint?.Invoke();
    }

    void TeleportPlayer(Vector3 position, Quaternion rotation)
    {
        if (player == null)
            return;

        Slide slide = player.GetComponent<Slide>();
        if (slide != null)
            slide.ForceEndSlideIfActive();

        PlayerJumpDive jump = player.GetComponent<PlayerJumpDive>();
        if (jump != null)
            jump.InterruptForTeleport();

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.position = position;
            playerRb.rotation = rotation;
        }
        else
        {
            player.SetPositionAndRotation(position, rotation);
        }
    }
}
