using BeauPools;
using BeauRoutine;
using System;
using System.Collections;
using UnityEngine;

namespace AIS.Model {
    [Serializable]
    public sealed class SparkleEffectPool : SerializablePool<SparkleEffect> { }

    public sealed class SparkleEffect : MonoBehaviour {
        [SerializeField] private SpriteRenderer[] m_Renderers;

        // Called by the pool owner before playing. Positions the effect and resets renderer state.
        public void Prepare(Vector3 worldPos, Sprite sprite) {
            transform.position = worldPos;
            foreach (var r in m_Renderers) {
                r.sprite = sprite;
                r.transform.localScale = Vector3.zero;
                Color c = r.color;
                c.a = 1f;
                r.color = c;
            }
        }

        public IEnumerator Play() {
            var scaleAnims = new IEnumerator[m_Renderers.Length];
            for (int i = 0; i < m_Renderers.Length; i++)
                scaleAnims[i] = m_Renderers[i].transform.ScaleTo(0.6f, 0.25f);
            yield return Routine.Combine(scaleAnims);

            var fadeAnims = new IEnumerator[m_Renderers.Length];
            for (int i = 0; i < m_Renderers.Length; i++)
                fadeAnims[i] = m_Renderers[i].FadeTo(0f, 0.3f);
            yield return Routine.Combine(fadeAnims);
        }
    }
}
