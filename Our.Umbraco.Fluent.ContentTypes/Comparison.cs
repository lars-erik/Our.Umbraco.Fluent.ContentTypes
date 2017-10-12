using System.Diagnostics;

namespace Our.Umbraco.Fluent.ContentTypes
{
    [DebuggerDisplay("Comparison of {Key}: {Result}")]
    public class Comparison
    {
        public string Key { get; }

        public ComparisonResult Result { get; }

        public string Discriminator { get; }

        public Comparison(string key, ComparisonResult result)
        {
            Key = key;
            Result = result;
        }

        public Comparison(string key, string discriminator, ComparisonResult result)
            : this(key, result)
        {
            Discriminator = discriminator;
        }
    }
}