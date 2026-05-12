using UnityEngine;

/// Контрольная точка: триггер-коллайдер + порядковый номер.
/// Расставь пустые объекты с BoxCollider (Is Trigger) и этим скриптом вдоль трассы.
[DisallowMultipleComponent]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("По возрастанию: 0, 1, 2… Игрок запоминает последнюю пройденную (больший номер перезаписывает).")]
    [SerializeField] private int orderIndex;

    [Tooltip("Если пусто — ищется RunRestartManager в сцене.")]
    [SerializeField] private RunRestartManager manager;

    public int OrderIndex => orderIndex;

    void Awake()
    {
        if (manager == null)
            manager = FindFirstObjectByType<RunRestartManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (manager == null)
            manager = FindFirstObjectByType<RunRestartManager>();

        if (manager != null)
            manager.NotifyCheckpointPassed(this);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        Collider c = GetComponent<Collider>();
        if (c != null && !c.isTrigger)
            c.isTrigger = true;
    }
#endif

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.85f, 0.3f, 0.6f);
        Collider col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        else
            Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
