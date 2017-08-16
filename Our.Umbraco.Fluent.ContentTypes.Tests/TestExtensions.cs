using System.Collections.Generic;
using ApprovalTests;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public static class TestExtensions
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter()
            },
            Formatting = Formatting.Indented
        };

        public static void Verify(this Diffgram value)
        {
            Approvals.VerifyJson(JsonConvert.SerializeObject(value, Settings));
        }
        public static void Verify<TConfiguration, TEntity>(this EntityDiffgram<TConfiguration, TEntity> value)
        {
            Approvals.VerifyJson(JsonConvert.SerializeObject(value, Settings));
        }

        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }
    }
}