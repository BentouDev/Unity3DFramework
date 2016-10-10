using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ConsoleController
{
    private readonly List<OutputMessage> ConsoleOutput = new List<OutputMessage>();
    private readonly List<string> CommandHistory = new List<string>();

    public Color CommandColor;
    public Color ErrorColor;
    public Color WarningColor;
    public Color NormalColor;

    private readonly Dictionary<string, CommandInfo> Commands = new Dictionary<string, CommandInfo>();
    private readonly Dictionary<string, VariableInfo> Variables = new Dictionary<string, VariableInfo>();

    public delegate bool CommandHandler(params string[] param);

    public class VariableInfo
    {
        public delegate float GetFloat();
        public delegate void SetFloat(float val);

        public delegate double GetDouble();
        public delegate void SetDouble(double val);

        public delegate int GetInt();
        public delegate void SetInt(int val);

        public delegate string VariablePrinter();
        public delegate bool VariableParser(string value);

        public readonly string Name;
        public readonly string Help;
        public readonly VariablePrinter Printer;
        public readonly VariableParser Parser;

        public VariableInfo(string name, string help, VariableParser parser, VariablePrinter printer)
        {
            Name = name;
            Help = help;
            Printer = printer;
            Parser = parser;
        }
    }

    public struct CommandInfo
    {
        public CommandInfo(string name, string help, CommandHandler command)
        {
            Name = name;
            Help = help;
            Command = command;
        }

        public readonly string Name;
        public readonly string Help;
        public readonly CommandHandler Command;
    }

    public struct OutputMessage
    {
        public readonly Color Color;
        public readonly string Message;

        public OutputMessage(string msg, Color col)
        {
            Message = msg;
            Color = col;
        }
    }

    public ConsoleController()
    {
        RegisterBasicCommands();
    }

    private void RegisterBasicCommands()
    {
        RegisterCommand("help", "prints this message", CmdHelp);
        RegisterCommand("info", "prints detalied information about this game", CmdInfo);
        RegisterCommand("clear", "clears console output", CmdClear);
        RegisterCommand("echo", "prints message to console, @ to print variable value", CmdEcho);
        RegisterCommand("nya", ";3", CmdNya);
        RegisterCommand("set", "set various game related variables", CmdSetVar);
    }

    public string GetHistory(int i)
    {
        return CommandHistory[i];
    }

    public int GetHistoryCount()
    {
        return CommandHistory.Count;
    }

    public int GetOutputCount()
    {
        return ConsoleOutput.Count;
    }

    public OutputMessage GetOutput(int i)
    {
        return ConsoleOutput[i];
    }

    public void CommitCommand(string commandInput)
    {
        if (string.IsNullOrEmpty(commandInput))
            return;

        string command;
        string[] parameters;

        bool parseSuccessfull = TryParseCommand(commandInput, out parameters, out command);
        if (parseSuccessfull)
        {
            bool result = RunCommand(command, parameters);
        }
        else
        {
            Error("Error when parsing command input!");
        }

        CommandHistory.Add(commandInput);
    }

    public bool TryParseCommand(string commandInput, out string[] param, out string command)
    {
        string[] keywords = { };

        bool inQuote = false;
        int quote = 0;
        int keywordBegin = 0;
        int currentChar = 0;
        int inputLength = commandInput.Length;
        int wordStart = 0;
        int wordLength = 0;

        while (currentChar < inputLength)
        {
            char c = commandInput[currentChar];

            wordStart = keywordBegin;
            wordLength = currentChar - keywordBegin;

            if (c == '"')
            {
                quote++;
                inQuote = !inQuote;
            }
            if (!inQuote && c == ' ')
            {
                if (keywordBegin == currentChar)
                    keywordBegin++;
                else
                {
                    if (quote > 0)
                    {
                        wordStart++;
                        wordLength -= quote;
                    }

                    Array.Resize(ref keywords, keywords.Length + 1);
                    keywords[keywords.Length - 1] = commandInput.Substring(wordStart, wordLength);

                    keywordBegin = currentChar + 1;
                    quote = 0;
                }
            }

            currentChar++;
        }

        wordStart = keywordBegin;
        wordLength = currentChar - keywordBegin;

        if (quote != 0 && quote != 2)
        {
            command = string.Empty;
            param = new string[] { };
            return false;
        }
        else if (quote == 2)
        {
            wordStart++;
            wordLength -= 2;
        }

        Array.Resize(ref keywords, keywords.Length + 1);
        keywords[keywords.Length - 1] = commandInput.Substring(wordStart, wordLength);

        int paramCount = keywords.Length - 1;

        param = new string[paramCount];
        command = keywords[0];

        Array.Copy(keywords, 1, param, 0, paramCount);

        return true;
    }

    bool RunCommand(string command, params string[] param)
    {
        if (!Commands.ContainsKey(command))
        {
            Error(string.Format("Command '{0}' unrecognized!", command));
            return false;
        }
        else
        {
            var cmd = Commands[command];
            if (cmd.Command != null)
            {
                bool result = false;
                try
                {
                    Print(command, CommandColor);
                    result = cmd.Command(param);
                }
                catch (Exception e)
                {
                    Error(string.Format("Command '{0}' throwed an exception! \n {1}", command, e.ToString()));
                    result = false;
                }
                return result;
            }
            else
            {
                Error(string.Format("Command '{0}' handler was null!", command));
                return false;
            }
        }
    }

    #region Public API

    public void RegisterCommand(string name, string help, CommandHandler handler)
    {
        if (Commands.ContainsKey(name))
            Commands[name] = new CommandInfo(name, help, handler);
        else
            Commands.Add(name, new CommandInfo(name, help, handler));
    }

    public void RegisterVariable(string name, string help, VariableInfo.VariableParser parser, VariableInfo.VariablePrinter printer)
    {
        if (Variables.ContainsKey(name))
            Variables[name] = new VariableInfo(name, help, parser, printer);
        else
            Variables.Add(name, new VariableInfo(name, help, parser, printer));
    }

    public void Error(string message)
    {
        Print(message, ErrorColor);
/*#if UNITY_EDITOR
        if (PasueOnError)
        {
            Show = true;
            EditorApplication.isPlaying = false;
        }
#endif*/
    }

    public void Warning(string message)
    {
        Print(message, WarningColor);
    }

    public void Print(string message)
    {
        Print(message, Color.white);
    }

    public void Print(string message, Color color)
    {
        ConsoleOutput.Add(new OutputMessage(message, color));
    }

    public void Clear()
    {
        ConsoleOutput.Clear();
    }
    
    public static string GetGameInfoString()
    {
        return string.Format("{0}, {1} version {2} \nUnity version {3} \n\u00A9 2016 - {4} {5}. All rights reserved.",
            Application.productName, Application.platform, Application.version, Application.unityVersion, DateTime.Now.Year, Application.companyName);
    }

    #endregion Public API
    #region Commandlets

    private bool CmdSetVar(params string[] param)
    {
        if (param.Length == 0)
        {
            Print("Registred variables:");
            foreach (var variable in Variables.Values)
            {
                Print(string.Format("{0} - {1} = {2}", variable.Name, variable.Help, variable.Printer()));
            }
            return true;
        }
        else if (param.Length == 2)
        {
            Print(string.Format("Variable '{0}' is now '{}'", param[0], param[1]));
            return true;
        }
        else
        {
            Error("Invalid argument count, expected 0 or 2, got " + param.Length);
            return false;
        }
    }

    private bool CmdInfo(params string[] param)
    {
        Print(GetGameInfoString());
        return true;
    }

    private bool CmdNya(params string[] param)
    {
        Print("What did you expect?", Color.magenta);
        throw new InvalidOperationException();
    }

    private bool CmdHelp(params string[] param)
    {
        Print(GetGameInfoString());

        foreach (var command in Commands.Values)
        {
            Print(string.Format("{0} - {1}", command.Name, command.Help));
        }

        return true;
    }

    private bool CmdClear(params string[] param)
    {
        Clear();
        return true;
    }

    private bool CmdEcho(params string[] param)
    {
        StringBuilder sb = new StringBuilder();
        foreach (string arg in param)
        {
            sb.AppendFormat("{0},", arg);
        }

        sb.Remove(sb.Length - 1, 1);
        Print(sb.ToString());

        return true;
    }

    #endregion Commandlets
}
