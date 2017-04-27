using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InputBuffer<EnumType> : MonoBehaviour where EnumType : struct, IConvertible
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

        public bool IsHold { get { return State == ButtonState.Hold; } }
        public bool IsRapid { get { return State == ButtonState.Rapid; } }
        public bool IsPressed { get { return State == ButtonState.Pressed; } }
        
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

    private Dictionary<EnumType, ButtonInput> buffer = new Dictionary<EnumType, ButtonInput>();
    private Dictionary<EnumType, ButtonInfo> buttons = new Dictionary<EnumType, ButtonInfo>();

    //private ButtonInput[] buffer;
    //private ButtonInfo[] buttons;
    private int buttonCount;

    struct ButtonInfo
    {
        public int RapidPotential;
        public int Frames;
        public float Elapsed;
        public bool Value;
    }

    void Start()
    {
        buttonCount = Enum.GetValues(typeof(InputButtonEnum)).Cast<InputButtonEnum>().Distinct().Count();
        Clear();
    }

    public void Clear()
    {
        buttons.Clear();
        buffer.Clear();
        
        //buttons = new ButtonInfo[buttonCount];
        //buffer = new ButtonInput[buttonCount];

        foreach (EnumType enumValue in Enum.GetValues(typeof(EnumType)))
        {
            buttons[enumValue] = new ButtonInfo() { Frames = FramesInBuffer };
            buffer[enumValue] = new ButtonInput();
        }

        //for(int i = 0; i < buttonCount; i++)
        //{
        //    buttons[i].Frames = FramesInBuffer;
        //}
    }
    
    public ButtonInput HandleButton(bool button, EnumType which)
    {
        ButtonInput result;
        ButtonInfo info = buttons[which];

        bool clean = buffer[which].State == ButtonState.None;
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

        buttons[which] = info;
        buffer[which] = result;

        return result;
    }

    void OnGUI()
    {
        if (!DrawDebug)
            return;

        int i = 10;
        foreach (EnumType enumValue in Enum.GetValues(typeof(EnumType)))
        {
            GUI.Label(new Rect(10, i, 500, 30), string.Format("{0} : {1}, state {2}, pot {3}", enumValue, (bool)buffer[enumValue], buffer[enumValue].State, buttons[enumValue].RapidPotential));
            i += 20;
        }
        
        /*GUI.Label(new Rect(10,10,500,30), "Dash " + (bool)buffer[0] + " state " + buffer[0].State + " pot " + buttons[0].RapidPotential);
        GUI.Label(new Rect(10,30,500,30), "Jump " + (bool)buffer[1] + " state " + buffer[1].State + " pot " + buttons[1].RapidPotential);
        GUI.Label(new Rect(10,50,500,30), "Block " + (bool)buffer[2] + " state " + buffer[2].State + " pot " + buttons[2].RapidPotential);
        GUI.Label(new Rect(10,70,500,30), "Burst " + (bool)buffer[3] + " state " + buffer[3].State + " pot " + buttons[3].RapidPotential);
        GUI.Label(new Rect(10,90,500,30), "Slash " + (bool)buffer[4] + " state " + buffer[4].State + " pot " + buttons[4].RapidPotential);
        GUI.Label(new Rect(10,110,500,30), "Kick " + (bool)buffer[5] + " state " + buffer[5].State + " pot " + buttons[5].RapidPotential);
        GUI.Label(new Rect(10,130,500,30), "Special " + (bool)buffer[6] + " state " + buffer[6].State + " pot " + buttons[6].RapidPotential);*/
    }
}
