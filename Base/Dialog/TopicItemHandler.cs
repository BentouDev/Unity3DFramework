using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.Base.Dialog
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class ItemHandler
    {
        delegate ItemHandler HandlerCreator(DialogTopic.Item item);

        private static Dictionary<System.Type, HandlerCreator> HandlerFactory = new Dictionary<System.Type, HandlerCreator>();

        protected DialogGameState Manager;
        protected TopicHandler Topic;

        public virtual bool IsTopicEnd => false;
    
        internal void Begin(TopicHandler handler)
        {
            Topic   = handler;
            Manager = handler.Manager;

            OnBegin();
        }

        internal virtual void OnBegin()
        { }

        internal virtual void End()
        { }

        internal virtual bool Execute()
        { return false; }
    
        static ItemHandler()
        {
            HandlerFactory.Clear();
            RegisterHandler<ExitItem>((item) => new ExitHandler());
            RegisterHandler<SayItem>((item) => new SayHandler() { Item = (SayItem)item });
            RegisterHandler<GotoItem>((item) => new GotoHandler() { Item = (GotoItem)item });
            RegisterHandler<InvokeItem>((item) => new InvokeHandler() { Item = (InvokeItem)item });
        }

        private static void RegisterHandler<TItem>(HandlerCreator creator)
        {
            HandlerFactory[typeof(TItem)] = creator;
        }

        public static ItemHandler PickHandler(DialogTopic.Item item)
        {
            return HandlerFactory[item.GetType()](item);
        }
    }

    abstract class ItemHandler<TItem> : ItemHandler
    {
        internal TItem Item;
    }

    class SayHandler : ItemHandler<SayItem>
    {
        private bool IsAnimating;
    
        internal override void OnBegin()
        {
            Topic.GUI.SetName(Topic.Dialog.GetActorName(Item.Actor));
            Topic.GUI.SetText(Item.Text);
        }

        internal override bool Execute()
        {
            if (Input.GetButtonDown("Submit"))
            {
                return true;
            }

            return false;
        }
    }

    class InvokeHandler : ItemHandler<InvokeItem>
    {
        internal override bool Execute()
        {
            Manager.CurrentDialog.InvokeFunction(Item.Function);
            return true;
        }
    }

    class GotoHandler : ItemHandler<GotoItem>
    {
        public override bool IsTopicEnd => true;

        internal override bool Execute()
        {
            Manager.SwitchState(Item.State);
            return true;
        }
    }

    class ExitHandler : ItemHandler
    {
        public override bool IsTopicEnd => true;

        internal override bool Execute()
        {
            Manager.Exit();
            return true;
        }
    }
}