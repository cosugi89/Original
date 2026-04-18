using System;
using Assets.Scripts.Core;
using UnityEngine;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// �V���O���g����PlayerController
    /// - �L�����N�^�[�p�[�c�̃Z�b�g�A�b�v
    /// - GameMode�̐؂�ւ�
    /// - Experience���[�h�ɂ�����N���b�N�ړ�
    /// </summary>
    public class Player : MonoBehaviour
    {
        private const string ANIM_IDLE = "Idle";
        private const string ANIM_WALK = "Walk";
        private const float MIN_DIRECTION_THRESHOLD = 0.01f;
        private const float ARRIVAL_DISTANCE = 0.1f;

        public static Player Instance { get; private set; }

        [SerializeField] private PartsManager partsManager;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private float moveSpeed = 3f;

        public PartsManager PartsManager => partsManager;   // �L�����N�^�[�J�X�^�}�C�Y�p
        public event Action<GameMode> OnModeChanged;        // �Q�[�����[�h�̕ύX

        private GameMode currentMode = GameMode.Home;
        private Vector2 moveTarget;
        private bool isMoving;
        private Camera _mainCamera;

        private void Awake()
        {
            // �d�����Đ������ꂽ Player �N���X���폜���Ă����o�^
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _mainCamera = Camera.main;
        }

        public void Init()
        {
            // PartsManager �̃Z�b�g�A�b�v
            if (partsManager != null)
                partsManager.Init();

            SetMode(GameMode.Home);
        }

        public void SetMode(GameMode mode)
        {
            currentMode = mode;
            isMoving = false;

            // �ړ����~����
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            // �A�j���[�V���������Z�b�g
            if (mode == GameMode.Home && partsManager != null)
                partsManager.PlayAnimation(ANIM_IDLE);

            OnModeChanged?.Invoke(mode);
        }

        private void Update()
        {
            if (currentMode == GameMode.Experience)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    HandleClick();
                }

                if (isMoving)
                {
                    UpdateMovement();
                }
            }
        }

        #region GameMode.Experience
        private void HandleClick()
        {
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            moveTarget = worldPos;
            isMoving = true;

            if (partsManager != null)
                partsManager.PlayAnimation(ANIM_WALK);

            Vector3 scale = transform.localScale;
            float dir = moveTarget.x - transform.position.x;
            if (Mathf.Abs(dir) > MIN_DIRECTION_THRESHOLD)
                transform.localScale = new Vector3(Mathf.Sign(dir) * Mathf.Abs(scale.x), scale.y, scale.z);
        }

        private void UpdateMovement()
        {
            Vector2 current = transform.position;
            Vector2 direction = (moveTarget - current);
            float distance = direction.magnitude;

            if (distance < ARRIVAL_DISTANCE)
            {
                isMoving = false;
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;

                if (partsManager != null)
                    partsManager.PlayAnimation(ANIM_IDLE);
                return;
            }

            if (rb != null)
                rb.linearVelocity = direction.normalized * moveSpeed;
        }
        #endregion GameMode.Experience
    }
}
