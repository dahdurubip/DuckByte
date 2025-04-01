using System.Threading;
using UnityEngine;

namespace UnionAvatars.VRUI
{    
    public class UIModule : MonoBehaviour
    {
        protected UIModule root;
        protected UIModule parent;
        protected AvatarUIManager uiManager;
        protected UIModule child;
        [Header("Modules")]
        /// <summary>
        /// Parent Transform where child modules will be spawned
        /// </summary>
        public Transform ModuleContainer;
        /// <summary>
        /// Text to be displayed in the base module
        /// </summary>
        public string StateText;
        /// <summary>
        /// If true, when the back button is pressed it will travel to the previous module
        /// </summary>
        public bool CanGoBack;
        /// <summary>
        /// The module to go once the back button is pressed
        /// </summary>
        public UIModule previousModule;
        protected CancellationTokenSource cancellationToken = new CancellationTokenSource();

        public void EnterModule(UIModule parent, UIModule root, AvatarUIManager uiManager)
        {
            this.parent = parent;
            this.root = root ?? this;
            this.uiManager = uiManager;

            (this.root as BaseModule).SetStateText(StateText);

            (this.root as BaseModule).ToggleBackButton(CanGoBack);

            if(CanGoBack)
            {
                (this.root as BaseModule).OnBack = GoBack;
            }
        }

        public UIModule SwapChild(UIModule module)
        {
            //Delete any previous module
            CloseRecursive(child);

            UIModule newChildModule = Instantiate(module, ModuleContainer);

            newChildModule.EnterModule(this, root, uiManager);

            child = newChildModule;

            return newChildModule;
        }

        public UIModule SwapModule(UIModule module)
        {
            if(parent == null)
                return this;

            return parent.SwapChild(module);
        }

        public void SwapModuleNoReturn(UIModule module)
        {
            if(parent == null)
                return;

            parent.SwapChild(module);
        }

        public void SwapRoot(UIModule module)
        {
            if(parent == null)
                return;

            parent.SwapChild(module);
        }

        public void CloseRecursive(UIModule module)
        {
            if(module == null) return;

            CloseRecursive(module.child);

            module.OnExitModule();
            Destroy(module.gameObject);
        }

        protected virtual void OnExitModule()
        {
            if(CanGoBack && parent == root)
            {
                (this.root as BaseModule).OnBack -= GoBack;
            }
        }

        protected virtual void GoBack()
        {
            if(previousModule != null)
            {
                SwapModule(previousModule);
            } 
            else
            {
                // Recurse hierarchy for parent modules
                UIModule currentModule = parent;
                while(parent.previousModule == null)
                {
                    if(currentModule.parent == null)
                        throw new System.InvalidOperationException("Couldn't find a previous module in hierarchy");
                    
                    currentModule = currentModule.parent;
                }

                currentModule.SwapModule(currentModule.previousModule);
            }
        }

        private void OnDestroy()
        {
            cancellationToken.Cancel();
        }
    }
}
