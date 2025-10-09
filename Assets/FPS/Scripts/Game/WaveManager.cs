using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;   // para Health y EventManager

public class WaveManager : MonoBehaviour
{
    [Header("Configuración")]
    public EnemySpawner[] spawners;
    public Wave[] waves;
    public float timeBetweenWaves = 5f;

    [Header("Pickups")]
    public GameObject healthPickupPrefab;   // arrastra aquí el prefab de vida
    public Transform pickupSpawnPoint;      // arrastra aquí un Empty sobre la plataforma

    private int currentWaveIndex = 0;
    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool allWavesCompleted = false;

    // 👇 Flags
    private bool waitingForPlayerInput = false;
    private bool playerInsideZone = false;

    // 👇 Control interno
    private int remainingToSpawn = 0;
    private bool waveCompleting = false;

    // Propiedad pública para otros sistemas (Objectives, respawn, etc.)
    public int CurrentWaveIndex => currentWaveIndex;

    void Start()
    {
        waitingForPlayerInput = true;
        SpawnHealthPickup();
    }

    void Update()
    {
        if (waitingForPlayerInput && playerInsideZone && !allWavesCompleted)
        {
            if (Input.GetButton("L1") && Input.GetButton("R1"))
            {
                Debug.Log("Jugador presionó L1+R1 dentro de la arena → comenzando cuenta atrás para wave " + currentWaveIndex);
                waitingForPlayerInput = false;
                StartCoroutine(StartNextWave());
            }
        }
    }

    IEnumerator StartNextWave()
    {
        // Limpieza por seguridad
        foreach (var enemy in aliveEnemies)
            if (enemy != null)
                Destroy(enemy);
        aliveEnemies.Clear();

        yield return new WaitForSeconds(timeBetweenWaves);

        if (currentWaveIndex < waves.Length)
        {
            Wave wave = waves[currentWaveIndex];
            Debug.Log("Iniciando " + wave.waveName);
            EventManager.Broadcast(new WaveStartedEvent(currentWaveIndex));
            yield return StartCoroutine(SpawnWave(wave));
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        aliveEnemies.Clear();
        waveCompleting = false;

        // Contamos cuántos enemigos deben spawnearse en total
        remainingToSpawn = 0;
        foreach (EnemyInWave set in wave.enemies)
            remainingToSpawn += set.count;

        Debug.Log($"[WaveManager] Wave '{wave.waveName}' -> Total a spawnear: {remainingToSpawn}");

        foreach (EnemyInWave enemySet in wave.enemies)
        {
            for (int i = 0; i < enemySet.count; i++)
            {
                EnemySpawner spawner = spawners[Random.Range(0, spawners.Length)];
                GameObject enemy = spawner.SpawnEnemy(enemySet.enemyPrefab);

                if (enemy != null)
                {
                    aliveEnemies.Add(enemy);

                    Health health = enemy.GetComponent<Health>();
                    if (health != null)
                    {
                        GameObject captured = enemy; // evitar cierre sobre variable mutable
                        health.OnDie += () => OnEnemyDied(captured);
                    }
                }

                // Este spawn ya fue ejecutado
                remainingToSpawn--;
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

    void OnEnemyDied(GameObject enemy)
    {
        if (aliveEnemies.Contains(enemy))
            aliveEnemies.Remove(enemy);

        Debug.Log($"[WaveManager] Enemigo muerto -> vivos: {aliveEnemies.Count}, por spawnear: {remainingToSpawn}");

        // Solo completamos la wave si no quedan vivos ni pendientes por salir
        if (!waveCompleting && remainingToSpawn <= 0 && aliveEnemies.Count == 0)
        {
            waveCompleting = true;

            Debug.Log("Oleada " + waves[currentWaveIndex].waveName + " completada!");
            EventManager.Broadcast(new WaveCompletedEvent(currentWaveIndex));

            currentWaveIndex++;

            if (currentWaveIndex < waves.Length)
            {
                waitingForPlayerInput = true;
                SpawnHealthPickup();
            }
            else if (!allWavesCompleted)
            {
                Debug.Log("✅ ¡Todas las oleadas completadas!");
                EventManager.Broadcast(new AllWavesCompletedEvent());
                allWavesCompleted = true;
            }
        }
    }

    void SpawnHealthPickup()
    {
        if (healthPickupPrefab != null)
        {
            Vector3 spawnPos = pickupSpawnPoint != null
                ? pickupSpawnPoint.position
                : transform.position;

            spawnPos += Vector3.up * 1.5f;

            Instantiate(healthPickupPrefab, spawnPos, Quaternion.identity);
            Debug.Log("Pickup de vida spawneado en " + spawnPos);
        }
    }

    public void ResetWaves()
    {
        if (allWavesCompleted)
        {
            Debug.Log("Las waves ya se completaron, no se reinician.");
            return;
        }

        Debug.Log("Reiniciando wave actual: " + waves[currentWaveIndex].waveName);

        foreach (var enemy in aliveEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        aliveEnemies.Clear();

        StopAllCoroutines();

        waitingForPlayerInput = true;
        SpawnHealthPickup();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Algo entró al trigger: " + other.name);

        if (other.CompareTag("Player"))
        {
            playerInsideZone = true;
            Debug.Log("Jugador entró en la zona de waves");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInsideZone = false;
            Debug.Log("Jugador salió de la zona de waves");
        }
    }
}
