using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TPSgameAssets.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("PLAYER STATS")] 
        public int enemiesKilled = 0;
        [SerializeField] private int playerHealth;
        [SerializeField] private int maxPlayerHealth = 100;
        public bool isPlayerDead;
        
        [Header("INPUT / MOVEMENT")] 
        public float movementSpeed;
        public float rotationSpeed;
        public float sprintMultiplier = 1f;
        public float jumpForce = 5f;
        public Transform lookTargetObject;
        public Transform aimLookTargetObject;

        [SerializeField] private float gravityValue = -9.81f;
        private GameInput _gameInput;
        private CharacterController _controller;
        private bool _isWalking;
        private bool _isJumping;
        private bool _isMovePressed;
        private bool _isSprinting;
        private Vector3 _gravity;
        private bool _isPlayerGrounded;
        private Vector3 _moveDirection;
        private Vector3 _verticalDirection;

        [Header("CAMERA / AIMING")] 
        public CinemachineVirtualCamera cinemachineCamera;
        private Quaternion _lookRotation;
        private float _initialFov;
        private Vector2 _mouseInput;
        private bool _isAiming;
        public float mouseSensetivity;

        [Header("CLAMP RANGES")] 
        public float verticalMinRange;
        public float verticalMaxRange;
        private float _verticalRotation;
        private float _horizontalRotation;

        [Header("Shooting")] 
        public GameObject projectileSpawner;
        [Space]
        public GameObject[] spellsList;
        [Space]
        public Cooldown castDelay;
        
        [Header("ANIMATIONS")]
        private Animator _animator;
        private int _isWalkingHash;
        private int _isJumpingHash;
        private Vector2 _inputVector2;
        private static readonly int IsWalking = Animator.StringToHash("isWalking");
        private static readonly int IsJumping = Animator.StringToHash("isJumping");

        [Header("AUDIO")]
        private AudioSource _spellCasterSRC;


        [Header("UI")] 
        public GameObject gameUI;
        public GameObject deathUI;
        
        [SerializeField] private GameObject uiCroshair;
        [SerializeField] private Slider uiHealthbar;
        
        
        
        private void Awake()
        {
            _gameInput = new GameInput();
            
            // Get components
            _controller = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();
            _spellCasterSRC = projectileSpawner.GetComponent<AudioSource>();
            
            
            #region InputSystem stuff
            // Movement & Keyboard inputs
            _gameInput.Player.Movement.performed += ctx =>
            {
                _inputVector2 = ctx.ReadValue<Vector2>();
                _isMovePressed = true;
            };
            _gameInput.Player.Movement.canceled += ctx =>
            {
                _inputVector2 = Vector2.zero;
                _isMovePressed = false;
            };
            
            
            // Jumping
            _gameInput.Player.Jump.performed += JumpHandler;
            
            // Mouse inputs
            _gameInput.Camera.Mouse.performed += ctx => _mouseInput = ctx.ReadValue<Vector2>();
            _gameInput.Camera.Mouse.canceled += ctx => _mouseInput = Vector2.zero;

            _gameInput.Player.Aiming.performed += ctx =>
            {
                _isAiming = true;
            };
            _gameInput.Player.Aiming.canceled += ctx =>
            {
                _isAiming = false;
            };

            // Shooting
            _gameInput.Player.Shoot.performed += CastProjectile;
            #endregion
        }
        
        private void Start()
        {
            _gravity = new Vector3(0f, gravityValue, 0f);
            _initialFov = cinemachineCamera.m_Lens.FieldOfView;
            playerHealth = maxPlayerHealth;
            uiHealthbar.maxValue = maxPlayerHealth;
            
            SetHealthBar(playerHealth);
            
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void OnEnable()
        {
            _gameInput.Player.Enable();
            _gameInput.Camera.Enable();
        }
        private void OnDisable()
        {
            _gameInput.Player.Disable();
            _gameInput.Camera.Disable();
        }
        
        
        private void Update()
        {
            Move();
            Aiming();
            Rotation();
            LookTargetRotation();
            
            // Gravity.
            if (_isPlayerGrounded && _verticalDirection.y < 0)
            {
                _verticalDirection.y = -1f;
            }
            _verticalDirection.y += gravityValue * Time.fixedDeltaTime;
            
            // Move player.
            _controller.Move(_verticalDirection * Time.deltaTime);
        }

        private void Move()
        {
            _isPlayerGrounded = _controller.isGrounded;
            
            _moveDirection = Quaternion.Euler(0, lookTargetObject.transform.eulerAngles.y, 0) *  new Vector3(_inputVector2.x, 0f, _inputVector2.y);
            _controller.Move(_moveDirection * (movementSpeed * Time.deltaTime));

            #region MovementAnimations
            _isWalking = _animator.GetBool(IsWalking);
            
            
            if (!_isWalking && _isMovePressed)
            {
                _animator.SetBool(IsWalking, true);
            }

            if (_isWalking && !_isMovePressed)
            {
                _animator.SetBool(IsWalking, false);
            }
            
            // STOP JUMPING! AЪАЪАЪАЪ
            if (_verticalDirection.y <= 0) _animator.SetBool(IsJumping, false);
            #endregion
        }

        private void LookTargetRotation()
        {
            _verticalRotation += -_mouseInput.y * mouseSensetivity * Time.deltaTime;
            _horizontalRotation += _mouseInput.x * mouseSensetivity * Time.deltaTime;
            var angles = lookTargetObject.transform.localEulerAngles;
            angles.z = 0;

            // Be careful, very scary and disgusting construction below:
            float verticalRotation = Mathf.Clamp(_verticalRotation, verticalMinRange, verticalMaxRange);
            if (_verticalRotation > verticalMaxRange)
            {
                _verticalRotation = verticalMaxRange;
            }
            if (_verticalRotation < verticalMinRange)
            {
                _verticalRotation = verticalMinRange;
            }
            
            _lookRotation = Quaternion.Euler(0, lookTargetObject.transform.rotation.y, 0);
            
            lookTargetObject.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
            lookTargetObject.transform.rotation = Quaternion.Euler(verticalRotation, _horizontalRotation, 0);
            
            aimLookTargetObject.transform.localEulerAngles = new Vector3(angles.x, 0, 0);
            aimLookTargetObject.transform.rotation = Quaternion.Euler(verticalRotation, _horizontalRotation, 0);
        }

        private void Rotation()
        {
            Quaternion newRotation;
            
            if (_isAiming)
            {
                Quaternion yAxis = Quaternion.Euler(0, _horizontalRotation, 0);
                newRotation = Quaternion.Slerp(transform.rotation, yAxis,
                    Time.deltaTime * rotationSpeed); // Rotate in camera look direction while aiming.
                transform.rotation = newRotation;
            }
            else
            {
                if (_inputVector2.sqrMagnitude == 0) return; // if player is moving.
                newRotation = Quaternion.LookRotation(_moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * rotationSpeed);
            }
        }

        private void Aiming()
        {
            // Manipulations with camera FOV.
            // TODO: Add smoothing.
            if (_isAiming)
            {
                cinemachineCamera.m_Lens.FieldOfView = _initialFov - 15;
                cinemachineCamera.Follow = aimLookTargetObject;
                uiCroshair.SetActive(true);
                
                projectileSpawner.SetActive(true);
                
            }
            else
            {
                cinemachineCamera.m_Lens.FieldOfView = _initialFov;
                cinemachineCamera.Follow = lookTargetObject;
                uiCroshair.SetActive(false);
                
                projectileSpawner.SetActive(false);
            }
        }
        
        private void CastProjectile(InputAction.CallbackContext ctx)
        {
            var spawnTransform = projectileSpawner.transform;
            var spawnPos = spawnTransform.position;
            var spawnPosRotation = spawnTransform.rotation;
            
            if (ctx.performed && _isAiming && !castDelay.IsInCooldown)
            {
                castDelay.StartCooldown();
                _spellCasterSRC.Play();
                Instantiate(spellsList[0], spawnPos, spawnPosRotation); // Just a template for adding more spells.
            }
        }
        
        private void JumpHandler(InputAction.CallbackContext ctx)
        {
            if (_isPlayerGrounded)
            {
                _verticalDirection.y += jumpForce * -1.0f * gravityValue;
               if (ctx.performed) _animator.SetBool(IsJumping, true);
               
            }
        }

        #region Outside interactions
        // There is methods that can be initiated from outside. Like takeing damage or healing.
        public void TakeDamage(int damage)
        {
            playerHealth -= damage;
            SetHealthBar(playerHealth);
            
                        
            if (playerHealth <= 0)
            {
                KillPlayer();
            }
        }

        public void HealHealth(int healamount)
        {
            if (playerHealth <= 0) return;
            if (playerHealth >= 100) return;
            
            playerHealth += healamount;
            SetHealthBar(playerHealth);
        }
        #endregion

        private void SetHealthBar(int health)
        {
            uiHealthbar.value = health;
        }
        
        private void KillPlayer()
        {
            print("PLAYER ARE DEAD!");
            isPlayerDead = true;
            
            gameUI.SetActive(false);
            deathUI.SetActive(true);

            _gameInput.Player.Disable();
            
            _controller.enabled = false;
            cinemachineCamera.enabled = false;
        }
    }
}
