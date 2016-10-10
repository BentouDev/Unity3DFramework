using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugConsole : MonoBehaviour
{
    public bool Show;
    public bool PasueOnError;
    public bool InterceptLogOutput;

    public string Prompt = " > ";

    [Header("Display")]
    public GUISkin Skin;

    public int Margin = 20;
    public int InputHeight = 25;

    private int HistoryIndex = 0;

    public string UserInput { get; private set; }
    public string CommandInput { get; private set; }
    
    public Rect DefaultWindowRect {  get { return new Rect
    (
        Margin, Margin, 
        Screen.width - (Margin*2), 
        Screen.height - (Margin*2));
    } }

    public Rect DefaultInputRect { get { return new Rect
    (
            DefaultWindowRect.xMin - Margin + 10,
            DefaultWindowRect.yMax - Margin - InputHeight,
            DefaultWindowRect.width - 10,
            InputHeight
    ); } }
    
    public Color CommandColor = new Color(0.5f, 0.5f, 1.0f);
    public Color ErrorColor = new Color(1.0f, 0.5f, 0.5f);
    public Color WarningColor = new Color(1.0f, 1.0f, 0.5f);
    public Color NormalColor = new Color(1.0f, 1.0f, 1.0f);
    public Color LogColor = new Color(0.75f, 0.75f, 0.75f);

    private ConsoleController Controller;

    private static readonly string CommandLineControlName = "commandLine";

    public DebugConsole()
    {
        Controller = new ConsoleController();
    }
    
    private class LogInterceptor : ILogHandler
    {
        private readonly ConsoleController Console;
        private readonly ILogHandler FallbackHandler;
        private Color LogColor;

        public LogInterceptor(ConsoleController console, ILogHandler fallback, Color logColor)
        {
            Console = console;
            LogColor = logColor;
            FallbackHandler = fallback;
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            string timePrefix = string.Format("[{0}:{1}.{2}] : ", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            switch (logType)
            {
                case LogType.Log:
                    Console.Print(timePrefix + string.Format(format, args), LogColor);
                    break;
                case LogType.Warning:
                    Console.Warning(timePrefix + string.Format(format, args));
                    break;
                case LogType.Assert:
                    Console.Error(timePrefix + "Failed Assertion!\n" + string.Format(format, args));
                    break;
                case LogType.Error:
                    Console.Error(timePrefix + string.Format(format, args));
                    break;
                case LogType.Exception:
                    Console.Error(timePrefix + "Unhandled Exception!\n" + string.Format(format, args));
                    break;
            }
            FallbackHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            Console.Error("Unhandled exception!\n" + exception);
            FallbackHandler.LogException(exception, context);
        }
    }

    void Awake()
    {
        Controller.CommandColor = CommandColor;
        Controller.NormalColor = NormalColor;
        Controller.WarningColor = WarningColor;
        Controller.ErrorColor = ErrorColor;

        CommandInput = string.Empty;
        UserInput = string.Empty;

        if (InterceptLogOutput)
        {
            Debug.logger.logHandler = new LogInterceptor(Controller, Debug.logger.logHandler, LogColor);
        }
    }

    public void ToggleConsole()
    {
        Show = !Show;
    }

    public void HistoryUp()
    {
        HistoryIndex--;
        if (HistoryIndex + Controller.GetHistoryCount() < 0)
            HistoryIndex = -Controller.GetHistoryCount();
    }

    public void HistoryDown()
    {
        HistoryIndex++;
        if (HistoryIndex > 0)
            HistoryIndex = 0;
    }

    public void RegisterCommand(string name, string help, 
        ConsoleController.CommandHandler handler)
    {
        Controller.RegisterCommand(name, help, handler);
    }

    public void RegisterVariable(string name, string help, 
        ConsoleController.VariableInfo.VariableParser parser, 
        ConsoleController.VariableInfo.VariablePrinter printer)
    {
        Controller.RegisterVariable(name, help, parser, printer);
    }

    public void RegisterVariableFloat(string name, string help, 
        ConsoleController.VariableInfo.SetFloat setter, 
        ConsoleController.VariableInfo.GetFloat getter)
    {
        RegisterVariable
        (
            name, help, str =>
            {
                float val;
                if (!float.TryParse(str, out val)) return false;
                setter(val);
                return true;
            },
            () => getter().ToString(CultureInfo.InvariantCulture)
        );
    }

    public void Print(string message)
    {
        Controller.Print(message);
    }

    public void Print(string message, Color color)
    {
        Controller.Print(message, color);
    }

    public void Error(string message)
    {
        Controller.Error(message);
    }

    public void Warning(string message)
    {
        Controller.Warning(message);
    }

    void OnGUI()
    {
        var current = Event.current;
        if (current.isKey && current.type == EventType.KeyDown && current.keyCode == KeyCode.BackQuote)
        {
            current.Use();
            ToggleConsole();
        }

        if (!Show)
            return;

        if (Skin)
            GUI.skin = Skin;

        GUI.Window(12345, DefaultWindowRect, HandleConsoleWindow, "Console");
    }

    bool HandleConsoleEvents()
    {
        bool result = false;
        Event current = Event.current;

        if (!current.isKey || current.type != EventType.KeyDown)
            return false;

        switch (current.keyCode)
        {
            case KeyCode.Return:
                result = true;
                current.Use();

                Controller.CommitCommand(CommandInput);

                CommandInput = string.Empty;
                UserInput = string.Empty;
                HistoryIndex = 0;
                break;
            case KeyCode.BackQuote:
                result = true;
                current.Use();

                ToggleConsole();
                break;
            case KeyCode.UpArrow:
                result = true;
                current.Use();

                HistoryUp();
                break;
            case KeyCode.DownArrow:
                result = true;
                current.Use();

                HistoryDown();
                break;
        }

        return result;
    }

    void HandleConsoleOutput()
    {
        var oldColor = GUI.color;
        var messageRect = DefaultInputRect;

        for (int i = Controller.GetOutputCount() - 1; i >= 0; i--)
        {
            var outputMessage = Controller.GetOutput(i);
            var rectAdvance = GUILayoutUtility.GetRect(new GUIContent(outputMessage.Message), GUI.skin.label);

            messageRect.y -= rectAdvance.height;
            messageRect.height = rectAdvance.height;

            GUI.color = outputMessage.Color;
            GUI.Label(messageRect, outputMessage.Message);
        }

        GUI.color = oldColor;
    }
    
    void HandleConsoleInput()
    {
        var commandRect = DefaultInputRect;
            commandRect.x += Prompt.Length * 8;

        GUI.Label(DefaultInputRect, Prompt);
        GUI.SetNextControlName(CommandLineControlName);

        CommandInput = HistoryIndex == 0 ? UserInput : Controller.GetHistory(Controller.GetHistoryCount() + HistoryIndex);
        CommandInput = GUI.TextField(commandRect, CommandInput, GUI.skin.label);

        if (GUI.GetNameOfFocusedControl() != CommandLineControlName)
        {
        //    GUI.FocusControl(CommandLineControlName);
        }

        if (HistoryIndex == 0 || !Controller.GetHistory(Controller.GetHistoryCount() + HistoryIndex).Equals(CommandInput))
        {
            HistoryIndex = 0;
            UserInput = CommandInput;
        }
    }

    void HandleConsoleWindow(int id)
    {
        if (HandleConsoleEvents())
            return;

        HandleConsoleOutput();
        HandleConsoleInput();
    }
}
