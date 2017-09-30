using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Editor
{
    public abstract class EditorPresenter
    {
        internal virtual void OnEnable()
        { }

        internal virtual void OnFocus()
        { }

        internal virtual void OnProjectChange()
        { }

        internal virtual void OnSelectionChange()
        { }

        internal abstract void OnDraw();
    }
}
