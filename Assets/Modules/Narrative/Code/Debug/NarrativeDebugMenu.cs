using System.Collections.Generic;
using System.Text.RegularExpressions;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.Variants;
using FieldDay;
using FieldDay.Debugging;
using FieldDay.Scripting;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using AIS.Model;
#endif // UNITY_EDITOR

namespace AIS.Narrative {
    /// <summary>
    /// Dev-only debug menu (LeftShift+W in play) for jumping around the narrative.
    /// Auto-discovers Chapters / Phases / Threads from the leaf content, so new content
    /// appears with no code changes.
    ///
    /// Relies on a leaf convention (see the phase thread files): every thread's entry node
    /// and its terminal "End" node are marked @exposed. Hub Entry/Continue/Exit are already
    /// @exposed. @exposed is what lets ScriptUtility.SpawnThread launch a node directly.
    /// </summary>
    static public class NarrativeDebugMenu {
        // global:currentPhase (int) / global:currentThread (string, "NONE" when idle) — see PlayerInventory.
        static private readonly TableKeyPair PhaseKey = TableKeyPair.Parse("global:currentPhase");
        static private readonly TableKeyPair ThreadKey = TableKeyPair.Parse("global:currentThread");
        static private readonly StringHash32 NoneThread = "NONE";

        // Discovered navigable content. IsHub items jump to <base>.Entry; thread items jump to
        // their entry node and can be "completed" by running <base>.End.
        private struct NavItem {
            public string Chapter;      // "Chapter 1"
            public int Phase;           // 2
            public bool IsHub;
            public string DisplayName;  // "Outreach" (thread), unused for hubs
            public string BasePath;     // "P2_Outreach"
            public StringHash32 NameHash; // hash of BasePath == the value stored in global:currentThread
            public string EntryNodeId;  // "P2_Outreach.Start" (thread) / "P2_Hub.Entry" (hub)
            public string EndNodeId;    // "P2_Outreach.End" or null (routers have no End)
            public string LeafPath;     // asset path, for the grant scan (editor only) or null
        }

        // Cached thread list so the "Complete Current Thread" button can resolve the active
        // thread (matched by hash) without relying on hash reverse-lookup.
        static private readonly List<NavItem> s_Threads = new List<NavItem>();

        #region Menu factory

        [DebugMenuFactory]
        static private DMInfo NarrativeMenu() {
            DMInfo menu = new DMInfo("Narrative");

            menu.AddText("State", GetStateText);
            menu.AddButton("Complete Current Thread", CompleteCurrentThread, IsThreadActive);
            menu.AddDivider();

            BuildJumpTree(menu);
            return menu;
        }

        static private void BuildJumpTree(DMInfo root) {
            List<NavItem> items = new List<NavItem>();
            Discover(items);

            s_Threads.Clear();
            foreach (NavItem it in items) {
                if (!it.IsHub) {
                    s_Threads.Add(it);
                }
            }

            // Chapters, sorted (by trailing number when present).
            List<string> chapters = new List<string>();
            foreach (NavItem it in items) {
                if (!chapters.Contains(it.Chapter)) {
                    chapters.Add(it.Chapter);
                }
            }
            chapters.Sort(CompareChapters);

            foreach (string chapter in chapters) {
                DMInfo chapterMenu = DMInfo.FindOrCreateSubmenu(root, chapter);

                // Phases in this chapter, sorted numerically.
                List<int> phases = new List<int>();
                foreach (NavItem it in items) {
                    if (it.Chapter == chapter && !phases.Contains(it.Phase)) {
                        phases.Add(it.Phase);
                    }
                }
                phases.Sort();

                foreach (int phase in phases) {
                    DMInfo phaseMenu = DMInfo.FindOrCreateSubmenu(chapterMenu, "Phase " + phase);

                    // Hub button first.
                    foreach (NavItem it in items) {
                        if (it.Chapter == chapter && it.Phase == phase && it.IsHub) {
                            string hubEntryId = it.EntryNodeId;
                            phaseMenu.AddButton("Jump to Hub", () => JumpToHub(hubEntryId), CanJump);
                        }
                    }

                    // Thread buttons, sorted by display name.
                    List<NavItem> phaseThreads = new List<NavItem>();
                    foreach (NavItem it in items) {
                        if (it.Chapter == chapter && it.Phase == phase && !it.IsHub) {
                            phaseThreads.Add(it);
                        }
                    }
                    phaseThreads.Sort((a, b) => string.CompareOrdinal(a.DisplayName, b.DisplayName));

                    foreach (NavItem thread in phaseThreads) {
                        NavItem t = thread; // capture a copy for the closure
                        phaseMenu.AddButton("Jump: " + t.DisplayName, () => JumpToThread(t), CanJump);
                    }
                }
            }
        }

        #endregion // Menu factory

        #region Actions

