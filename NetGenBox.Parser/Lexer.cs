using NetGenBox.Parser.Types;

namespace NetGenBox.Parser;

public class Lexer
{
    private readonly string _input;
    private int _position;
    private int _readPosition;
    private char _currentChar;

    public Lexer(string input)
    {
        _input = input;
        ReadChar();
    }

    private void ReadChar()
    {
        if (_readPosition >= _input.Length)
        {
            _currentChar = '\0'; // Indicates end of input
        }
        else
        {
            _currentChar = _input[_readPosition];
        }

        _position = _readPosition;
        _readPosition ++;
    }


    private void SkipWhiteSpaces()
    {
        while(_currentChar == '\r' || _currentChar == '\t' 
              || _currentChar == ' ' || _currentChar == '\n')
            ReadChar();
    }


    private string ReadIdentifier()
    {
        int pos = _position;
        while (char.IsLetterOrDigit(_currentChar) || _currentChar == '_')
            ReadChar();

        return _input.Substring(pos, _position - pos);
    }

    private string ReadNumber()
    {
        int pos = _position;
        while(char.IsDigit(_currentChar))
            ReadChar();

        return _input.Substring(pos, _position - pos );
    }

    private string ReadString()
    {
        int pos = _position;
        ReadChar();
        while (_currentChar != '\"')
            ReadChar();
        ReadChar();
        return _input.Substring(pos  , _position - pos);
    }
    public Token NextToken()
    {
        Token token;
        SkipWhiteSpaces();

        switch (_currentChar)
        {
            case '(':
                token = new Token(TokenType.LParen, '(');
                break;
            case ')':
                token = new Token(TokenType.RParen, ')');
                break;
            case (char)0:
                token = new Token(TokenType.Eof, '0');
                break;
            case '\"':
            {
                token = new Token(TokenType.String, ReadString());
                return token;
            }
            default:
            {
                if (char.IsLetter(_currentChar))
                {
                    token = new Token(TokenType.Identifier, ReadIdentifier());
                }
                else if (char.IsDigit(_currentChar))
                {
                    token = new Token(TokenType.Number, ReadNumber());
                }
                else
                {
                    Console.WriteLine($"Reached Illegal = {(int)_currentChar}");
                    token = new Token(TokenType.Illegal, _currentChar);
                }
                return token;
            }
        }
        
        ReadChar();
        
        return token;
    }
}