using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnionAvatars.UI
{
    public class EventToggleGroup : MonoBehaviour
    {
        [System.Serializable]
        public class ToggleEvent : UnityEvent<int> { }

        [SerializeField]
        public ToggleEvent onActiveToggleChanged;

        private Toggle[] _toggles;

        private void Start()
        {
            RefreshToggles();
        }

        public void RefreshToggles(int defaultIndex = -1)
        {
            CleanToggles();

            _toggles = GetComponentsInChildren<Toggle>(true);

            if (defaultIndex >= 0 && defaultIndex < _toggles.Length)
                _toggles[defaultIndex].isOn = true;

            SetupEvents();
        }

        private void OnEnable()
        {
            if(_toggles == null)
                return;
                
            // Reset toggles
            for (int i = 0; i < _toggles.Length; i++)
            {
                _toggles[i].isOn = i == 0;
                _toggles[i].interactable = i != 0;
            }
        }

        // Start is called before the first frame update
        void SetupEvents()
        {
            for (int i = 0; i < _toggles.Length; i++)
            {
                int indexValue = i;
                _toggles[i].onValueChanged.AddListener(
                    (isOn) => HandleToggleValueChanged(isOn, indexValue)
                );
            }
        }

        void HandleToggleValueChanged(bool isOn, int index)
        {
            if (!isOn)
                return;

            for (int i = 0; i < _toggles.Length; i++)
            {
                _toggles[i].isOn = i == index;
                _toggles[i].interactable = i != index;
            }

            onActiveToggleChanged?.Invoke(index);
        }

        void CleanToggles()
        {
            if (_toggles == null)
                return;

            foreach (Toggle toggle in _toggles)
            {
                toggle.onValueChanged.RemoveAllListeners();
            }
        }
    }
}
