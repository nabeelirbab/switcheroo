using System;
using System.Collections.Generic;
using System.Text;

namespace Domain
{
    public class KeyValue
    {
        public string Key { get; set; }
        public int Value { get; set; }

        public KeyValue(string key, int value)
        {
            Key = key;
            Value = value;
        }
    }
}
