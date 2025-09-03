using System.Collections.Generic;

namespace BehaviorTree
{
    // E Lógico: Executa filhos em ordem, falha se QUALQUER UM falhar.
    public class Sequence : Node
    {
        private List<Node> _children = new List<Node>();
        public Sequence(List<Node> children) { _children = children; }

        public override NodeState Evaluate()
        {
            foreach (Node node in _children)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE:
                        return NodeState.FAILURE;
                    case NodeState.SUCCESS:
                        continue;
                    case NodeState.RUNNING:
                        return NodeState.RUNNING;
                }
            }
            return NodeState.SUCCESS;
        }
    }
}