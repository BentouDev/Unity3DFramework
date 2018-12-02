using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Framework;

public class InputBuffer<TKeyType> : BaseBehaviour
{
    public bool DrawDebug;

    [Range(1, 32)]
    public int FramesInBuffer = 4;

    [Range(1.0f / 60.0f, 0.5f)]
    public float FrameTime = 0.01666667f;

    [Range(1.0f / 60.0f, 1f)]
    public float RapidTimeTreshold = 0.02f;

    [Range(1, 32)]
    public int RapidFrameRequirement = 25;

    [Range(1.0f / 60.0f, 1f)]
    public float HoldTimeRequirement = 0.05f;

    public struct ButtonInput
    {
        public ButtonState State;

        public bool IsHold => State == ButtonState.Hold;
        public bool IsRapid => State == ButtonState.Rapid;
        public bool IsPressed => State == ButtonState.Pressed;

        public ButtonInput(bool val) : this()
        {
            State = ButtonState.Pressed;
        }

        public static implicit operator ButtonInput(bool value)
        {
            return new ButtonInput(value);
        }

        public static implicit operator ButtonState(ButtonInput instance)
        {
            return instance.State;
        }

        public static implicit operator bool(ButtonInput instance)
        {
            return instance.State == ButtonState.Pressed;
        }
    }

    private Dictionary<TKeyType, ButtonInput> Buffer = new Dictionary<TKeyType, ButtonInput>();
    private Dictionary<TKeyType, ButtonInfo> Buttons = new Dictionary<TKeyType, ButtonInfo>();
    private IList<TKeyType> Definitions;

    private int buttonCount;

    struct ButtonInfo
    {
        public int RapidPotential;
        public int Frames;
        public float Elapsed;
        public bool Value;
    }

    public void DefineButtons(IList<TKeyType> buttons)
    {
        Definitions = buttons;

        Clear();
    }

    public void Clear()
    {
        Buttons.Clear();
        Buffer.Clear();
        
        foreach (TKeyType key in Definitions)
        {
            Buttons[key] = new ButtonInfo() { Frames = FramesInBuffer};
            Buffer[key] = new ButtonInput();
        }
    }
    
    public ButtonInput HandleButton(bool button, TKeyType which)
    {
        ButtonInput result;
        ButtonInfo info = Buttons[which];

        bool clean = Buffer[which].State == ButtonState.None;
        bool changed = info.Value != button;
        bool buffered = info.Frames < FramesInBuffer;

        info.Value = button;
        // This one was created before, has a bug when on release it sends pressed once again
        // But probably also creates another one, discovered in engine - multiple presses and releases lock state of button
        // Must check if it is not created by press and hold avoidance somehow
        // result.State = (button && changed) || buffered ? ButtonState.Pressed : ButtonState.None;

        // This one was created after, to prevent from above bug
        result.State = (!button && ((changed && clean) || buffered)) ? ButtonState.Pressed : ButtonState.None;

        if (!changed)
        {
            info.Elapsed += Time.deltaTime;

            if (info.Elapsed > FrameTime && buffered)
            {
                info.Frames++;
                info.Elapsed = 0;
            }
        }
        else
        {
            info.Elapsed = 0;
            info.Frames = 0;
        }

        if (changed || info.Elapsed < RapidTimeTreshold)
        {
            info.RapidPotential++;
            result.State = info.RapidPotential > RapidFrameRequirement ? ButtonState.Rapid : result.State;
        }
        else
        {
            info.RapidPotential = 0;
        }

        result.State = button && info.Elapsed > HoldTimeRequirement ? ButtonState.Hold : result.State;

        Buttons[which] = info;
        Buffer[which] = result;

        return result;
    }

    void OnGUI()
    {
        if (!DrawDebug)
            return;

        int i = 10;
        foreach (TKeyType key in Definitions)
        {
            GUI.Label(new Rect(10, i, 500, 30),
                $"{key} : {(bool) Buffer[key]}, state {Buffer[key].State}, pot {Buttons[key].RapidPotential}");
            i += 20;
        }
    }
}
