using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public interface ICinematicCamera
    {
        void StartAnimating(float speed, float duration, float rate);

        bool IsAnimating();
    }
}
