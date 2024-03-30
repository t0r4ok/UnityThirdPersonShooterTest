using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TPSgameAssets.Scripts
{
    public class EnemySpawner : MonoBehaviour
    {
        public Transform[] spawnPosition;
        public GameObject[] enemyType;
        [SerializeField] private Cooldown spawnCooldown;
        [SerializeField] private int maxEnemiesOnMap;
        
        

        private void LateUpdate()
        {
            SpawnEnemy();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void SpawnEnemy()
        {
            GameObject[] enemycount = GameObject.FindGameObjectsWithTag("Enemy");
            
            if (spawnCooldown.IsInCooldown && enemycount.Length > maxEnemiesOnMap) return;
            
            Instantiate(enemyType[Random.Range(0, enemyType.Length)], spawnPosition[Random.Range(0, spawnPosition.Length)]);
            spawnCooldown.StartCooldown();
        }
    }
}
