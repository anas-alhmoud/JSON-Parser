using System;
using System.Collections.Generic;

namespace JSON_Parser
{
    public class JSONValue
    {
        public string NodeType { get; }

        public JSONValue(string value)
        {
            NodeType = "single-value";
            Value = value;
        }

        public JSONValue(List<KeyValue> value)
        {
            NodeType = "object";
            Object = value;
        }

        public JSONValue(List<JSONValue> value)
        {
            NodeType = "array";
            Array = value;
        }

        private string Value;
        private List<KeyValue> Object;
        private List<JSONValue> Array;

        public string getVal()
        {
            if(NodeType == "single-value")
                return Value;
            throw new Exception("You can't call getVal on type " + NodeType);
        }

        public List<KeyValue> getObj()
        {
            if (NodeType == "object")
                return Object;
            throw new Exception("You can't call getObj on type " + NodeType);
        }

        public List<JSONValue> getArr()
        {
            if (NodeType == "array")
                return Array;
            throw new Exception("You can't call getArr on type " + NodeType );
        }

    }


    public class KeyValue {
        public string key;
        public JSONValue value;
    }

    public class JSON
    {
        public static JSONValue parse(string str) {

            Input i = new Input(str);
            Tokenizer tkzr = new Tokenizer(i,
                new Tokenizable[]
                {
                new StringTokenizer(),
                new KeywordsTokenizer(new List<string>
                {
                    "true","false","null"
                }),
                new NumberTokenizer(),
                new NewLineTokenizer(true),
                new WhiteSpaceTokenizer(true),
                new JSymbolsTokenizer('[', "array-start"),
                new JSymbolsTokenizer(']', "array-end"),
                new JSymbolsTokenizer(':', "colon"),
                new JSymbolsTokenizer(',', "comma"),
                new JSymbolsTokenizer('{', "object-start"),
                new JSymbolsTokenizer('}', "object-end")
                }
                );
            Token tkn = tkzr.tokenize();

            JSONValue result = checkToken(tkn, tkzr);

            Token nexTkn = tkzr.tokenize();

            if(nexTkn != null)
            {
                throw new Exception("");
            }

            return result;
        }

        private static List<JSONValue> collectArrayValue(Tokenizer tkzr)
        {
            List<JSONValue> JList = new List<JSONValue>();

            Token token = tkzr.tokenize();

            if (token.Type == "array-end")
            {
                return JList;
            }

            JList.Add(checkToken(token, tkzr));


            while (true)
            {
                token = tkzr.tokenize();

                if (token.Type != "comma")
                {
                    break;
                }

                token = tkzr.tokenize();
                JList.Add(checkToken(token, tkzr));
            }

            if (token.Type == "array-end")
            {
                return JList;
            }

            throw new Exception("Error");
        }

        private static List<KeyValue> collectObjectValue(Tokenizer tkzr)
        {
            List<KeyValue> JList = new List<KeyValue>();

            Token token = tkzr.tokenize();

            if (token.Type == "object-end")
            {
                return JList;
            }


            while (true)
            {
                if (token.Type == "string")
                {
                    KeyValue keyValue = new KeyValue();
                    keyValue.key = token.Value;

                    token = tkzr.tokenize();

                    if (token.Type == "colon")
                    {
                        token = tkzr.tokenize();

                        keyValue.value = checkToken(token, tkzr);
                        JList.Add(keyValue);

                        token = tkzr.tokenize();

                        if (token.Type != "comma")
                        {
                            break;
                        }

                        token = tkzr.tokenize();

                        if (token.Type == "object-end")
                        {
                            throw new Exception("Error");
                        }

                        continue;
                    }

                }

                break;
            }


            /*
             * 
             * 
             *                 token = tkzr.tokenize();

                if (token.Type == "string")
                {
                    KeyValue keyValue = new KeyValue();
                    keyValue.key = token.Value;

                    token = tkzr.tokenize();

                    if (token.Type == "colon")
                    {
                        token = tkzr.tokenize();

                        keyValue.value = checkToken(token, tkzr);
                        JList.Add(keyValue);
                    }

                }

                break;
             * 
             * 
             * 
             * 
             */
            if (token.Type == "object-end")
            {
                return JList;
            }

            throw new Exception("Error");
        }

        private static JSONValue checkToken(Token tkn, Tokenizer tkzr)
        {
            if (tkn.Type == "string")
            {
                return new JSONValue(tkn.Value);
            }
            else if (tkn.Type == "number")
            {
                return new JSONValue(tkn.Value);
            }
            else if (tkn.Type == "boolean")
            {
                return new JSONValue(tkn.Value);
            }
            else if (tkn.Type == "null")
            {
                return new JSONValue(tkn.Value);
            }
            else if (tkn.Type == "array-start")
            {
                return new JSONValue(collectArrayValue(tkzr));
            }
            else if (tkn.Type == "object-start")
            {
                return new JSONValue(collectObjectValue(tkzr));
            }

            throw new Exception("Error");
        }
    }
}
