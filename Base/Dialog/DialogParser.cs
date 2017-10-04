using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    public class DialogParser : ScriptParser
    {
        private class UnresolvedToken
        {
            internal UnresolvedToken(string name, int line, DialogTopic topic)
            {
                Name = name;
                Line = line;
                State = topic;
                InsertPosition = topic.Items.Count;
            }

            internal UnresolvedToken(string name, int line, DialogMenu menu, string text)
            {
                Name = name;
                Line = line;
                State = menu;
                Text = text;
                InsertPosition = menu.Items.Count;
            }

            public string Name;
            public string Text;
            public DialogState State;
            public int Line;
            public int InsertPosition;
        }

        private List<UnresolvedToken> Gotos = new List<UnresolvedToken>();
        private List<UnresolvedToken> Invokes = new List<UnresolvedToken>();

        private string FirstElement;

        private DialogTopic LastTopic;
        private DialogMenu LastMenu;

        private Dialog DialogObject;

        protected override UnityEngine.Object ParseTarget
        {
            get { return DialogObject; }
        }

        protected override bool Validate()
        {
            bool success = true;

            var firstElement = DialogObject.States.Dictionary.Values.FirstOrDefault(s => s.name.Equals(FirstElement));
            if (firstElement != null)
            {
                DialogObject.FirstState = firstElement;
            }
            else
            {
                Error("Start uses undeclared state!", 0);
                success = false;
            }

            foreach (var token in Invokes)
            {
                var topic = token.State as DialogTopic;
                if (!topic)
                {
                    Error("Unable to insert invoke!", token.Line);
                    success = false;
                }
                else
                {
                    var func = DialogObject.Functions.FirstOrDefault(f => f.name.Equals(token.Name));
                    if (!func)
                    {
                        Error(string.Format("Invoke of undefined function '{0}'", token.Name), token.Line);
                        success = false;
                    }
                    else
                    {
                        topic.Items.Insert(token.InsertPosition, new DialogTopic.InvokeItem()
                        {
                            Function = func
                        });
                    }
                }
            }

            foreach (var token in Gotos)
            {
                var topic = token.State as DialogTopic;
                var menu = token.State as DialogMenu;
                if (topic == null && menu == null)
                {
                    Error("Unable to insert goto!", token.Line);
                    success = false;
                }
                else
                {
                    var state = DialogObject.States.Dictionary[token.Name];
                    if (!state)
                    {
                        Error(string.Format("Goto to undefined state '{0}'", token.Name), token.Line);
                        success = false;
                    }
                    else
                    {
                        if (topic)
                        {
                            topic.Items.Insert(token.InsertPosition, new DialogTopic.GotoItem()
                            {
                                State = state
                            });
                        }
                        else if (menu)
                        {
                            menu.Items.Insert(token.InsertPosition, new DialogMenu.MenuItem()
                            {
                                State = state,
                                Text = token.Text
                            });
                        }
                    }
                }
            }

            return success;
        }

        public DialogParser(Dialog dialogObject)
        {
            DialogObject = dialogObject;

            KeywordDictionary["start"] = ParseStart;
            KeywordDictionary["actor"] = ParseActor;
            KeywordDictionary["function"] = ParseFunction;
            KeywordDictionary["topic"] = ParseTopic;
            KeywordDictionary["say"] = ParseSay;
            KeywordDictionary["invoke"] = ParseInvoke;
            KeywordDictionary["goto"] = ParseGoto;
            KeywordDictionary["menu"] = ParseMenu;
            KeywordDictionary["exit"] = ParseExit;
        }

        private bool ParseStart(string arguments)
        {
            // start DialogStateName
            if (string.IsNullOrEmpty(arguments))
                return false;

            if (!string.IsNullOrEmpty(FirstElement))
                Warning(string.Format("Start element redefined from '{0}' to '{1}'", FirstElement, arguments));

            FirstElement = arguments;

            return true;
        }

        private bool ParseActor(string arguments)
        {
            // actor ActorName
            if (string.IsNullOrEmpty(arguments))
                return false;

            if (DialogObject.Actors.Any(a => a.name.Equals(arguments)))
            {
                Warning(string.Format("Actor '{0}' defined twice", arguments));
            }
            else
            {
                var actor = ScriptableObject.CreateInstance<DialogActorSlot>();
                actor.name = arguments;
                DialogObject.Actors.Add(actor);
                AddToAsset(actor);
            }

            return true;
        }

        private bool ParseFunction(string arguments)
        {
            // function FunctionName
            if (string.IsNullOrEmpty(arguments))
                return false;

            if (DialogObject.Functions.Any(f => f.name.Equals(arguments)))
            {
                Warning(string.Format("Function '{0}' defined twice", arguments));
            }
            else
            {
                var func = ScriptableObject.CreateInstance<DialogFunctionSlot>();
                func.name = arguments;
                DialogObject.Functions.Add(func);
                AddToAsset(func);
            }

            return true;
        }

        private bool ParseTopic(string arguments)
        {
            // topic TopicName
            LastMenu = null;

            if (string.IsNullOrEmpty(arguments))
                return false;

            if (DialogObject.States.Dictionary.ContainsKey(arguments))
            {
                Error(string.Format("Topic '{0}' defined twice", arguments));
                return false;
            }

            LastTopic = ScriptableObject.CreateInstance<DialogTopic>();
            LastTopic.name = arguments;
            DialogObject.States.Dictionary[arguments] = LastTopic;
            AddToAsset(LastTopic);

            return true;
        }

        private bool ParseSay(string arguments)
        {
            // say ActorName "What does the actor say?"
            if (string.IsNullOrEmpty(arguments))
                return false;

            if (LastTopic == null)
            {
                Error("Say must be called in topic context!");
                return false;
            }

            int textBegin = arguments.IndexOf("\"", StringComparison.Ordinal);
            int textEnd   = arguments.LastIndexOf("\"", StringComparison.Ordinal);

            if (textBegin < 0 || textEnd < 0
            || textBegin >= arguments.Length 
            || textEnd >= arguments.Length)
            {
                Error("Not matching say text quotes!");
                return false;
            }

            string actorName = arguments.Substring(0, textBegin - 1);
            string text  = arguments.Substring(textBegin + 1, textEnd - textBegin - 1);

            var  actor = DialogObject.Actors.FirstOrDefault(a => a.name.Equals(actorName));
            if (!actor)
            {
                Error(string.Format("Say uses undefined actor {0}!", actorName));
                return false;
            }

            LastTopic.Items.Add(new DialogTopic.SayItem()
            {
                Actor = actor,
                Text  = text
            });
            
            return true;
        }

        private bool ParseInvoke(string arguments)
        {
            // invoke FunctionName
            if (string.IsNullOrEmpty(arguments))
                return false;

            if (LastTopic == null)
            {
                Error("Invoke must be called in topic context!");
                return false;
            }

            Invokes.Add(new UnresolvedToken(arguments, CurrentLine, LastTopic));
            
            return true;
        }

        private bool ParseGoto(string arguments)
        {
            // goto DialogStateName
            if (string.IsNullOrEmpty(arguments))
                return false;

            if (LastTopic == null && LastMenu == null)
            {
                Error("Invoke must be called in menu or topic context!");
                return false;
            }

            string state = string.Empty;
            string text = string.Empty;

            int textBegin = arguments.IndexOf("\"", StringComparison.Ordinal);
            int textEnd   = arguments.LastIndexOf("\"", StringComparison.Ordinal);

            if (textBegin >= 0 && textBegin < arguments.Length
            &&  textEnd >= 0 && textEnd < arguments.Length)
            {
                state = arguments.Substring(0, textBegin - 1);
                text = arguments.Substring(textBegin + 1, textEnd - textBegin - 1);
            }
            else
            {
                state = arguments;
            }

            if (LastMenu != null && string.IsNullOrEmpty(text))
            {
                Error("Goto in menu must be called with text to display!");
                return false;
            }

            if (LastTopic != null)
                Gotos.Add(new UnresolvedToken(state, CurrentLine, LastTopic));
            
            if (LastMenu != null)
                Gotos.Add(new UnresolvedToken(state, CurrentLine, LastMenu, text));

            return true;
        }

        private bool ParseMenu(string arguments)
        {
            // menu MenuName
            LastTopic = null;

            if (string.IsNullOrEmpty(arguments))
                return false;

            if (DialogObject.States.Dictionary.ContainsKey(arguments))
            {
                Error(string.Format("Menu '{0}' defined twice", arguments));
                return false;
            }

            LastMenu = ScriptableObject.CreateInstance<DialogMenu>();
            LastMenu.name = arguments;
            DialogObject.States.Dictionary[arguments] = LastMenu;
            AddToAsset(LastMenu);

            return true;
        }

        private bool ParseExit(string arguments)
        {
            // exit
            if (LastTopic == null && LastMenu == null)
            {
                Error("Exit must be called in menu or topic context!");
                return false;
            }

            if (LastTopic != null)
            {
                LastTopic.Items.Add(new DialogTopic.ExitItem());
            }

            if (LastMenu != null)
            {
                LastMenu.Items.Add(new DialogMenu.MenuItem()
                {
                    Text = "Exit"
                });
            }

            return true;
        }
    }
}
