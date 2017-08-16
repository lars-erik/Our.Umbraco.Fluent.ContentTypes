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
            }
        };

        public static void Verify(this Diffgram value)
        {
            Approvals.VerifyJson(value.ToJson());
        }
        public static void Verify<TConfiguration, TEntity>(this EntityDiffgram<TConfiguration, TEntity> value)
        {
            Approvals.VerifyJson(value.ToJson());
        }

        public static string ToJson(this object obj, Formatting formatting = Formatting.Indented)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }
    }
}