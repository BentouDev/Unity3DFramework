using System;

namespace Framework
{
    public class AsReorderableList : BaseEditorAttribute
    {
        public bool Editable { get; private set; }
        
        public AsReorderableList(bool editable = true)
        {
            Editable = editable;
        }
    }
}