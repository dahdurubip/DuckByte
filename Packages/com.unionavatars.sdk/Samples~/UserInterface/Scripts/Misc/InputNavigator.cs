using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
 
namespace UnionAvatars.UI
{
    public class InputNavigator : MonoBehaviour
    {
        EventSystem system;
        [SerializeField] private Selectable firstElementSelection;
    
        void Start()
        {
            system = EventSystem.current;// EventSystemManager.currentSystem;
            firstElementSelection.Select();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if(Input.GetKey(KeyCode.LeftShift))
                    SelectPreviousElement();
                else
                    SelectNextElement();
            }
        }

        public void SelectNextElement(string text)
        {
            SelectNextElement();
        }

        public void SelectPreviousElement()
        {
            if(system.currentSelectedGameObject == null) return;
            
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
            
            if (next != null)
            {
                TMP_InputField inputfield = next.GetComponent<TMP_InputField>();
                if (inputfield != null)
                {
                    inputfield.OnPointerClick(new PointerEventData(system));
                    system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
                }
            }
        }

        public void SelectNextElement()
        {
            if(system.currentSelectedGameObject == null) return;
            
            Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            
            if (next != null)
            {
                TMP_InputField inputfield = next.GetComponent<TMP_InputField>();
                if (inputfield != null)
                {
                    inputfield.OnPointerClick(new PointerEventData(system));
                    system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
                }
            }
        }
    }
}