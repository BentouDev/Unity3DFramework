using Framework.Base.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework
{
    public abstract class IGUIActivateable : GUIBase, ISubmitHandler
    {
        public abstract void OnActivatorSelected(ActionActivator activator);

        private Activateable Actor;
        
        public void Init(Activateable activateable)
        {
            Actor = activateable;
        }
        
        public void OnSubmit(BaseEventData eventData)
        {
            Actor?.OnSubmit();
        }
    }
}