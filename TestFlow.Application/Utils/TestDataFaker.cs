using System.Text.Json;
using Bogus;

namespace TestFlow.Application.Utils
{
    public static class TestDataFaker
    {
        public static Dictionary<string, object?> GenerateValidInput(JsonElement model)
        {
            var faker = new Faker();
            var input = new Dictionary<string, object?>();

            foreach (var prop in model.EnumerateObject())
            {
                object? value = prop.Value.ValueKind switch
                {
                    JsonValueKind.Number => faker.Random.Int(1, 100),
                    JsonValueKind.String when prop.Name.ToLower().Contains("email") => faker.Internet.Email(),
                    JsonValueKind.String => faker.Lorem.Word(),
                    JsonValueKind.True or JsonValueKind.False => faker.Random.Bool(),
                    JsonValueKind.Array => new[] { faker.Lorem.Word(), faker.Lorem.Word() },
                    JsonValueKind.Object => new Dictionary<string, object?> { { "nested", faker.Lorem.Word() } },
                    _ => null
                };

                input[prop.Name] = value;
            }

            return input;
        }
    }
}
