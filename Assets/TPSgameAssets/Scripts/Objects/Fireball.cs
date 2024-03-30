using TPSgameAssets.Scripts.Enemy;
using UnityEngine;

namespace TPSgameAssets.Scripts.Objects
{
    public class Fireball : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private float maxDistance;
        [SerializeField] private float lifeTime;
        [SerializeField] public int damage = 25;
        
        private float _initialPos;
        
        private void Start()
        {
            Invoke(nameof(DestroyFireball), lifeTime);
        }

        public void Update()
        {
            FireballMovement();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                DamageEnemy(collision);
                DestroyFireball();
            }
            DestroyFireball();
            UnityEngine.Debug.Log($"Collision of {collision.gameObject.name} is touched!");
        }

        private void FireballMovement()
        {
            var transform1 = transform;
            
            transform1.position += transform1.forward * (speed * Time.deltaTime);
        }

        private void DestroyFireball()
        {
            Destroy(gameObject);
        }

        private void DamageEnemy(Collision collisionObject)
        {
            collisionObject.gameObject.GetComponent<EnemyController>().DamageEnemy(damage);
        }
    }
}
