namespace NetGenBox.Parser.Types;

public enum TokenType
{
    
    Identifier,
    Number,
    LParen,
    RParen,
    Illegal,
    String,
    Eof,
}


public class Token
{
    public TokenType Type { get; }
    public string Literal { get; }
    
    public Token(TokenType type, char literal)
    {
        Type = type;
        Literal = literal.ToString();
    }

    public Token(TokenType type, string literal)
    {
        Type = type;
        Literal = literal;
    }

    public override string ToString()
    {
        return $"Token= {Literal} <---> Type= {Type}";
    }
}