        // Jump to a phase hub fresh, clearing any in-progress thread. The hub Entry node sets
        // global:currentPhase itself.
        static private void JumpToHub(string hubEntryId) {
            if (!Game.SharedState.TryGet(out PlayerInventory _)) { return; }
            ScriptUtility.KillAllThreads();
            ScriptUtility.WriteVariable(ThreadKey, NoneThread);
            ScriptUtility.SpawnThread(hubEntryId);
        }

        // Set up the hub state, then start a thread fresh. The thread's entry node sets
        // global:currentThread to its own id.
        static private void JumpToThread(NavItem thread) {
            if (!Game.SharedState.TryGet(out PlayerInventory _)) { return; }
            ScriptUtility.KillAllThreads();
            ScriptUtility.WriteVariable(PhaseKey, thread.Phase);
            ScriptUtility.WriteVariable(ThreadKey, NoneThread);
            ScriptUtility.SpawnThread(thread.EntryNodeId);
        }

        // Grant everything the current thread would give (evidence chips / action cards, in ANY
        // node), then run its End node to reset state and route back to the hub.
        static private void CompleteCurrentThread() {
            if (!Game.SharedState.TryGet(out PlayerInventory _)) { return; }

            StringHash32 current = ScriptUtility.ReadVariable(ThreadKey).AsStringHash();
            if (current.IsEmpty || current == NoneThread) { return; }

            bool found = false;
            NavItem match = default;
            foreach (NavItem t in s_Threads) {
                if (t.NameHash == current) {
                    match = t;
                    found = true;
                    break;
                }
            }

#if UNITY_EDITOR
            if (found && !string.IsNullOrEmpty(match.LeafPath)) {
                GrantThreadItems(match.LeafPath);
            }
#endif // UNITY_EDITOR

            // Prefer the discovered End id; fall back to reverse-lookup (works in DEVELOPMENT).
            string endId = (found && match.EndNodeId != null) ? match.EndNodeId : current.ToString() + ".End";
            ScriptUtility.SpawnThread(endId);
        }

        #endregion // Actions

        #region Predicates / readout

        static private bool CanJump() {
            return Game.SharedState.TryGet(out PlayerInventory _);
        }

        static private bool IsThreadActive() {
            if (!Game.SharedState.TryGet(out PlayerInventory _)) { return false; }
            StringHash32 current = ScriptUtility.ReadVariable(ThreadKey).AsStringHash();
            return !current.IsEmpty && current != NoneThread;
        }

        static private string GetStateText() {
            if (!Game.SharedState.TryGet(out PlayerInventory _)) { return "Phase: -   Thread: - (not in narrative scene)"; }
            int phase = ScriptUtility.ReadVariable(PhaseKey).AsInt();
            StringHash32 current = ScriptUtility.ReadVariable(ThreadKey).AsStringHash();
            string threadStr = current.IsEmpty ? "NONE" : current.ToString();
            return string.Format("Phase: {0}   Thread: {1}", phase, threadStr);
        }

        #endregion // Predicates / readout

        #region Discovery

        static private void Discover(List<NavItem> items) {
#if UNITY_EDITOR
            DiscoverFromAssets(items);
#else
            DiscoverFromLoaders(items);
#endif // UNITY_EDITOR
        }

        static private int CompareChapters(string a, string b) {
            int na, nb;
            if (TryTrailingInt(a, out na) && TryTrailingInt(b, out nb)) {
                return na.CompareTo(nb);
            }
            return string.CompareOrdinal(a, b);
        }

        static private bool TryTrailingInt(string s, out int value) {
            Match m = Regex.Match(s, @"(\d+)\s*$");
            if (m.Success) { return int.TryParse(m.Groups[1].Value, out value); }
            value = 0;
            return false;
        }

        // "P2_Outreach" -> "Outreach"; "P0_Intro" -> "Intro"
        static private string StripPhasePrefix(string basePath) {
            return Regex.Replace(basePath, @"^P\d+_", "");
        }

#if UNITY_EDITOR
        // Editor discovery: scan the leaf assets under Narrative/Data, group by folder
        // (Chapter / Phase), and read each file to find its entry node + End node. This is the
        // only place the Chapter is visible (it lives in the folder path, not the file name).
        static private void DiscoverFromAssets(List<NavItem> items) {
            string[] guids = AssetDatabase.FindAssets("t:LeafAsset", new[] { "Assets/Modules/Narrative/Data" });
            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Match m = Regex.Match(path, @"/(Chapter [^/]+)/Phase (\d+)/([^/]+)\.leaf$", RegexOptions.IgnoreCase);
                if (!m.Success) { continue; } // skip root location scripts / demos not in a Phase folder

                string chapter = m.Groups[1].Value;
                int phase = int.Parse(m.Groups[2].Value);
                string fileName = m.Groups[3].Value;

                string text;
                try { text = File.ReadAllText(path); }
                catch { continue; }

                string basePath = ParseBasePath(text) ?? fileName;

                if (fileName.EndsWith("_Hub")) {
                    items.Add(new NavItem {
                        Chapter = chapter, Phase = phase, IsHub = true,
                        BasePath = basePath, EntryNodeId = basePath + ".Entry",
                    });
                    continue;
                }

                string entryNode = FirstExposedNode(text);
                if (entryNode == null) { continue; } // no @exposed entry -> not jumpable

                items.Add(new NavItem {
                    Chapter = chapter, Phase = phase, IsHub = false,
                    DisplayName = StripPhasePrefix(basePath), BasePath = basePath,
                    NameHash = basePath,
                    EntryNodeId = basePath + "." + entryNode,
                    EndNodeId = HasEndNode(text) ? basePath + ".End" : null,
                    LeafPath = path,
                });
            }
        }

