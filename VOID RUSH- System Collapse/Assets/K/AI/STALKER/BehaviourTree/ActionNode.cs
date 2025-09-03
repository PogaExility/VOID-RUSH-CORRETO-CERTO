using System;

namespace BehaviorTree
{
    // A Folha da Árvore: Executa uma ação do mundo real.
    public class ActionNode : Node
    {
        private Func<NodeState> _action;
        public ActionNode(Func<NodeState> action) { _action = action; }

        public override NodeState Evaluate()
        {
            return _action();
        }
    }
}