using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sabresaurus.SabreCSG;
using UnityEngine;
using UnityEditor;

namespace Framework
{
    public abstract class ScriptParser
    {
        public delegate bool LineParseDelegate(string arguments);

        protected readonly Dictionary<string, LineParseDelegate> KeywordDictionary;

        protected abstract UnityEngine.Object ParseTarget { get; }

        protected int CurrentLine { get; private set; }

        protected ScriptParser()
        {
            KeywordDictionary = new Dictionary<string, LineParseDelegate>();
        }

        public bool ProcessFile(string filePath)
        {
            CurrentLine = 0;

            bool success = true;
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        success &= ProcessLine(line);
                    }
                }
            }

            success &= Validate();

            if (success)
                Debug.Log("Parse successfull", ParseTarget);
            else
                Debug.LogError("Parse failed", ParseTarget);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return success;
        }

        protected abstract bool Validate();

        public bool ProcessLine(string line)
        {
            CurrentLine++;

            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                return true;

            string action     = string.Empty;
            string arguments  = string.Empty;
            bool   result     = false;
            int    keywordPos = line.IndexOf(' ');

            if (keywordPos > 0)
            {
                action = line.Substring(0, keywordPos);
                arguments = line.Substring(keywordPos + 1);
            }
            else
            {
                action = line;
            }

            if (!string.IsNullOrEmpty(action) && KeywordDictionary.ContainsKey(action))
            {
                result = KeywordDictionary[action].Invoke(arguments);
                if (!result)
                    Error(string.Format("Failed to parse token '{0}'", action));
            }
            else
            {
                Error("Cannot find a correct parsing process for line");
            }

            return result;
        }

        protected void AddToAsset(UnityEngine.Object asset)
        {
            AssetDatabase.AddObjectToAsset(asset, ParseTarget);
        }

        protected void Error(string message)
        {
            Debug.LogError(string.Format("{0} : line ({1}) : {2}", ParseTarget.name, CurrentLine, message), ParseTarget);
        }

        protected void Error(string message, int line)
        {
            Debug.LogError(string.Format("{0} : line ({1}) : {2}", ParseTarget.name, line, message), ParseTarget);
        }

        protected void Warning(string message)
        {
            Debug.LogWarning(string.Format("{0} : line ({1}) : {2}", ParseTarget.name, CurrentLine, message), ParseTarget);
        }
    }
}
