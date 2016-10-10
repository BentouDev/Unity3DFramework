using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InputBuffer : MonoBehaviour
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

    public class InputData
    {
        public Vector2 Move;
        public Vector2 Look;
        public ButtonInput Dash;
        public ButtonInput Jump;
        public ButtonInput Block;
        public ButtonInput Burst;
        public ButtonInput Slash;
        public ButtonInput Kick;
        public ButtonInput Special;
    }

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

    private ButtonInput[] buffer;
    private ButtonInfo[] buttons;
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
        buttons = new ButtonInfo[buttonCount];
        buffer = new ButtonInput[buttonCount];

        for(int i = 0; i < buttonCount; i++)
        {
            buttons[i].Frames = FramesInBuffer;
        }
    }
    
    public InputData ProcessInput(PlayerController.RawInputData currentRawInput)
    {
        InputData input = new InputData();

        input.Move = currentRawInput.Move;
        input.Look = currentRawInput.Look;

        input.Dash    = HandleButton(currentRawInput.Dash,     0);
        input.Jump    = HandleButton(currentRawInput.Jump,     1);
        input.Block   = HandleButton(currentRawInput.Block,    2);
        input.Burst   = HandleButton(currentRawInput.Burst,    3);
        input.Slash   = HandleButton(currentRawInput.Slash,    4);
        input.Kick    = HandleButton(currentRawInput.Kick,     5);
        input.Special = HandleButton(currentRawInput.Special,  6);

        return input;
    }

    private ButtonInput HandleButton(bool button, int which)
    {
        ButtonInput result;
        bool clean = buffer[which].State == ButtonState.None;
        bool changed = buttons[which].Value != button;
        bool buffered = buttons[which].Frames < FramesInBuffer;

        buttons[which].Value = button;
        result.State = (button && changed) || buffered ? ButtonState.Pressed : ButtonState.None;
        // result.State = (!button && ((changed && clean) || buffered)) ? ButtonState.Pressed : ButtonState.None;

        if (!changed)
        {
            buttons[which].Elapsed += Time.deltaTime;

            if (buttons[which].Elapsed > FrameTime && buffered)
            {
                buttons[which].Frames++;
                buttons[which].Elapsed = 0;
            }
        }
        else
        {
            buttons[which].Elapsed = 0;
            buttons[which].Frames = 0;
        }

        if (changed || buttons[which].Elapsed < RapidTimeTreshold)
        {
            buttons[which].RapidPotential++;
            result.State = buttons[which].RapidPotential > RapidFrameRequirement ? ButtonState.Rapid : result.State;
        }
        else
        {
            buttons[which].RapidPotential = 0;
        }

        result.State = button && buttons[which].Elapsed > HoldTimeRequirement ? ButtonState.Hold : result.State;

        buffer[which] = result;

        return result;
    }

    void OnGUI()
    {
        if (!DrawDebug)
            return;
        
        /*for (int i = 0; i < buttonCount - 1; i++)
        {
            var btn = buffer[i];
            GUI.Label(new Rect(10,10 + i * 20, 500, 30), "Button : " + Enum.GetNames(typeof(InputButtonEnum))[i+1] + ", val " + (bool)btn + ", state " + btn.State + ", pot " + buttons[i].RapidPotential);
        }*/

        GUI.Label(new Rect(10,10,500,30), "Dash " + (bool)buffer[0] + " state " + buffer[0].State + " pot " + buttons[0].RapidPotential);
        GUI.Label(new Rect(10,30,500,30), "Jump " + (bool)buffer[1] + " state " + buffer[1].State + " pot " + buttons[1].RapidPotential);
        GUI.Label(new Rect(10,50,500,30), "Block " + (bool)buffer[2] + " state " + buffer[2].State + " pot " + buttons[2].RapidPotential);
        GUI.Label(new Rect(10,70,500,30), "Burst " + (bool)buffer[3] + " state " + buffer[3].State + " pot " + buttons[3].RapidPotential);
        GUI.Label(new Rect(10,90,500,30), "Slash " + (bool)buffer[4] + " state " + buffer[4].State + " pot " + buttons[4].RapidPotential);
        GUI.Label(new Rect(10,110,500,30), "Kick " + (bool)buffer[5] + " state " + buffer[5].State + " pot " + buttons[5].RapidPotential);
        GUI.Label(new Rect(10,130,500,30), "Special " + (bool)buffer[6] + " state " + buffer[6].State + " pot " + buttons[6].RapidPotential);
    }
}
