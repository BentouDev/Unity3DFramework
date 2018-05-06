namespace Framework.Base.Dialog
{
    public abstract class IGUIDialogMenu : GUIBase
    {
        public abstract void SetLineText(int i, string text);
        public abstract void ClearLines();
        public abstract int  GetLineCount();
    }
}