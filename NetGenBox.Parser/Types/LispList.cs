namespace NetGenBox.Parser.Types;

public class LispList : ILispExpression
{
    public List<ILispExpression> Expressions { get; } = new();

    public void AddToExpression(ILispExpression expression) => Expressions.Add(expression);

    public ILispExpression? Head => Expressions.FirstOrDefault();

    public ILispExpression? Tail => Expressions.LastOrDefault();

    public ILispExpression this[int index] => Expressions[index];
    

    public override string ToString()
    {
        return $"({string.Join(" ", Expressions)})";
    }
}