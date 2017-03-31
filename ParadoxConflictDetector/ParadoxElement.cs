using System.Collections.Generic;

namespace ParadoxConflictDetector
{
    internal sealed class ParadoxElement
    {
        private readonly List<ParadoxElement> children;

        public ParadoxElement(string key, string value)
        {
            Key = key;
            Value = value;
        }
        public ParadoxElement(string value)
        {
            Value = value;
        }

        public ParadoxElement(string key, List<ParadoxElement> children)
        {
            Key = key;
            this.children = children;
        }

        public override string ToString()
        {
            if (Key == null)
                return Value;

            if (Value != null)
                return $"{Key} = {Value}";

            return $"{Key} = {{ }}";
        }

        public string Key { get; }

        public string Value { get; }

        public IReadOnlyList<ParadoxElement> Children => children;
    }
}