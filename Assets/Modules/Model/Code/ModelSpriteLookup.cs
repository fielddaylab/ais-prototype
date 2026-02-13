using BeauUtil;
using BeauUtil.Debugger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIS.Model
{
    public class ModelSpriteLookup : MonoBehaviour
    {
        public static ModelSpriteLookup Instance;

        [SerializeField] private SpeciesDef[] m_AllSpeciesDefs;

        private Dictionary<SerializedHash32, SpeciesDef> m_SpeciesMap;

        private void Awake()
        {
            Instance = this;

            if (m_SpeciesMap == null)
            {
                BuildSpeciesLookup();
            }
        }

        private void BuildSpeciesLookup()
        {
            if (m_SpeciesMap == null)
            {
                m_SpeciesMap = new Dictionary<SerializedHash32, SpeciesDef>(m_AllSpeciesDefs.Length, CompareUtils.DefaultEquals<SerializedHash32>());
                foreach (var def in m_AllSpeciesDefs)
                {
                    Log.Msg("[SpriteLibrary] Adding sprite {0}", def.name);
                    m_SpeciesMap.Add(def.SpeciesId, def);
                }
            }
        }

        public Sprite LookupSpeciesIcon(SerializedHash32 speciesId)
        {
            if (m_SpeciesMap == null)
            {
                BuildSpeciesLookup();
            }
            SpeciesDef def;
            m_SpeciesMap.TryGetValue(speciesId, out def);

            return def.Sprite;
        }
    }
}