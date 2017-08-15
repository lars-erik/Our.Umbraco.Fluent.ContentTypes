using System.Collections.Generic;
using ApprovalTests;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Our.Umbraco.Fluent.ContentTypes.Tests
{
    public static class DiffgramTestExtensions
    {
        public static void Verify(this Diffgram value)
        {
            Approvals.VerifyJson(JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            }));
        }
        public static void Verify<TConfiguration, TEntity>(this EntityDiffgram<TConfiguration, TEntity> value)
        {
            Approvals.VerifyJson(JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                }
            }));
        }
    }
}