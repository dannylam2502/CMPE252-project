using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject pfEnemy;
    public GameObject enemyContainer;
    public float timeBetweenSpawn = 30.0f;
    public int maxEnemies = 5;
    private int currentEnemyCount = 0;

    void Start()
    {
        StartCoroutine(CoSpawnEnemy());
    }

    public IEnumerator CoSpawnEnemy()
    {
        while (currentEnemyCount < maxEnemies)
        {
            Spawn();
            yield return new WaitForSeconds(timeBetweenSpawn);
        }
    }

    void Spawn()
    {
        if (pfEnemy == null || enemyContainer == null)
        {
            Debug.LogError("pfEnemy or enemyContainer is not assigned!");
            return;
        }

        GameObject obj = Instantiate(pfEnemy, enemyContainer.transform.position, Quaternion.identity, enemyContainer.transform);
        obj.SetActive(true);
        currentEnemyCount++;
    }
}
