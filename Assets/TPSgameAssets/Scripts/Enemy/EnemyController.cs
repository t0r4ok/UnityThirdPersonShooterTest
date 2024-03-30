using System;
using TPSgameAssets.Scripts.Player;
using UnityEngine;

namespace TPSgameAssets.Scripts.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        public int enemyHealth = 100;
        private GameObject player;
        private PlayerController _playerController;
        public Cooldown deathcooldown;
        private Animator _animator;
        private EnemyAI AI;
        
        private static readonly int goDead = Animator.StringToHash("goDead");
        private static readonly int isBeingHited = Animator.StringToHash("isBeingHited");
        
        
        private void Start()
        {
            player = GameObject.FindWithTag("Player");
            _playerController = player.GetComponent<PlayerController>();
            AI = GetComponent<EnemyAI>();
            
            _animator = GetComponentInChildren<Animator>();
        }
        
        private void Update()
        {
            if (enemyHealth <= 0)
            {
                // TODO: Play animation before enemy despawn. I don't have time for this now.
                _animator.SetTrigger(goDead);
                
                KillEnemy();
            }
            
            //_animator.SetBool(isBeingHited, false);
        }

        public void DamageEnemy(int damage)
        {
            enemyHealth -= damage;
            //_animator.SetBool(isBeingHited, true);
        }
        
        private void KillEnemy()
        {
            _playerController.enemiesKilled++;
            Destroy(gameObject);
        }
        
        // Base to make other stuff here. 
    }
}
