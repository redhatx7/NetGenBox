namespace NetGenBox.Parser.Types;



public enum AtomType
{
    Identifier = 1 << 0,
    String = 1 << 1,
    Number = 1 << 2,
}

public class LispAtom : ILispExpression
{
    public AtomType AtomType { get; }
    public string Literal { get;  }

    public LispAtom(string literal, AtomType type)
    {
        AtomType = type;
        Literal = literal;
    }

    public override string ToString()
    {
        return Literal;
    }
}