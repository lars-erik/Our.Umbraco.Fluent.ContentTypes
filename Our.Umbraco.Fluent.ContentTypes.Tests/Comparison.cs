namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public class Comparison
    {
        public string Key { get; }
        public ComparisonResult Result { get; }

        public Comparison(string key, ComparisonResult result)
        {
            Key = key;
            Result = result;
        }
    }
}