using System.Collections.Generic;
using AIS.Model;
using BeauUtil;
using FieldDay;
using FieldDay.Scenes;
using FieldDay.Scripting;

namespace AIS.Narrative {
    public sealed class ChapterCtrl : SceneController, ISceneLoadDependency {
        private int m_StartingThreadCount;

        protected override IEnumerator<WorkSlicer.Result?> OnScenePreload() {
            InvasionModel.Instance.Load(0, SimDetailPhase.Narrative);
            InvasionModel.Instance.gameObject.SetActive(false);
            m_StartingThreadCount = ScriptUtility.CurrentThreadCount;
            return null;
        }

        protected override void OnSceneEnable() {
            ScriptUtility.Invoke("ChapterPreload");
        }

        protected override void OnSceneReady() {
            ScriptUtility.Trigger("ChapterStart");
        }

        bool ISceneLoadDependency.IsLoaded(SceneLoadPhase loadPhase) {
            return ScriptUtility.CurrentThreadCount == m_StartingThreadCount;
        }
    }
}