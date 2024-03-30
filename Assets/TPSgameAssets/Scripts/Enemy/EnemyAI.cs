using TPSgameAssets.Scripts.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TPSgameAssets.Scripts.Enemy
{
    public class EnemyAI : MonoBehaviour
    {
        [Header("INFO")] 
        public float remainingDistance;
        
        [Header("SETTINGS:")] 
        [SerializeField] private bool shouldPatrol;
        public Transform[] patrolPoints;
        public GameObject player;
        public Transform playerPosition;
        public int damage = 25;
        public Cooldown atackCoolDown; 
        
        [SerializeField] private float viewAngle = 80;
        public UnityEngine.AI.NavMeshAgent navAgent;
        private Transform _path;
        private bool _playerNoticed;
        
        [Header("Animations")]
        private Animator _animator;
        
        
        private static readonly int IsAtacking = Animator.StringToHash("IsAtacking");
        
        private void Start()
        {
            navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();

            player = GameObject.FindWithTag("Player");
            
            // Just make sure that mob always has some path to patrol.
            if (navAgent.path == null)
            {
                navAgent.destination = patrolPoints[Random.Range(0, patrolPoints.Length)].position;
            }
        }


        private void Update()
        {
            if (!navAgent.isOnNavMesh) return;
            
            remainingDistance = navAgent.remainingDistance;
            
            ChaseLogic();
            NavigationLogic();
            NoticePlayerLogic();
            if (!atackCoolDown.IsInCooldown) AtackPlayer();
            
            if (!shouldPatrol)
            {
                _playerNoticed = true;
            }
            
            if (navAgent.remainingDistance > navAgent.stoppingDistance)
            {
                _animator.SetBool(IsAtacking, false);
            }
        }

        private void NavigationLogic()
        {
            if (_playerNoticed) return;
            if (!shouldPatrol) return;
            
            if (navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                PatrolLogic();
            }
        }


        // ReSharper disable Unity.PerformanceAnalysis
        private void AtackPlayer()
        {
            if (!_playerNoticed) return;
            
            // Pathfinding sometimes thinking that remaining distance is 0, so i decided to fix it with this bandage. 
            if (Vector3.Distance(player.transform.position, transform.position) > 15) return;
            if (navAgent.pathPending) return;
            
            
            if (navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                _animator.SetBool(IsAtacking, true);
                player.GetComponent<PlayerController>().TakeDamage(damage);
                atackCoolDown.StartCooldown();
                UnityEngine.Debug.Log($"Player is damaged by {gameObject.name}, from pos: {transform.position} \n Remaining distance: {navAgent.remainingDistance}");
            }
        }
        
        
        private void PatrolLogic()
        {
            if (!shouldPatrol) return;
            if (_playerNoticed) return;
            
            if (navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                navAgent.destination = patrolPoints[Random.Range(0, patrolPoints.Length)].position;
            }
        }

        private void ChaseLogic()
        {
            if (_playerNoticed)
            {
                navAgent.destination = player.transform.position;
            }
        }
        
        private void NoticePlayerLogic() 
        {
            if (!shouldPatrol) return;
            
            var direction = player.transform.position - transform.position;
            _playerNoticed = false;
            RaycastHit hit;
            
            if (Vector3.Angle(transform.forward, direction) < viewAngle)
            {
                if (Physics.Raycast(transform.position + Vector3.up, direction, out hit))
                {
                    if (hit.collider.gameObject == player.gameObject)
                    {
                        _playerNoticed = true;
                    }
                }
            }
        }
    }
}
