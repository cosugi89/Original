using UnityEngine;

namespace Assets.Scripts.Samples.CharacterMaker.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraControl : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float cameraSize = 5f;
        [SerializeField] private Vector3 offset = new(0, 1, -10);

        private UnityEngine.Camera _camera;

        public void Init(Transform followTarget)
        {
            target = followTarget;
            _camera = GetComponent<UnityEngine.Camera>();
            _camera.orthographicSize = cameraSize;
            transform.position = target.position + offset;
        }

        private void LateUpdate()
        {
            if (target is null || _camera is null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            _camera.orthographicSize = cameraSize;
        }
    }
}
