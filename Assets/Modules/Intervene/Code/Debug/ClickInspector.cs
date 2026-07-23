using System.Collections.Generic;
using System.Text;
using FieldDay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AIS.Intervene {
    /// <summary>
    /// Dev-only diagnostic. Drop onto any GameObject in the Intervene scene and, on each mouse
    /// click, it logs everything sitting under the cursor and what would actually consume the
    /// click. Use it to track down cases where a click on a SpriteRenderer/Collider2D is being
    /// swallowed by something else — usually a UI Graphic with a raycast target laid over the
    /// world, or another 2D collider stacked on top.
    ///
    /// A click can be blocked by two independent systems:
    ///   - UI: any raycast-target Graphic under the cursor is hit by the EventSystem and, if it's
    ///     the topmost, eats the pointer event before it ever reaches world objects.
    ///   - World: multiple Collider2Ds can overlap the same point; the one drawn on top (highest
    ///     sorting layer / order) is the one you visually expect to click.
    /// Both stacks are logged, topmost-first, so the blocker is obvious.
    /// </summary>
    public sealed class ClickInspector : MonoBehaviour {
        [Tooltip("Camera used to convert the mouse position into world space. Falls back to the primary world camera (the panning/zooming ecosystem camera) when empty — not Camera.main, which is unreliable in this multi-scene setup.")]
        [SerializeField] private Camera m_Camera;
        [Tooltip("Which mouse button to inspect. 0 = left, 1 = right, 2 = middle.")]
        [SerializeField] private int m_MouseButton = 0;
        [Tooltip("Log UI elements (raycast-target Graphics) under the cursor via the EventSystem.")]
        [SerializeField] private bool m_InspectUI = true;
        [Tooltip("Log Collider2Ds under the cursor.")]
        [SerializeField] private bool m_Inspect2D = true;

        // Reused across clicks so the diagnostic doesn't allocate a fresh buffer every time.
        private readonly List<RaycastResult> m_UIResults = new List<RaycastResult>();
        private readonly StringBuilder m_Builder = new StringBuilder(256);

        private void Update() {
            if (!Input.GetMouseButtonDown(m_MouseButton)) {
                return;
            }

            Camera cam = m_Camera != null ? m_Camera : Game.Rendering.PrimaryCamera;
            Vector3 screenPos = Input.mousePosition;

            m_Builder.Clear();
            m_Builder.Append("[ClickInspector] click at screen ").Append((Vector2)screenPos);
            if (cam != null) {
                Vector3 world = cam.ScreenToWorldPoint(screenPos);
                m_Builder.Append(" / world (").Append(world.x.ToString("0.00")).Append(", ").Append(world.y.ToString("0.00")).Append(')');
            } else {
                m_Builder.Append(" / world <no camera>");
            }

            string topmostUI = m_InspectUI ? AppendUIHits(screenPos) : null;
            string topmost2D = (m_Inspect2D && cam != null) ? Append2DHits(cam, screenPos) : null;

            // The EventSystem consumes the pointer event before world objects see it, so a UI hit
            // always wins over a collider hit when deciding what "gets clicked".
            m_Builder.Append("\n=> Receives the click: ");
            if (topmostUI != null) {
                m_Builder.Append(topmostUI).Append("  (UI — blocks world input beneath it)");
            } else if (topmost2D != null) {
                m_Builder.Append(topmost2D).Append("  (2D collider)");
            } else {
                m_Builder.Append("nothing");
            }

            Debug.Log(m_Builder.ToString());
        }

        // Returns the name of the topmost UI element, or null if none. Appends the full UI stack.
        private string AppendUIHits(Vector3 screenPos) {
            m_Builder.Append("\nUI (EventSystem, topmost first): ");
            if (EventSystem.current == null) {
                m_Builder.Append("<no EventSystem in scene>");
                return null;
            }

            PointerEventData pointer = new PointerEventData(EventSystem.current) { position = screenPos };
            m_UIResults.Clear();
            EventSystem.current.RaycastAll(pointer, m_UIResults);

            if (m_UIResults.Count == 0) {
                m_Builder.Append("none");
                return null;
            }

            for (int i = 0; i < m_UIResults.Count; i++) {
                GameObject go = m_UIResults[i].gameObject;
                m_Builder.Append("\n  ").Append(i + 1).Append(". ").Append(GetPath(go.transform));
            }
            return m_UIResults[0].gameObject.name;
        }

        // Returns the name of the topmost Collider2D, or null if none. Appends the full collider stack.
        private string Append2DHits(Camera cam, Vector3 screenPos) {
            m_Builder.Append("\n2D colliders (topmost first): ");
            Vector2 worldPoint = cam.ScreenToWorldPoint(screenPos);
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);

            if (hits.Length == 0) {
                m_Builder.Append("none");
                return null;
            }

            // Order by draw order so the collider you visually see on top is reported first.
            System.Array.Sort(hits, (a, b) => SortKey(b).CompareTo(SortKey(a)));

            for (int i = 0; i < hits.Length; i++) {
                Collider2D col = hits[i];
                m_Builder.Append("\n  ").Append(i + 1).Append(". ").Append(GetPath(col.transform));
                Renderer r = col.GetComponent<Renderer>();
                if (r != null) {
                    m_Builder.Append("  [layer '").Append(r.sortingLayerName).Append("', order ").Append(r.sortingOrder).Append(']');
                }
            }
            return hits[0].name;
        }

        // Higher = drawn on top. Combines sorting layer value and order into one comparable long;
        // colliders with no renderer sort to the bottom.
        private static long SortKey(Collider2D col) {
            Renderer r = col.GetComponent<Renderer>();
            if (r == null) {
                return long.MinValue;
            }
            return ((long)SortingLayer.GetLayerValueFromID(r.sortingLayerID) << 32) + r.sortingOrder;
        }

        // Full hierarchy path (e.g. "Canvas/Panel/Button") so duplicate names are distinguishable.
        private static string GetPath(Transform t) {
            string path = t.name;
            while (t.parent != null) {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
