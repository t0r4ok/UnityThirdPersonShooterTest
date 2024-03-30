
using TMPro;
using TPSgameAssets.Scripts.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TPSgameAssets.Scripts
{
    public class GameController : MonoBehaviour
    {
        public TextMeshProUGUI scoreBoardText;
        public TextMeshProUGUI finalScoreBoardText;
        
        private GameObject player;
        private GameInput _gameInput;
        private PlayerController playerController;


        private void OnEnable()
        {
            _gameInput.Player.Enable();
        }
        private void OnDisable()
        {
            _gameInput.Player.Disable();
        }

        private void Awake()
        {
            _gameInput = new GameInput();
            
            _gameInput.Player.Jump.performed += Scenenmanager;
        }

        private void FixedUpdate()
        {
            scoreBoardText.text = $"ENEMIES KILLED: {playerController.enemiesKilled}";

            if (playerController.isPlayerDead)
            {
                finalScoreBoardText.text = $"You killed {playerController.enemiesKilled} enemies!";
            }
        }

        private void Start()
        {
            player = GameObject.FindWithTag("Player");
            playerController = player.GetComponent<PlayerController>();
        }
        
        private void Scenenmanager(InputAction.CallbackContext ctx)
        {
            if (!playerController.isPlayerDead) return;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
