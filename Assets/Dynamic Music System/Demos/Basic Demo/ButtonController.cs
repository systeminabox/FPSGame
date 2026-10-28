using UnityEngine;
using UnityEngine.UI;

namespace MordiAudio.MusicSystem.Demo
{
    public class ButtonController : MonoBehaviour
    {
        public DynamicMusicSystem musicSystem;
        public Button playButton;
        public Button[] sectionButtons;

        private void Awake() {
            playButton.onClick.AddListener(OnPlayButtonClick);

            for (int i = 0; i < sectionButtons.Length; i++) {
                sectionButtons[i].onClick.AddListener(OnSectionButtonClick);
            }
        }

        void OnPlayButtonClick() {
            musicSystem.StartPlaybackAllTracks();
        }

        void OnSectionButtonClick() {
            GameObject go = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

            if (go == null)
                return;

            Text text = go.GetComponentInChildren<Text>();

            if (text == null)
                return;

            musicSystem.GoToSectionAllTracks(text.text);
        }
    }
}