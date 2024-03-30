using System;
using TPSgameAssets.Scripts.Player;
using UnityEngine;

namespace TPSgameAssets.Scripts.Debug
{
    public class DealDamage : MonoBehaviour
    {
        public int damageAmount = 15;
        public Cooldown cooldown;
        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            if (cooldown.IsInCooldown) return;
            
            var player = other.GetComponent<PlayerController>();
            player.TakeDamage(damageAmount);
        }
    }
}
