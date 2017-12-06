namespace IdentityBase.IntegrationTests
{
    using System.Collections.Generic;
    using System.Net.Http;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;
    using Newtonsoft.Json.Schema.Generation;
    using Xunit;

    public static class SchemaUtils
    {
        public static void AssertSchema(
            this HttpResponseMessage response,
            string schemaStr)
        {
            JSchema schema = JSchema.Parse(schemaStr);
            string json = response.Content.ReadAsStringAsync().Result;
            JObject user = JObject.Parse(json);
            bool valid = user.IsValid(schema, out IList<string> errorMessages);
            Assert.True(valid, string.Join("\n", errorMessages));
        }

        public static void AssertSchema(
            this HttpResponseMessage response,
            JSchema schema)
        {
            string json = response.Content.ReadAsStringAsync().Result;
            JObject user = JObject.Parse(json);
            bool valid = user.IsValid(schema, out IList<string> errorMessages);
            Assert.True(valid, string.Join("\n", errorMessages));
        }

        public static JSchema GenerateSchema<TObject>() where TObject : class
        {
            // Generate schema 
            JSchemaGenerator generator = new JSchemaGenerator();
            JSchema schema = generator.Generate(typeof(TObject));

            schema.AllowAdditionalProperties = false;

            return schema;
        }
    }
}

