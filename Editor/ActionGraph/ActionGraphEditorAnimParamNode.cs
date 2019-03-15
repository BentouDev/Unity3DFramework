using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Framework.Editor
{
    [CustomActionEditor(typeof(ActionAnimParam))]
    public class ActionGraphEditorAnimParamNode : ActionGraphEditorNode
    {
        public ActionAnimParam Node => (ActionAnimParam) ActionNode;

        public ActionGraphEditorAnimParamNode(ActionGraph graph, ActionGraphNode node, ActionGraphPresenter presenter)
            : base(graph, node, presenter)
        {
            
        }

        private void SetNewParam(AnimatorControllerParameter param)
        {
            Undo.RecordObject(Node, "Changed anim param");
            Node.AnimParam.Name = param.name;

            switch (param.type)
            {
                case AnimatorControllerParameterType.Trigger:
                    Node.AnimParam = new Variant
                    (
                        new SerializedType(typeof(bool), ActionAnimParam.TriggerMetadata)
                    );
                    break;
                case AnimatorControllerParameterType.Bool:
                    Node.AnimParam = new Variant
                    (
                        typeof(bool)
                    );
                    Node.AnimParam.SetAs(param.defaultBool);
                    break;
                case AnimatorControllerParameterType.Float:
                    Node.AnimParam = new Variant
                    (
                        typeof(float)
                    );
                    Node.AnimParam.SetAs(param.defaultFloat);
                    break;
                case AnimatorControllerParameterType.Int:
                    Node.AnimParam = new Variant
                    (
                        typeof(int)
                    );
                    Node.AnimParam.SetAs(param.defaultInt);
                    break;
            }

            Node.AnimParam.Name = param.name;
        }

        protected override void DrawContent()
        {
            if (Node.Anim)
            {
                GUI.Box(drawRect, GUIContent.none, EditorStyles.helpBox);

                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(Node.Anim));
                if (controller == null)
                {
                    Debug.LogErrorFormat("AnimatorController must not be null.");
                    return;
                }

                int index = -1;
                var paramList = controller.parameters.ToList();
                var param = paramList.FirstOrDefault(p => p.name == Node.AnimParam.Name);
                if (param != null)
                {
                    index = paramList.IndexOf(param);
                }

                drawRect.x += ContentMargin;
                drawRect.width = drawRect.width - ContentMargin * 2;

                drawRect.height = VariantUtils.FieldHeight;
                int newIndex = EditorGUI.Popup(drawRect, index, paramList.Select(p => p.name).ToArray());
                if (newIndex != index)
                {
                    SetNewParam(paramList[newIndex]);
                }

                if (string.IsNullOrWhiteSpace(Node.AnimParam?.HoldType?.Metadata))
                {
                    drawRect.y += drawRect.height;
                    VariantUtils.DrawParameter(drawRect, Node.AnimParam, false);
                }
            }
            else
            {
                EditorGUI.HelpBox(drawRect, "AnimController is required!", MessageType.Error);
            }
        }
    }
}