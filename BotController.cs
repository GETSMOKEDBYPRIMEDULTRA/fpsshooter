using UnityEngine;

public class BotController : MonoBehaviour
{
    public CharacterController controller;
    public float moveSpeed = 4f;
    public float detectionRange = 40f;
    public float fireRange = 30f;
    public float fireInterval = 0.3f;
    public LayerMask visionMask;

    Transform target;
    float nextFireTime;

    void Update()
    {
        FindTarget();
        MoveAndShoot();
    }

    void FindTarget()
    {
        if (target != null) return;

        PlayerController[] players = FindObjectsOfType<PlayerController>();
        float bestDist = Mathf.Infinity;
        foreach (var p in players)
        {
            if (!p.isLocalPlayer) // treat local player as human
            {
                float d = Vector3.Distance(transform.position, p.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    target = p.transform;
                }
            }
        }
    }

    void MoveAndShoot()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position);
        float dist = dir.magnitude;
        dir.y = 0;
        dir.Normalize();

        if (dist > fireRange * 0.7f)
        {
            controller.Move(dir * moveSpeed * Time.deltaTime);
        }

        if (dist < detectionRange)
        {
            transform.rotation = Quaternion.LookRotation((target.position - transform.position).normalized);
            TryShoot();
        }
    }

    void TryShoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireInterval;

        if (Physics.Raycast(transform.position + Vector3.up * 1.6f, transform.forward, out RaycastHit hit, fireRange, visionMask))
        {
            Health hp = hit.collider.GetComponentInParent<Health>();
            if (hp != null)
                hp.TakeDamage(10f, null);
        }
    }
}
