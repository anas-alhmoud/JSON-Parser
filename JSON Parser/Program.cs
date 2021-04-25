using System;
using System.Collections.Generic;

namespace JSON_Parser
{
    class Program
    {
       
        static void Main(string[] args)
        {
            JSONValue root = JSON.parse(@"{
                                ""data"": ""Click Here"",
                                ""size"": 1,
                                ""style"": ""bold"",
                                ""name"": ""text1"",
                                ""hOffset"": ""one"",
                                ""vOffset"": ""two"",
                                ""alignment"": ""center"",
                                ""onMouseUp"": ""sun1.opacity = (sun1.opacity / v);"",
                                ""More"": {
                                    ""data"": [true, false],
                                    ""size"": 100,
                                    ""style"": false
                                    }
                            }");

            Console.WriteLine(
                "Key:" + root.getObj()[0].key + ", Value:" + root.getObj()[0].value.getVal()
                );
            Console.WriteLine(
                "Key:" + root.getObj()[1].key + ", Value:" + root.getObj()[1].value.getVal()
                );
            Console.WriteLine(
                "Key:" + root.getObj()[8].value.getObj()[0].key + ", Value:" + root.getObj()[8].value.getObj()[0].value.getArr()[0].getVal()
                );
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
            set
            {
                this.lineNumber = value;
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
        public bool isOptional;
        public Tokenizable(bool isOptional = false)
        {
            this.isOptional = isOptional;
        }
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
            for (int i = 0; i < this.handlers.Length; i++)
            {
                if (this.handlers[i].tokenizable(this))
                {
                    if (this.handlers[i].isOptional)
                    {
                        this.handlers[i].tokenize(this);
                        i = -1;
                    } else
                    {
                        return this.handlers[i].tokenize(this);
                    }
                    
                }
            }
                
            if(this.input.hasMore())
            throw new Exception("Tokenizer: Unexpected token at line number: " + this.input.LineNumber);

            return null;
        }
        public List<Token> all() { return null; }
    }

    public class WhiteSpaceTokenizer : Tokenizable
    {
        
        public WhiteSpaceTokenizer(bool isOptional = false)
        {
            this.isOptional = isOptional;
        }
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
    public class NewLineTokenizer : Tokenizable
    {
        public NewLineTokenizer(bool isOptional = false)
        {
            this.isOptional = isOptional;
        }
        public override bool tokenizable(Tokenizer t)
        {
            return String.Concat(t.input.peek(), t.input.peek(2)) == Environment.NewLine || t.input.peek().ToString() == Environment.NewLine;
        }

        public override Token tokenize(Tokenizer t)
        {
            t.input.step(Environment.NewLine.Length);
            t.input.LineNumber++;
            return new Token(t.input.Position, t.input.LineNumber,
                "NewLine", Environment.NewLine);
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
                throw new Exception("KeywordsTokenizer: Unknown keyword at line number: " + t.input.LineNumber);

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
                throw new Exception("StringTokenizer: string has not been closed at line number: " + t.input.LineNumber);

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
            if ( ( t.input.peek(2) == '0' && t.input.peek() == '-') || t.input.peek() == '0')
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
                        throw new Exception("NumberTokenizer: Unexpected number at line number: " + t.input.LineNumber);

                    value = "-";

                }
                else
                {
                    if (Char.IsDigit(t.input.step().peek()))
                        throw new Exception("NumberTokenizer: Unexpected number at line number: " + t.input.LineNumber);
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
