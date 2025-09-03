using System.Collections.Generic;

namespace BehaviorTree
{
    // OU Lógico: Executa filhos em ordem, tem sucesso se QUALQUER UM tiver sucesso.
    public class Selector : Node
    {
        private List<Node> _children = new List<Node>();
        public Selector(List<Node> children) { _children = children; }

        public override NodeState Evaluate()
        {
            foreach (Node node in _children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        continue;
                    case NodeState.SUCCESS:
                        return NodeState.SUCCESS;
                    case NodeState.RUNNING:
                        return NodeState.RUNNING;
                }
            }
            return NodeState.FAILURE;
        }
    }
}