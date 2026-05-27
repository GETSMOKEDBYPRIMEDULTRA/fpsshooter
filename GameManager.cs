using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Spawns")]
    public Transform[] spawnPoints;

    [Header("Match")]
    public bool teamDeathmatch = false;
    public int scoreToWin = 25;
    public float matchTime = 600f; // 10 min

    float timeLeft;
    int team1Score;
    int team2Score;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        timeLeft = matchTime;
    }

    void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            EndMatch();
        }
    }

    public void OnEntityKilled(Health victim, PlayerController attacker)
    {
        // Respawn victim
        Transform spawn = GetRandomSpawn();
        victim.Respawn(spawn.position);

        // Scoring
        if (attacker == null) return;

        if (!teamDeathmatch)
        {
            // FFA: you’d track per‑player score here
            Debug.Log(attacker.playerName + " got a kill (FFA)");
        }
        else
        {
            if (attacker.teamId == 1) team1Score++;
            else if (attacker.teamId == 2) team2Score++;

            Debug.Log($"T1: {team1Score}  T2: {team2Score}");

            if (team1Score >= scoreToWin || team2Score >= scoreToWin)
                EndMatch();
        }
    }

    Transform GetRandomSpawn()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    void EndMatch()
    {
        Debug.Log("Match over");
        // TODO: show UI, restart, etc.
    }
}
