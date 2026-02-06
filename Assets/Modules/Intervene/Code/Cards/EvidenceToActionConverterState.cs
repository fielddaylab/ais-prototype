using BeauUtil;
using FieldDay;
using FieldDay.SharedState;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AIS.Intervene.EvidenceToActionConverterState;

namespace AIS.Intervene
{
    public class EvidenceToActionConverterState : SharedStateComponent, IRegistrationCallbacks
    {
        #region Structs & Enums

        public enum CheckCondition
        {
            AlwaysTrue,
            AlwaysFalse,
        }

        [Serializable]
        public struct ActionResultCheck {
            public SerializedHash32 ActionCardId;
            public CheckCondition Condition;
        }

        [Serializable]
        public struct ConversionEntry {
            public SerializedHash32 EvidenceSource; // The id of the Evidence Card being converted
            public ActionResultCheck[] ActionResultChecks; // the list of Action Cards that are unlocked if checks pass
        }

        #endregion // Structs & Enums

        public ConversionEntry[] ConversionEntries; // TODO: load this dynamically

        public Dictionary<SerializedHash32, ConversionEntry> ConversionEntriesMap;

        #region IRegistration Callbacks

        public void OnRegister()
        {
            ConversionEntriesMap = new Dictionary<SerializedHash32, ConversionEntry>();
            EvidenceActionConvertUtility.LoadConversionEntries(this, null);
        }

        public void OnDeregister()
        {

        }

        #endregion // IRegistration Callbacks
    }

    public static class EvidenceActionConvertUtility
    {
        // TODO: load conversion entries from an external file or external object
        public static void LoadConversionEntries(EvidenceToActionConverterState state, TextAsset conversionEntries)
        {
            state.ConversionEntriesMap.Clear();

            // TODO: implement conversion parsing
        }

        public static List<SerializedHash32> ConvertEvidenceToActionCardIds(EvidenceToActionConverterState state, SerializedHash32 evidenceCardId)
        {
            List<SerializedHash32> actionIds = new List<SerializedHash32>();

            if (state.ConversionEntriesMap == null) {
                Debug.LogWarning("[EvidenceActionConvertUtility] Conversion Entry Map is empty or has not been initialized!");
                return actionIds; 
            }

            if (state.ConversionEntriesMap.ContainsKey(evidenceCardId))
            {
                var conversionEntry = state.ConversionEntriesMap[evidenceCardId];
                foreach (var actionResultCheck in conversionEntry.ActionResultChecks)
                {
                    if (DoesActionResultCheckPass(actionResultCheck))
                    {
                        actionIds.Add(actionResultCheck.ActionCardId);
                    }
                }
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("[EvidenceActionConvertUtility] Key (" + evidenceCardId.ToDebugString() + ") not present in dictionary!");
#endif // UNITY_EDITOR
            }

            return actionIds;
        }

        public static bool DoesActionResultCheckPass(ActionResultCheck toCheck)
        {
            switch (toCheck.Condition)
            {
                case CheckCondition.AlwaysTrue:
                    return true;
                case CheckCondition.AlwaysFalse:
                    return false;
                // TODO: additional types of checks here as needed
                default:
                    return true;
            }
        }
    }
}