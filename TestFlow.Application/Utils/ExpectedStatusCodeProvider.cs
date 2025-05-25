namespace TestFlow.Application.Utils
{
    public static class ExpectedStatusCodeProvider
    {
        public enum TestType
        {
            Functional,
            Validation,
            Fuzzy
        }

        public enum HttpMethod
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        public enum ExpectedStatus
        {
            Success,
            ClientError,
            ServerError
        }

        public static List<int> GetExpectedStatusCodes(string testType, string httpMethod, string expectedStatus)
        {
            if (!Enum.TryParse<TestType>(testType, true, out var testTypeEnum))
                throw new ArgumentException($"Invalid TestType: {testType}");

            if (!Enum.TryParse<HttpMethod>(httpMethod, true, out var httpMethodEnum))
                throw new ArgumentException($"Invalid HttpMethod: {httpMethod}");

            if (!Enum.TryParse<ExpectedStatus>(expectedStatus, true, out var expectedStatusEnum))
                throw new ArgumentException($"Invalid ExpectedStatus: {expectedStatus}");

            return GetExpectedStatusCodes(testTypeEnum, httpMethodEnum, expectedStatusEnum);
        }

        public static List<int> GetExpectedStatusCodes(TestType testType, HttpMethod httpMethod, ExpectedStatus expectedStatus)
        {

            return (testType, httpMethod, expectedStatus) switch
            {
                (TestType.Functional, HttpMethod.GET, ExpectedStatus.Success) => new List<int> { 200 },
                (TestType.Functional, HttpMethod.POST, ExpectedStatus.Success) => new List<int> { 200, 201 },
                (TestType.Functional, HttpMethod.PUT, ExpectedStatus.Success) => new List<int> { 200, 204 },
                (TestType.Functional, HttpMethod.DELETE, ExpectedStatus.Success) => new List<int> { 200, 204 },

                (TestType.Validation, HttpMethod.POST, ExpectedStatus.Success) => new List<int> { 200, 201 },
                (TestType.Validation, HttpMethod.POST, ExpectedStatus.ClientError) => new List<int> { 400, 404 },
                (TestType.Validation, HttpMethod.PUT, ExpectedStatus.Success) => new List<int> { 200, 201 },
                (TestType.Validation, HttpMethod.PUT, ExpectedStatus.ClientError) => new List<int> { 400, 404 },
                (TestType.Validation, HttpMethod.DELETE, ExpectedStatus.ClientError) => new List<int> { 400 },
                (TestType.Validation, HttpMethod.GET, ExpectedStatus.ClientError) => new List<int> { 400 },
                (TestType.Validation, HttpMethod.GET, ExpectedStatus.Success) => new List<int> { 200, 201 },

                (TestType.Fuzzy, HttpMethod.POST, ExpectedStatus.ClientError) => new List<int> { 400, 422 },
                (TestType.Fuzzy, HttpMethod.PUT, ExpectedStatus.ClientError) => new List<int> { 400, 422 },
                (TestType.Fuzzy, HttpMethod.GET, ExpectedStatus.Success) => new List<int> { 200, 201 },
                (TestType.Fuzzy, HttpMethod.POST, ExpectedStatus.Success) => new List<int> { 200, 201 },

                _ => new List<int> { 200 } 
            };
        }
    }
}
