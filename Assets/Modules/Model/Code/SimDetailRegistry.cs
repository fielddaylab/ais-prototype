using System.Collections.Generic;
using BeauUtil;
using FieldDay;
using FieldDay.SharedState;
using UnityEngine;

namespace AIS.Narrative
{
    [CreateAssetMenu(menuName = "Narrative/Sim Detail")]
    public sealed class SimDetail : ScriptableObject
    {
        public StringHash32 EvidenceId;
        public GameObject Target;
    }

    public sealed class SimDetailRegistry : MonoBehaviour
    {
        public SimDetail[] Details;
        // TODO: setup evidence cards and details relations in unity editor.

        private void Awake()
        {
            HideAll();
        }

        public void HideAll()
        {
            foreach (SimDetail detail in Details)
            {
                detail.Target.SetActive(false);
            }
        }

        public void RefreshVisibility()
        {
            PlayerInventory inv = Find.State<PlayerInventory>();
            foreach (SimDetail detail in Details)
            {
                bool unlocked = inv.EvidenceCards.Contains(detail.EvidenceId);
                detail.Target.SetActive(unlocked);
            }
        }
    }
}

