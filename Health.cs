using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    float currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, PlayerController attacker)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die(attacker);
        }
    }

    void Die(PlayerController attacker)
    {
        GameManager.Instance.OnEntityKilled(this, attacker);
    }

    public void Respawn(Vector3 pos)
    {
        currentHealth = maxHealth;
        transform.position = pos;
    }
}
