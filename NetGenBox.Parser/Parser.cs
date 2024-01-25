using System.Data;
using NetGenBox.Parser.Types;

namespace NetGenBox.Parser;

public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;
    private Token _peekToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        NextToken();
        NextToken(); // Initialize current and peek tokens
    }

    private void NextToken()
    {
        _currentToken = _peekToken;
        _peekToken = _lexer.NextToken();
    }


    public ILispExpression ParseExpression(int idx = 0)
    {
        if (_currentToken.Type == TokenType.Eof || _currentToken.Type == TokenType.Illegal)
            throw new InvalidExpressionException("Expression is invalid");

        Stack<LispList> stack = new Stack<LispList>();
        LispList currentList = new LispList(); // Initialize a top-level list
        stack.Push(currentList); // Start with the top-level list on the stack

        do
        {
            switch (_currentToken.Type)
            {
                case TokenType.LParen:
                    var newList = new LispList();
                    stack.Peek().Expressions.Add(newList);
                    stack.Push(newList);
                    break;

                case TokenType.RParen:
                    stack.Pop();
                    if (stack.Count == 0)
                    {
                        throw new InvalidExpressionException("Unbalanced parentheses in expression");
                    }
                    break;

                case TokenType.Identifier:
                case TokenType.Number:
                case TokenType.String:
                    AtomType atomType = AtomType.Identifier;
                    switch (_currentToken.Type)
                    {
                        case TokenType.Identifier:
                            atomType = AtomType.Identifier;
                            break;
                        case TokenType.Number:
                            atomType = AtomType.Number;
                            break;
                        case TokenType.String:
                            atomType = AtomType.String;
                            break;
                    }
                    var atom = new LispAtom(_currentToken.Literal, atomType );
                    stack.Peek().Expressions.Add(atom);
                    break;

                default:
                    throw new InvalidExpressionException("Unexpected token type");
            }

            NextToken();
        } while (_currentToken.Type != TokenType.Eof && stack.Count > 0);

        if (stack.Count > 1)
        {
            throw new InvalidExpressionException("Unbalanced parentheses in expression");
        }

        return stack.Pop(); // Return the top-level list
    }
    
    public LispList ParseProgram()
    {
        return (LispList)ParseExpression();
    }
}