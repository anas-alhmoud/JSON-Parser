using System;
using System.Collections.Generic;

namespace JSON_Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            string testCase = "true  false \"anas\" null    -0 0.423 -0.23  0.22e4 0.22e+4 0.22e-4 -0.22e4 -0.22e+4 -0.22e-4 0. 8468";
            string tc = "3224 -3231  13.31 -3242.32   2E+3 2E3 2E-3 265e+324 265e324 265e-324 -2E+3 -2E3 -2E-3 -265e+324 -265e324 -265e-324";
            Tokenizer t = new Tokenizer(new Input(testCase), new Tokenizable[] {
                new StringTokenizer(),
                new KeywordsTokenizer(new List<string>
                {
                    "true","false","null"
                }),
                new NumberTokenizer(),
                new WhiteSpaceTokenizer(),

            }); 
            Token token = t.tokenize();
            
            while (token != null)
            {
                Console.WriteLine(token.Value + " ---> " + token.Type);
                token = t.tokenize();
            }
            

        }
    }

    public delegate bool InputCondition(Input input);
    public class Input
    {
        private readonly string input;
        private readonly int length;
        private int position;
        private int lineNumber;
        //Properties
        public int Length
        {
            get
            {
                return this.length;
            }
        }
        public int Position
        {
            get
            {
                return this.position;
            }
        }
        public int NextPosition
        {
            get
            {
                return this.position + 1;
            }
        }
        public int LineNumber
        {
            get
            {
                return this.lineNumber;
            }
        }
        public char Character
        {
            get
            {
                if (this.position > -1) return this.input[this.position];
                else return '\0';
            }
        }
        public Input(string input)
        {
            this.input = input;
            this.length = input.Length;
            this.position = -1;
            this.lineNumber = 1;
        }
        public bool hasMore(int numOfSteps = 1)
        {
            if (numOfSteps <= 0) throw new Exception("Invalid number of steps");
            return (this.position + numOfSteps) < this.length;
        }
        public bool hasLess(int numOfSteps = 1)
        {
            if (numOfSteps <= 0) throw new Exception("Invalid number of steps");
            return (this.position - numOfSteps) > -1;
        }
        //callback -> delegate
        public Input step(int numOfSteps = 1)
        {
            if (this.hasMore(numOfSteps))
                this.position += numOfSteps;
            else
            {
                throw new Exception("There is no more step");
            }
            return this;
        }
        public Input back(int numOfSteps = 1)
        {
            if (this.hasLess(numOfSteps))
                this.position -= numOfSteps;
            else
            {
                throw new Exception("There is no more step");
            }
            return this;
        }
        public Input reset() { return this; }
        public char peek(int numOfSteps = 1)
        {
            if (this.hasMore(numOfSteps)) return this.input[this.Position + numOfSteps];
            return '\0';
        }
        public string loop(InputCondition condition)
        {
            string buffer = "";
            while (this.hasMore() && condition(this))
                buffer += this.step().Character;

            return buffer;
        }
    }
    public class Token
    {
        public int Position { set; get; }
        public int LineNumber { set; get; }
        public string Type { set; get; }
        public string Value { set; get; }
        public Token(int position, int lineNumber, string type, string value)
        {
            this.Position = position;
            this.LineNumber = lineNumber;
            this.Type = type;
            this.Value = value;
        }
    }
    public abstract class Tokenizable
    {
        public abstract bool tokenizable(Tokenizer tokenizer);
        public abstract Token tokenize(Tokenizer tokenizer);
    }
    public class Tokenizer
    {
        public List<Token> tokens;
        public bool enableHistory;
        public Input input;
        public Tokenizable[] handlers;
        public Tokenizer(string source, Tokenizable[] handlers)
        {
            this.input = new Input(source);
            this.handlers = handlers;
        }
        public Tokenizer(Input source, Tokenizable[] handlers)
        {
            this.input = source;
            this.handlers = handlers;
        }
        public Token tokenize()
        {
            foreach (var handler in this.handlers)
                if (handler.tokenizable(this)) return handler.tokenize(this);
            return null;
        }
        public List<Token> all() { return null; }
    }

    public class WhiteSpaceTokenizer : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            return Char.IsWhiteSpace(t.input.peek());
        }
        public override Token tokenize(Tokenizer t)
        {
            t.input.step();
            return new Token(t.input.Position, t.input.LineNumber,
                "whitespace", " ");
        }
    }
    public class JSymbolsTokenizer : Tokenizable
    {

        private List<char> validSymobls;
        private char symbol;
        private string type;


        public JSymbolsTokenizer(char symbol, string type)
        {
            this.symbol = symbol;
            this.type = type;
        }
        public override bool tokenizable(Tokenizer t)
        {
            return this.symbol == t.input.peek();
        }

        public override Token tokenize(Tokenizer t)
        {
            char currentChar = t.input.peek();
            t.input.step();
            return new Token(t.input.Position, t.input.LineNumber,
                this.type, currentChar.ToString());
        }
    }


    public class KeywordsTokenizer : Tokenizable
    {
        private List<string> keywords;
        public KeywordsTokenizer(List<string> keywords)
        {
            this.keywords = keywords;
        }
        public override bool tokenizable(Tokenizer t)
        {
            return isLetter(t.input);
        }
        static bool isLetter(Input input)
        {
            char currentCharacter = input.peek();
            return Char.IsLetter(currentCharacter);
        }
        public override Token tokenize(Tokenizer t)
        {
            string value = t.input.loop(isLetter);

            string type;
            if (value == "null")
                type = "null";
            else 
                type = "boolean";

            if (!this.keywords.Contains(value))
                throw new Exception("Unexpected token");

            return new Token(t.input.Position, t.input.LineNumber,
                type, value);
        }
    }

    public class StringTokenizer : Tokenizable
    {
        public override bool tokenizable(Tokenizer t)
        {
            return t.input.peek() == '"';
        }

        public override Token tokenize(Tokenizer t)
        {
            t.input.step();

            string value = t.input.loop(input => input.peek() != '"');

            if (t.input.peek() != '"')
                throw new Exception("Error");

            t.input.step();

            return new Token(t.input.Position, t.input.LineNumber,
                    "string", value);
        }
    }

    public class NumberTokenizer : Tokenizable
    {
        private bool isNegative;
        private bool isZero;
        public override bool tokenizable(Tokenizer t)
        {
            if (t.input.peek(2) == '0' || t.input.peek() == '0')
            {
                isZero = true;
            }
            else
            {
                isZero = false;
            }

            if (t.input.peek() == '-' && Char.IsDigit(t.input.peek(2)) )
            {
                isNegative = true;
                return true;
            } else
            {
                isNegative = false;
                return Char.IsDigit(t.input.peek());
            }

        }
        public override Token tokenize(Tokenizer t)
        {
            string value = "";
            if (isZero)
            {
                if (isNegative)
                {
                    if (Char.IsDigit(t.input.step().step().peek()))
                        throw new Exception("Error");

                    value = "-";

                }
                else
                {
                    if (Char.IsDigit(t.input.step().peek()))
                        throw new Exception("Error");
                }

                value += "0";
            } else
            {
                if (isNegative)
                {
                    value += t.input.step().Character;
                }

                value += t.input.loop(input => Char.IsDigit(t.input.peek()));

            }

            if (t.input.peek() == '.' && Char.IsDigit(t.input.peek(2)))
            {
                value += t.input.step().Character;
                value += t.input.loop(input => Char.IsDigit(t.input.peek()));
            }

            if ( (t.input.peek() == 'E' || t.input.peek() == 'e') && ( Char.IsDigit(t.input.peek(2)) || t.input.peek(2) == '+' || t.input.peek(2) == '-') )
            {
                value += t.input.step().Character;
                value += t.input.step().Character;

                value += t.input.loop(input => Char.IsDigit(t.input.peek()));
            }

            return new Token(t.input.Position, t.input.LineNumber, "number", value);
        }

    }

}
