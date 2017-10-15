namespace Framework.Editor
{
    public class PrefabBrushEditorPresenter : EditorPresenter
    {
        private PrefabBrushEditorView View;
        
        public PrefabBrushEditorPresenter(PrefabBrushEditorView prefabBrushEditorView)
        {
            View = prefabBrushEditorView;
        }

        internal override void OnDraw()
        {
            View.DrawHandle();
        }
    }
}