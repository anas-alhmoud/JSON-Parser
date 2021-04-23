using System;
using System.Collections.Generic;
using System.Text;

namespace JSON_Parser
{
    abstract class JSONValue
    {

    }

    abstract class SingleJSONValue<T> : JSONValue
    {
        public T Value { get; set; }
    }

    abstract class CollectionJSONValue<T> : JSONValue
    {
        public List<T> Value { get; set; }
    }

    class StringJSONValue : SingleJSONValue<string> { }
    class NumberJSONValue : SingleJSONValue<double> { }
    class BoolenJSONValue : SingleJSONValue<bool> { }
    class NullJSONValue : SingleJSONValue<bool?> { }

    class ObjectJSONValue : CollectionJSONValue<KeyValue> { }
    class ArrayJSONValue : CollectionJSONValue<JSONValue> { }

    class KeyValue {
        public string key;
        public JSONValue value;
    }

    class JSON
    {
        public static JSONValue parse(string str) { return null; }
    }
}
