using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BeauUtil;
using BeauUtil.Debugger;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif // UNITY_EDITOR

namespace AIS.Narrative {
    /// <summary>
    /// Dev-only validator. On game start it scans every .leaf script under Narrative/Data for the
    /// speaker tags ({@Character}) they use, then checks each referenced character against the
    /// CharacterData assets under Narrative/Data/Characters. Any character named in a script that
    /// has no matching CharacterData is logged as a warning.
    ///
    /// It reads the raw .leaf / .asset files straight off disk (rather than waiting for leaf
    /// scripts and CharacterData packs to load dynamically at runtime), so it is editor-only and
    /// does nothing in a build - WebGL and other players ship no source Assets folder to scan.
    /// Attach it to the Boot object to have it run automatically when play starts.
    /// </summary>
    [AddComponentMenu("AIS/Narrative/Character Data Validator")]
    public sealed class CharacterDataValidator : MonoBehaviour {
        [Tooltip("Run the scan automatically when the game starts.")]
        public bool RunOnStart = true;

        private void Start() {
            if (RunOnStart) {
                Validate();
            }
        }

        [ContextMenu("Validate Now")]
        public void Validate() {
#if UNITY_EDITOR
            ValidateEditor();
#else
            Log.Warn("[CharacterDataValidator] Skipped: character validation only runs in the editor (it scans the source .leaf/.asset files, which are not shipped in a build).");
#endif // UNITY_EDITOR
        }

#if UNITY_EDITOR
        // Project-relative so AssetDatabase / File.ReadAllText resolve them directly.
        private const string LeafSearchFolder = "Assets/Modules/Narrative/Data";
        private const string CharacterSearchFolder = "Assets/Modules/Narrative/Data/Characters";

        // Captures the character id right after "{@" - everything up to the first whitespace,
        // comma, pipe (pose separator) or closing brace. "{@}" (the narrator) captures empty.
        static private readonly Regex CharacterTagRegex = new Regex(@"\{@([^\s,|}]*)", RegexOptions.Compiled);

        private sealed class MissingCharacter {
            public string DisplayName;
            public readonly List<string> References = new List<string>();
        }

        private void ValidateEditor() {
            // 1. Collect the id of every CharacterData asset. AssetId hashes the asset name the
            // same way the runtime does (CharacterData.Get -> Find.NamedAsset), so matching a leaf
            // tag against this set is identical to how the tag actually resolves in game.
            HashSet<StringHash32> knownCharacters = new HashSet<StringHash32>();
            string[] charGuids = AssetDatabase.FindAssets("t:CharacterData", new[] { CharacterSearchFolder });
            foreach (string guid in charGuids) {
                CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid));
                if (data != null) {
                    knownCharacters.Add(data.AssetId);
                }
            }

            // 2. Scan every leaf script for {@Character} speaker tags and remember any whose id has
            // no CharacterData. Keyed by hash so identical ids (incl. casing) collapse together.
            Dictionary<StringHash32, MissingCharacter> missing = new Dictionary<StringHash32, MissingCharacter>();
            string[] leafGuids = AssetDatabase.FindAssets("t:LeafAsset", new[] { LeafSearchFolder });
            int leafCount = 0;
            foreach (string guid in leafGuids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string[] lines;
                try { lines = File.ReadAllLines(path); }
                catch { continue; }

                leafCount++;
                for (int i = 0; i < lines.Length; i++) {
                    foreach (Match match in CharacterTagRegex.Matches(lines[i])) {
                        string charName = match.Groups[1].Value;
                        if (string.IsNullOrEmpty(charName)) {
                            continue; // {@} - narrator, no named speaker
                        }

                        StringHash32 charId = new StringHash32(charName);
                        if (knownCharacters.Contains(charId)) {
                            continue;
                        }

                        if (!missing.TryGetValue(charId, out MissingCharacter entry)) {
                            entry = new MissingCharacter() { DisplayName = charName };
                            missing.Add(charId, entry);
                        }
                        entry.References.Add(string.Format("{0}:{1}", path, i + 1));
                    }
                }
            }

            // 3. Report - one warning per missing character, listing where it is referenced.
            if (missing.Count == 0) {
                Log.Msg("[CharacterDataValidator] Scanned {0} leaf script(s) against {1} CharacterData asset(s): every referenced character has a CharacterData.", leafCount, knownCharacters.Count);
                return;
            }

            foreach (MissingCharacter entry in missing.Values) {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[CharacterDataValidator] No CharacterData found for character \"{0}\" (referenced {1} time(s)):", entry.DisplayName, entry.References.Count);
                foreach (string reference in entry.References) {
                    sb.Append("\n    ").Append(reference);
                }
                Log.Warn("{0}", sb.ToString());
            }

            Log.Warn("{0}", string.Format("[CharacterDataValidator] {0} referenced character(s) have no CharacterData (scanned {1} leaf script(s), {2} CharacterData asset(s)).", missing.Count, leafCount, knownCharacters.Count));
        }
#endif // UNITY_EDITOR
    }
}
