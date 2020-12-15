namespace Compiler.Nodes
{
    public class RepeatCommandNode :ICommandNode
    { 
        public ICommandNode Command { get;}

        public IExpressionNode Expression { get; }

        public Position Position { get; }


        public RepeatCommandNode(ICommandNode command, IExpressionNode expression, Position position)
        {
            Command = command;
            Expression = expression;
            Position = position;
        }
    }
}
