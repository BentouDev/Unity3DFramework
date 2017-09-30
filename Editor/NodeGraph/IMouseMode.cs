using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Editor
{
    public interface IMouseMode
    {
        void Start(Vector2 pos);
        void End(Vector2 pos);
        void Update(Vector2 pos);
    }
}
