using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Intervene
{
    /// <summary>
    /// System that enables the player to specify how to use their actions.
    /// </summary>
    public class ActionEffectSpecifierSystem : MonoBehaviour
    {
        #region Unity Callbacks

        private void Awake()
        {
            AisGame.Events.Register(InterveneEvents.OnEffectSpecifyCancel, HandleEffectSpecifyCancel);
        }

        #endregion // Unity Callbacks

        public void ResetChoices()
        {
            
        }

        #region Handlers

        private void HandleEffectSpecifyCancel()
        {
            ResetChoices();
        }

        #endregion // Handlers
    }
}
