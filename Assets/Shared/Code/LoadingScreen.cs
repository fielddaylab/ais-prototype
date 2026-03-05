using BeauUtil;
using FieldDay;
using FieldDay.Scenes;
using FieldDay.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AIS {
    public sealed class LoadingScreen : BaseGuiModule, IRegistrationCallbacks {
        public Canvas Canvas;

        void IRegistrationCallbacks.OnRegister() {
            Game.Scenes.RegisterTransitionHandlers(UnloadTransition, LoadTransition);
            Canvas.enabled = true;

            if (!GameLoop.IsBooted()) {
                gameObject.SetActive(true);
            } else {
                gameObject.SetActive(false);
            }
        }

        void IRegistrationCallbacks.OnDeregister() {
            Game.Scenes.RegisterTransitionHandlers(null, null);
        }

        private IEnumerator UnloadTransition(Scene scene, StringHash32 tag, MainSceneTransitionArgs transitionArgs) {
            gameObject.SetActive(true);
            return null;
        }

        private IEnumerator LoadTransition(Scene scene, StringHash32 tag, MainSceneTransitionArgs transitionArgs) {
            gameObject.SetActive(false);
            return null;
        }
    }
}