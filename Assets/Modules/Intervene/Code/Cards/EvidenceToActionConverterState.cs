using BeauRoutine;
using BeauUtil;
using FieldDay;
using FieldDay.Scenes;
using FieldDay.SharedState;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AIS.Intervene.EvidenceToActionConverterState;

namespace AIS.Intervene
{
    public class EvidenceToActionConverterState : SharedStateComponent, IScenePreload
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

            public ConversionEntry(SerializedHash32 evidenceSrc, ActionResultCheck[] checks)
            {
                EvidenceSource = evidenceSrc;
                ActionResultChecks = checks;
            }
        }

        #endregion // Structs & Enums

        public TextAsset[] ConversionDefs; // TODO: load this dynamically

        public Dictionary<SerializedHash32, ConversionEntry> ConversionEntriesMap;

        #region IPreload

        private void Awake()
        {
            ConversionEntriesMap = new Dictionary<SerializedHash32, ConversionEntry>();

            // Populate Card data
            EvidenceActionConvertUtility.PopulateConversions(this);
        }

        public IEnumerator<WorkSlicer.Result?> Preload()
        {
            yield return null;

            /*
            ConversionEntriesMap = new Dictionary<SerializedHash32, ConversionEntry>();

            // Populate Card data
            var populate = Async.Schedule(EvidenceActionConvertUtility.PopulateConversions(this), AsyncFlags.HighPriority | AsyncFlags.MainThreadOnly);
            Game.Scenes.RegisterLoadDependency(populate);
            */
        }

        #endregion // IPreload
    }

    public static class EvidenceActionConvertUtility
    {
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

        // Returns the FIRST evidence source whose conversion lists this action; default if none.
        public static SerializedHash32 GetEvidenceSourceForAction(EvidenceToActionConverterState state, SerializedHash32 actionCardId)
        {
            if (state == null || state.ConversionEntriesMap == null)
            {
                return default;
            }

            foreach (var entry in state.ConversionEntriesMap.Values)
            {
                foreach (var actionResultCheck in entry.ActionResultChecks)
                {
                    if (actionResultCheck.ActionCardId.Equals(actionCardId))
                    {
                        return entry.EvidenceSource;
                    }
                }
            }

            return default;
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

        #region Card Definition Parsing

        private static readonly string SRC_TAG = "@from";
        private static readonly string RESULT_TAG = "@to";

        private static readonly string ENTRY_SEP = "::";
        private static readonly string RESULTS_SEP = ",";

        private static readonly char[] END_DELIMS = new char[] { '\r', '\n' };

        #endregion // Card Definition Parsing

        static public void PopulateConversions(EvidenceToActionConverterState converterState)
        {
            List<string> conversionStrings;

            foreach (TextAsset conversionDef in converterState.ConversionDefs)
            {
                conversionStrings = TextIO.TextAssetToList(conversionDef, ENTRY_SEP);

                foreach (string str in conversionStrings)
                {
                    try
                    {
                        ConversionEntry newConversion = ConvertDefToConversion(str);

                        converterState.ConversionEntriesMap.Add(newConversion.EvidenceSource, newConversion);

                        Debug.Log("[EvidenceToActionConvertUtility] added " + newConversion.EvidenceSource + ".");
                    }
                    catch (Exception e)
                    {
                        Debug.Log("[EvidenceToActionConvertUtility] Parsing error! " + e.Message);
                    }
                    // yield return null;
                }
            }
        }

        static private ConversionEntry ConvertDefToConversion(string conversionDef)
        {
            Debug.Log("[CardUtility] forming conversion: " + conversionDef);
            // SerializedHash32 cardID = "";
            string src = "";
            string results = "";
            List<ActionResultCheck> checks = new List<ActionResultCheck>();

            // Parse into data

            // Source comes after @from
            int srcIndex = conversionDef.ToLower().IndexOf(SRC_TAG);
            if (srcIndex != -1)
            {
                string afterSrc = conversionDef.Substring(srcIndex);
                int offset = SRC_TAG.Length;
                src = conversionDef.Substring(srcIndex + offset, afterSrc.IndexOfAny(END_DELIMS) - offset).Trim();
            }
            else
            {
                // syntax error
                Debug.Log("[CardUtility] source syntax error!");

                throw new Exception("Source");
            }

            // Results comes after @to
            int resultsIndex = conversionDef.ToLower().IndexOf(RESULT_TAG);

            if (resultsIndex != -1)
            {
                string afterResults = conversionDef.Substring(resultsIndex);
                int offset = RESULT_TAG.Length;
                results = conversionDef.Substring(resultsIndex + offset, afterResults.IndexOfAny(END_DELIMS) - offset).Trim();

                // split by separator
                var splitResults = results.Split(RESULTS_SEP);

                foreach (var splitResult in splitResults)
                {
                    ActionResultCheck newCheck = new ActionResultCheck();
                    newCheck.ActionCardId = splitResult.Trim();

                    // TODO: handle check conditions if needed

                    checks.Add(newCheck);
                }
            }
            else
            {
                // syntax error
                Debug.Log("[CardUtility] checks syntax error!");

                throw new Exception("Description");
            }

            ActionResultCheck[] arrChecks = new ActionResultCheck[checks.Count];
            for (int i = 0; i < checks.Count; i++)
            {
                arrChecks[i] = checks[i];
            }

            return new ConversionEntry(src, arrChecks);
        }
    }
}