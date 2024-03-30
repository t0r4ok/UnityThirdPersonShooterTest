using UnityEngine;

namespace TPSgameAssets.Scripts
{
    [System.Serializable]
    public class Cooldown
    {
        [SerializeField] private float cooldownTime;
        private float _nextCDtime;

        public bool IsInCooldown => Time.time < _nextCDtime;
        public void StartCooldown() => _nextCDtime = Time.time + cooldownTime;
    }
}
