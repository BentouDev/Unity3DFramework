using UnityEngine;
using System.Collections;

[System.Serializable]
public class ActionInput
{
    public InputButtonEnum Buttons;
    public ButtonState State;
    public bool IsMoving;
    public bool IsInAir;
}

public enum ButtonState
{
    None,
    Pressed,
    Hold,
    Rapid
}