        static private string ParseBasePath(string text) {
            Match m = Regex.Match(text, @"(?m)^\s*#basePath\s+(\S+)");
            return m.Success ? m.Groups[1].Value : null;
        }

        // Returns the name of the first "::" node whose header carries an @exposed tag.
        static private string FirstExposedNode(string text) {
            MatchCollection headers = Regex.Matches(text, @"(?m)^::[ \t]*(\w+)");
            for (int i = 0; i < headers.Count; i++) {
                int start = headers[i].Index;
                int end = (i + 1 < headers.Count) ? headers[i + 1].Index : text.Length;
                string block = text.Substring(start, end - start);
                if (Regex.IsMatch(block, @"(?m)^[ \t]*@exposed\b")) {
                    return headers[i].Groups[1].Value;
                }
            }
            return null;
        }

        static private bool HasEndNode(string text) {
            return Regex.IsMatch(text, @"(?m)^::[ \t]*End\b");
        }

        // Scan the thread's leaf source for GiveEvidenceChip / GiveActionCard calls anywhere in
        // the file and grant them via the same data-path as ScriptHooks (adds are idempotent).
        // Skips commented-out lines and bracket placeholders like [Evidence] / [Action].
        static private void GrantThreadItems(string leafPath) {
            string text;
            try { text = File.ReadAllText(leafPath); }
            catch { return; }

            PlayerInventory inv = Find.State<PlayerInventory>();
            if (inv == null) { return; }

            foreach (string rawLine in text.Split('\n')) {
                int comment = rawLine.IndexOf("//");
                string code = comment >= 0 ? rawLine.Substring(0, comment) : rawLine;

                foreach (Match m in Regex.Matches(code, @"GiveEvidenceChip\s*\(\s*""?([^""\)]+)""?\s*\)")) {
                    string id = m.Groups[1].Value.Trim();
                    if (id.Length == 0 || id[0] == '[') { continue; }
                    StringHash32 hash = id;
                    if (inv.EvidenceChips.Add(hash)) {
                        InvasionModel.Instance?.SimDetailRegistry?.TryEnqueueReveal(hash);
                    }
                }

                foreach (Match m in Regex.Matches(code, @"GiveActionCard\s*\(\s*""?([^""\)]+)""?\s*\)")) {
                    string id = m.Groups[1].Value.Trim();
                    if (id.Length == 0 || id[0] == '[') { continue; }
                    StringHash32 hash = id;
                    inv.ActionCards.Add(hash);
                }
            }
        }
#else
        // Non-editor dev-build fallback: enumerate loaded leaf assets by name. Folder info
        // (Chapter) is not available, so everything is grouped under a single default chapter,
        // and the entry node is assumed to be "Start" by convention.
        static private void DiscoverFromLoaders(List<NavItem> items) {
            ScriptLoader[] loaders = UnityEngine.Object.FindObjectsOfType<ScriptLoader>();
            HashSet<string> seen = new HashSet<string>();
            foreach (ScriptLoader loader in loaders) {
                if (loader.Scripts == null) { continue; }
                foreach (var asset in loader.Scripts) {
                    if (asset == null) { continue; }
                    string name = asset.name;
                    if (!seen.Add(name)) { continue; }

                    Match m = Regex.Match(name, @"^P(\d+)_");
                    if (!m.Success) { continue; }
                    int phase = int.Parse(m.Groups[1].Value);

                    if (name.EndsWith("_Hub")) {
                        items.Add(new NavItem {
                            Chapter = "Chapter 1", Phase = phase, IsHub = true,
                            BasePath = name, EntryNodeId = name + ".Entry",
                        });
                    } else {
                        items.Add(new NavItem {
                            Chapter = "Chapter 1", Phase = phase, IsHub = false,
                            DisplayName = StripPhasePrefix(name), BasePath = name,
                            NameHash = name,
                            EntryNodeId = name + ".Start",
                            EndNodeId = name + ".End",
                            LeafPath = null,
                        });
                    }
                }
            }
        }
#endif // UNITY_EDITOR

        #endregion // Discovery
    }
}
