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
        [SerializeField] private PathwayDef[] m_AllPathwayDefs;

        private Dictionary<SerializedHash32, SpeciesDef> m_SpeciesMap;
        private Dictionary<PathwayType, PathwayDef> m_PathwayMap;
        private PathwayDef m_HiddenPathwayDef;

        private void Awake()
        {
            Instance = this;

            if (m_SpeciesMap == null) {
                BuildSpeciesLookup();
            }

            if (m_PathwayMap == null) {
                BuildPathwayLookup();
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

        private void BuildPathwayLookup()
        {
            if (m_PathwayMap == null)
            {
                m_PathwayMap = new Dictionary<PathwayType, PathwayDef>(m_AllPathwayDefs.Length);
                foreach (var def in m_AllPathwayDefs)
                {
                    Log.Msg("[SpriteLibrary] Adding sprite {0}", def.name);
                    m_PathwayMap.Add(def.PathType, def);
                    if (def.IsHidden) { m_HiddenPathwayDef = def; }
                }
            }
        }

        public Sprite LookupSpeciesIcon(SerializedHash32 speciesId)
        {
            if (m_SpeciesMap == null) {
                BuildSpeciesLookup();
            }
            SpeciesDef def;
            m_SpeciesMap.TryGetValue(speciesId, out def);

            return def.Sprite;
        }

        public string LookupSpeciesDisplayName(SerializedHash32 speciesId)
        {
            if (m_SpeciesMap == null)
            {
                BuildSpeciesLookup();
            }
            SpeciesDef def;
            m_SpeciesMap.TryGetValue(speciesId, out def);

            return def.DisplayName;
        }

        public Sprite LookupPathwayIcon(PathwayType pathType, bool isHidden = false)
        {
            if (isHidden) { return m_HiddenPathwayDef.Sprite; }

            if (m_AllPathwayDefs == null) {
                BuildPathwayLookup();
            }
            PathwayDef def;
            m_PathwayMap.TryGetValue(pathType, out def);

            return def.Sprite;
        }
    }
}