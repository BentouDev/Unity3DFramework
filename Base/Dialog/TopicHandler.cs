using System.Linq;
using UnityEngine;

namespace Framework.Base.Dialog
{
    public class TopicHandler : DialogStateHandler<DialogTopic>
    {
        public  IGUIDialogTopic GUI;
    
        private ItemHandler CurrentHandler;
        private int         CurrentIndex;
    
        private DialogTopic.Item Next()
        {
            CurrentIndex++;
            return (CurrentState.Items != null && CurrentIndex < CurrentState.Items.Count) ? CurrentState.Items[CurrentIndex] : null;
        }

        protected void SwitchItem(DialogTopic.Item item)
        {
            CurrentHandler?.End();

            if (item == null)
            {
                Debug.LogError("Unable to handle dialog topic item NULL", CurrentState);
                Manager.Exit();
            }
            else
            {
                CurrentHandler = ItemHandler.PickHandler(item);

                if (CurrentHandler != null)
                    CurrentHandler.Begin(this);
                else
                {
                    Debug.LogError(string.Format("Unable to handle dialog topic item {0}", item), CurrentState);
                    Manager.Exit();
                }
            }
        }

        protected override void OnBegin()
        {
            GUI.Show();
            CurrentIndex = 0;
            SwitchItem(CurrentState.Items.FirstOrDefault());
        }

        protected override void OnTick()
        {
            if (CurrentHandler.Execute() && !CurrentHandler.IsTopicEnd)
                SwitchItem(Next());
        }

        protected override void OnEnd()
        {
            GUI.Hide();
        }
    }
}
