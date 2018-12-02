using System;
using System.Collections;
using UnityEngine;
using System.Globalization;

namespace Framework
{
    public class DebugConsole : MonoBehaviour
    {
        public ConsoleDisplay Display;
        public bool PasueOnError;
        public bool InterceptLogOutput;

        public string Prompt = " > ";

        [Header("Display")] public GUISkin Skin;

        public int Margin = 20;
        public int InputHeight = 25;

        private int HistoryIndex = 0;

        public string UserInput { get; private set; }
        public string CommandInput { get; private set; }

        public enum ConsoleDisplay
        {
            Hidden,
            HalfSize,
            FullSize
        }

        public Rect CurrentWindowRect
        {
            get
            {
                switch (Display)
                {
                    case ConsoleDisplay.HalfSize:
                        return HalfWindowRect;
                    case ConsoleDisplay.FullSize:
                        return DefaultWindowRect;
                }
                
                throw new System.InvalidOperationException("Unable to get console rect when its hidden!");
            }
        }

        public Rect HalfWindowRect
        {
            get
            {
                return new Rect
                (
                    Margin, Margin,
                    Screen.width - (Margin * 2),
                    Screen.height * 0.5f
                );
            }
        }

        public Rect DefaultWindowRect
        {
            get
            {
                return new Rect
                (
                    Margin, Margin,
                    Screen.width - (Margin*2),
                    Screen.height - (Margin*2)
                );
            }
        }

        public Rect DefaultInputRect
        {
            get
            {
                return new Rect
                (
                    CurrentWindowRect.xMin - Margin + 10,
                    CurrentWindowRect.yMax - Margin - InputHeight,
                    CurrentWindowRect.width - 10,
                    InputHeight
                );
            }
        }

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
                string timePrefix = string.Format("[{0}:{1}.{2}] : ", DateTime.Now.Hour, DateTime.Now.Minute,
                    DateTime.Now.Second);
                switch (logType)
                {
                    case LogType.Log:
                        Console.Print(timePrefix + string.Format(format, args), LogColor, ConsoleController.Output.Logger);
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
                Debug.unityLogger.logHandler = new LogInterceptor(Controller, Debug.unityLogger.logHandler, LogColor);
            }
        }

        public void ToggleConsole()
        {
            Display = Display.Next();
        }

        public void Close()
        {
            Display = ConsoleDisplay.Hidden;
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

        public void OutputUp()
        {
            Controller.PreviousOutput();
        }
        
        public void OutputDown()
        {
            Controller.NextOutput();
        }

        public void RegisterCommand(string name, string help,
            ConsoleController.CommandHandler handler)
        {
            Controller.RegisterCommand(name, help, handler);
        }

        public void RegisterVariableDataSet(IDataSet set)
        {
            Controller.RegisterVariableDataSet(set);
        }

//        public void RegisterVariable(string name, string help,
//            ConsoleController.VariableInfo.VariableParser parser,
//            ConsoleController.VariableInfo.VariablePrinter printer)
//        {
//            Controller.RegisterVariable(name, help, parser, printer);
//        }
//
//        public void RegisterVariableFloat(string name, string help,
//            ConsoleController.VariableInfo.SetFloat setter,
//            ConsoleController.VariableInfo.GetFloat getter)
//        {
//            RegisterVariable
//            (
//                name, help, str =>
//                {
//                    float val;
//                    if (!float.TryParse(str, out val)) return false;
//                    setter(val);
//                    return true;
//                },
//                () => getter().ToString(CultureInfo.InvariantCulture)
//            );
//        }

        public void Print(string message)
        {
            Controller.Print(message, ConsoleController.Output.Logger);
        }

        public void Print(string message, Color color)
        {
            Controller.Print(message, color, ConsoleController.Output.Logger);
        }

        public void Error(string message)
        {
            Controller.Error(message);
        }

        public void Warning(string message)
        {
            Controller.Warning(message);
        }

        private bool IsBackQuoteEvent()
        {
            return Event.current.isKey
                && Event.current.keyCode == KeyCode.BackQuote;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                ToggleConsole();
            }
        }

        void OnGUI()
        {
            if (IsBackQuoteEvent())
            {
                Event.current.Use();
                return;
            }

            if (Display == ConsoleDisplay.Hidden)
                return;

            if (Skin)
                GUI.skin = Skin;

            GUI.Window(12345, CurrentWindowRect, HandleConsoleWindow, "Console");
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
                
                case KeyCode.PageUp:
                    result = true;
                    current.Use();

                    OutputUp();
                    break;
                
                case KeyCode.PageDown:
                    result = true;
                    current.Use();

                    OutputDown();
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
            commandRect.x += GUI.skin.label.CalcSize(new GUIContent(Prompt)).x;//Prompt.Length*8;

            GUI.Label(DefaultInputRect, Prompt);
            GUI.SetNextControlName(CommandLineControlName);

            CommandInput = HistoryIndex == 0
                ? UserInput
                : Controller.GetHistory(Controller.GetHistoryCount() + HistoryIndex);
            CommandInput = GUI.TextField(commandRect, CommandInput, GUI.skin.label);

            CommandInput = CommandInput.Replace("`", string.Empty);

            if (GUI.GetNameOfFocusedControl() != CommandLineControlName)
            {
                GUI.FocusControl(CommandLineControlName);
            }

            if (HistoryIndex == 0 ||
                !Controller.GetHistory(Controller.GetHistoryCount() + HistoryIndex).Equals(CommandInput))
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
}