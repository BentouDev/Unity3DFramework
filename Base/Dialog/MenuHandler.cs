using UnityEngine;

namespace Framework.Base.Dialog
{
    public class MenuHandler : DialogStateHandler<DialogMenu>
    {
        public IGUIDialogMenu GUI;
        public RectTransform  SelectionHandle;

        private int CurrentIndex;
        private int ItemCount => CurrentState.Items.Count;

        public void ClearLines()
        {
            GUI.ClearLines();
        }

        public void NextItem()
        {
            CurrentIndex++;
            if (CurrentIndex >= ItemCount)
                CurrentIndex = 0;
        }

        public void PreviousItem()
        {
            CurrentIndex--;
            if (CurrentIndex < 0)
                CurrentIndex = Mathf.Max(0, ItemCount - 1);
        }

        protected override void OnBegin()
        {
            GUI.Show();

            ClearLines();

            CurrentIndex = 0;

            for (int i = 0; i < ItemCount && i < GUI.GetLineCount(); i++)
            {
                GUI.SetLineText(i, CurrentState.Items[i].Text);
            }
        }
    
        private float PrevY = 0;

        private int HandleAxis(ref float prev, float axis)
        {
            int result = 0;
            if (prev <= 0 && axis > 0)
            {
                result = 1;
            }

            if (prev >= 0 && axis < 0)
            {
                result = - 1;
            }
        
            prev = axis;

            return result;
        }

        public override void Tick()
        {
            var y = Input.GetAxis("UI Vertical");
            var dir = HandleAxis(ref PrevY, y);
        
            if (dir < 0)
                PreviousItem();

            if (dir > 0)
                NextItem();

            if (Input.GetButtonDown("Submit"))
            {
                var nextState = CurrentState.Items[CurrentIndex].State;
                if (nextState)
                    Manager.SwitchState(nextState);
                else
                    Manager.Exit();
            }

            // SelectionHandle.localPosition = GUI.Lines[CurrentIndex].rectTransform.localPosition;
        }

        protected override void OnEnd()
        {
            GUI.Hide();
        }
    }
}
