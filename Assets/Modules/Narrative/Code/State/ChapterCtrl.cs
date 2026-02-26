using System.Collections.Generic;
using AIS.Model;
using BeauUtil;
using FieldDay.Scenes;
using FieldDay.Scripting;

namespace AIS.Narrative {
    public sealed class ChapterCtrl : SceneController {
        protected override IEnumerator<WorkSlicer.Result?> OnScenePreload() {
            InvasionModel.Instance.Load(0);
            InvasionModel.Instance.gameObject.SetActive(false);
            return null;
        }

        protected override void OnSceneReady() {
            ScriptUtility.Trigger("ChapterStart");
        }
    }
}