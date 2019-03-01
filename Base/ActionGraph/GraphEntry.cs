using System.Collections.Generic;

namespace Framework
{
    public struct ConditionsSelector
    {
        ActionGraphNode SelectNode(List<Condition> conditions, List<ActionGraphNode> nodes)
        {
            // TODO implement selection!
            return null;
        }
    }

    public abstract class GraphEntry : ActionGraphNodeBase
    {
        public override void OnSetupParametrizedProperties()
        { }
    }
}