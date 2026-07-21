using UnityEngine;

namespace AIS.Intervene {
    /// <summary>
    /// Attach to a Camera to let the player pan with WASD / arrow keys and zoom with the mouse
    /// scroll wheel. Panning is clamped to a rectangular world-space region, and zoom is clamped
    /// between a minimum and maximum. Built for the project's orthographic 2D cameras, but also
    /// works on a perspective camera (zoom then drives the field of view).
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraPanZoomController : MonoBehaviour {
        [Header("Pan")]
        [Tooltip("Pan speed in world units per second at the most zoomed-out level.")]
        [SerializeField] private float m_PanSpeed = 8f;
        [Tooltip("When enabled, panning slows as you zoom in so the on-screen pan rate stays consistent.")]
        [SerializeField] private bool m_ScalePanSpeedWithZoom = true;
        [Tooltip("Minimum (x, y) world position the camera can pan to.")]
        [SerializeField] private Vector2 m_PanBoundsMin = new Vector2(-10f, -10f);
        [Tooltip("Maximum (x, y) world position the camera can pan to.")]
        [SerializeField] private Vector2 m_PanBoundsMax = new Vector2(10f, 10f);

        [Header("Zoom")]
        [Tooltip("Orthographic size (or field of view) at the most zoomed-in point.")]
        [SerializeField] private float m_MinZoom = 2.5f;
        [Tooltip("Orthographic size (or field of view) at the most zoomed-out point.")]
        [SerializeField] private float m_MaxZoom = 10f;
        [Tooltip("How much a single notch of scroll changes the zoom.")]
        [SerializeField] private float m_ZoomSpeed = 4f;
        [Tooltip("Seconds to ease toward the target zoom. 0 = instant.")]
        [SerializeField] private float m_ZoomSmoothTime = 0.12f;

        private Camera m_Camera;
        private float m_TargetZoom;
        private float m_ZoomVelocity;

        private void Awake() {
            m_Camera = GetComponent<Camera>();
            m_TargetZoom = Mathf.Clamp(GetZoom(), m_MinZoom, m_MaxZoom);
            SetZoom(m_TargetZoom);
        }

        private void Update() {
            HandlePan();
            HandleZoom();
        }

        private void HandlePan() {
            Vector2 input = ReadPanInput();
            if (input == Vector2.zero) {
                return;
            }

            float speed = m_PanSpeed;
            if (m_ScalePanSpeedWithZoom && m_Camera.orthographic && m_MaxZoom > 0f) {
                // Slower panning while zoomed in keeps the perceived on-screen pan rate consistent.
                speed *= m_Camera.orthographicSize / m_MaxZoom;
            }

            Vector3 pos = transform.position;
            pos.x += input.x * speed * Time.unscaledDeltaTime;
            pos.y += input.y * speed * Time.unscaledDeltaTime;
            transform.position = ClampToBounds(pos);
        }

        // Reads WASD and arrow keys directly so panning works regardless of the project's input axis setup.
        private static Vector2 ReadPanInput() {
            Vector2 input = Vector2.zero;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x += 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) input.y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) input.y -= 1f;
            // Normalize so diagonal movement isn't faster than moving along a single axis.
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private void HandleZoom() {
            float scroll = Input.mouseScrollDelta.y;
            if (!Mathf.Approximately(scroll, 0f)) {
                // Scroll up zooms in (smaller orthographic size / narrower field of view).
                m_TargetZoom = Mathf.Clamp(m_TargetZoom - scroll * m_ZoomSpeed, m_MinZoom, m_MaxZoom);
            }

            float current = GetZoom();
            float next = m_ZoomSmoothTime > 0f
                ? Mathf.SmoothDamp(current, m_TargetZoom, ref m_ZoomVelocity, m_ZoomSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime)
                : m_TargetZoom;
            SetZoom(next);

            // Zooming out can widen the view past the pan limits, so re-clamp the position.
            transform.position = ClampToBounds(transform.position);
        }

        private Vector3 ClampToBounds(Vector3 pos) {
            pos.x = Mathf.Clamp(pos.x, m_PanBoundsMin.x, m_PanBoundsMax.x);
            pos.y = Mathf.Clamp(pos.y, m_PanBoundsMin.y, m_PanBoundsMax.y);
            return pos;
        }

        private float GetZoom() {
            return m_Camera.orthographic ? m_Camera.orthographicSize : m_Camera.fieldOfView;
        }

        private void SetZoom(float value) {
            if (m_Camera.orthographic) {
                m_Camera.orthographicSize = value;
            } else {
                m_Camera.fieldOfView = value;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (m_MaxZoom < m_MinZoom) m_MaxZoom = m_MinZoom;
            m_PanBoundsMax.x = Mathf.Max(m_PanBoundsMax.x, m_PanBoundsMin.x);
            m_PanBoundsMax.y = Mathf.Max(m_PanBoundsMax.y, m_PanBoundsMin.y);
        }

        private void OnDrawGizmosSelected() {
            // Visualize the pan bounds in the Scene view when this camera is selected.
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (m_PanBoundsMin.x + m_PanBoundsMax.x) * 0.5f,
                (m_PanBoundsMin.y + m_PanBoundsMax.y) * 0.5f,
                transform.position.z);
            Vector3 size = new Vector3(m_PanBoundsMax.x - m_PanBoundsMin.x, m_PanBoundsMax.y - m_PanBoundsMin.y, 0f);
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}
