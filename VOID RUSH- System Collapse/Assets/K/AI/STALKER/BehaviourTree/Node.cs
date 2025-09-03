namespace BehaviorTree
{
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    public abstract class Node
    {
        public abstract NodeState Evaluate();
    }
}