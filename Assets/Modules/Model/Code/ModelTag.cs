using AIS.Intervene;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AIS.Model
{
    /// <summary>
    /// Data used by ActionEffectSpecifierSystem to manage action decisions
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ModelTag : MonoBehaviour
    {
        public ActionTarget TargetType;
        public SpriteRenderer Highlight;

        #region Unity Callbacks

        public void OnEnable()
        {
            ModelTagMgr.Instance.Register(this);
        }

        public void OnDisable()
        {
            if (AisGame.IsShuttingDown) { return; }

            ModelTagMgr.Instance.Deregister(this);
        }

        #endregion // Unity Callbacks

        #region Visuals

        public void ShowHighlight()
        {
            Highlight.enabled = true;
        }

        public void HideHighlight()
        {
            Highlight.enabled = false;
        }

        #endregion // Visuals
    }
}
