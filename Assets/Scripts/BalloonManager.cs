using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BalloonManager : MonoBehaviour
{
    public static BalloonManager Instance;

    public GameObject balloonPrefab;
    public GameObject horizontalLayoutPrefab;
    public int poolSize = 50;
    public float minSpawnDelay = 2f;
    public float maxSpawnDelay = 0.5f;
    public float spawnPointCooldown = 2.4f;

    private List<SpawnPoint> spawnPoints;
    private List<GameObject> balloonPool;
    private int poolIndex = 0;
    // time over which difficulty increases
    public float difficultyRampUpTime = 120f;
    private float startTime;

    private Coroutine spawnCoroutine;

    private float spawnRateModifier = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            InstantiateHorizontalLayout();
            CreateBalloonPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartSpawning()
    {
        startTime = Time.time;

        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnBalloons());
        }
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        foreach (GameObject balloon in balloonPool)
        {
            if (balloon.activeInHierarchy)
            {
                balloon.SetActive(false);
            }
        }
    }

    public void AdjustSpawnRate()
    {
        spawnRateModifier = UIManager.Instance.spawnRateSlider.value;
    }

    IEnumerator SpawnBalloons()
    {
        while (true)
        {
            float elapsedTime = Time.time - startTime;
            float difficultyFactor = Mathf.Clamp01(elapsedTime / difficultyRampUpTime);

            float baseMinDelay = Mathf.Lerp(minSpawnDelay, maxSpawnDelay, difficultyFactor);
            float adjustedMinDelay = baseMinDelay / spawnRateModifier;

            yield return new WaitForSeconds(adjustedMinDelay);

            // get spawn points that are doenw with cooldown
            List<SpawnPoint> availableSpawnPoints = new();
            foreach (var sp in spawnPoints)
            {
                if (Time.time >= sp.nextAvailableTime)
                {
                    availableSpawnPoints.Add(sp);
                }
            }

            if (availableSpawnPoints.Count == 0)
            {
                // no spawn points are done with cooldown; so skip
                continue;
            }

            int numBalloonsToSpawn = Random.Range(1, availableSpawnPoints.Count + 1);

            for (int i = 0; i < numBalloonsToSpawn; i++)
            {
                if (availableSpawnPoints.Count == 0)
                    break;

                int randomIndex = Random.Range(0, availableSpawnPoints.Count);
                SpawnPoint spawnPoint = availableSpawnPoints[randomIndex];
                availableSpawnPoints.RemoveAt(randomIndex);

                GameObject balloon = GetPooledBalloon();
                if (balloon != null)
                {
                    balloon.transform.position = spawnPoint.transform.position;
                    balloon.SetActive(true);
                }

                // add cooldown to spawn point
                spawnPoint.nextAvailableTime = Time.time + spawnPointCooldown / spawnRateModifier;
            }
        }
    }

    void InstantiateHorizontalLayout()
    {
        // put horizontal layout just below the bottom of screen
        Camera mainCamera = Camera.main;
        Vector3 bottomCenter = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, mainCamera.nearClipPlane));
        Vector3 spawnPosition = new Vector3(0, bottomCenter.y - 1f, 0); // Adjust '-1f' to position just off-screen

        GameObject horizontalLayout = Instantiate(horizontalLayoutPrefab, spawnPosition, Quaternion.identity);
        horizontalLayout.transform.SetParent(this.transform);

        spawnPoints = new List<SpawnPoint>();
        foreach (Transform child in horizontalLayout.transform)
        {
            spawnPoints.Add(new SpawnPoint(child));
        }
    }

    void CreateBalloonPool()
    {
        balloonPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject balloon = Instantiate(balloonPrefab);
            balloon.SetActive(false);
            balloonPool.Add(balloon);
        }
    }

    GameObject GetPooledBalloon()
    {
        // find inactive balloon
        for (int i = 0; i < balloonPool.Count; i++)
        {
            poolIndex = (poolIndex + 1) % balloonPool.Count;
            if (!balloonPool[poolIndex].activeInHierarchy)
            {
                return balloonPool[poolIndex];
            }
        }

        return null;
    }

    public int PopBalloonsAbove(float yPosition)
    {
        int NumberOfBalloonsAbove = 0;
        foreach (GameObject balloon in balloonPool)
        {
            if (balloon.activeInHierarchy && balloon.transform.position.y > yPosition)
            {
                NumberOfBalloonsAbove ++;
                if (balloon.TryGetComponent<Balloon>(out var balloonScript))
                {
                    balloonScript.Pop();
                }
            }
        }
        return NumberOfBalloonsAbove;
    }

    private class SpawnPoint
    {
        public Transform transform;
        public float nextAvailableTime;

        public SpawnPoint(Transform transform)
        {
            this.transform = transform;
            nextAvailableTime = 0f;
        }
    }
}
