using AutoMapper;
using TestFlow.Domain.Entities;
using TestFlow.Application.Models.Tests;
using TestFlow.Application.Models.Responses;

namespace TestFlow.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Example: TestCase to TestCaseDto  
            CreateMap<TestCase, TestCaseDto>();

            // TestResult -> TestResultDto  
            CreateMap<TestResult, TestResultDto>()
                .ForMember(dest => dest.TestCaseType, opt =>
                    opt.MapFrom(src =>
                        src.TestCase != null && !string.IsNullOrWhiteSpace(src.TestCase.Type)
                            ? src.TestCase.Type
                            : (GetFromDetails<string>(src.Details, "Type") ?? "Unknown")))
                .ForMember(dest => dest.Input, opt =>
                    opt.MapFrom(src =>
                        src.TestCase != null
                            ? src.TestCase.Input
                            : (GetFromDetails<string>(src.Details, "Input") ?? string.Empty)))
                .ForMember(dest => dest.ExpectedStatusCode, opt =>
                    opt.MapFrom(src =>
                        src.TestCase != null
                            ? src.TestCase.ExpectedStatusCode
                            : (GetFromDetails<List<int>>(src.Details, "ExpectedStatusCode") ?? new List<int>())))
                .ForMember(dest => dest.ActualStatusCode, opt =>
                    opt.MapFrom(src => GetActualStatusCode(src.Details)))
                .ForMember(dest => dest.Passed, opt =>
                    opt.MapFrom(src => src.Outcome == "Pass"))
                .ForMember(dest => dest.ResponseBody, opt =>
                    opt.MapFrom(src => GetResponseBody(src.Details)));

            // TestReport -> TestReportDto  
            CreateMap<TestReport, TestReportDto>()
                .ForMember(dest => dest.EndpointName, opt =>
                    opt.MapFrom(src => src.TestRun != null && src.TestRun.Endpoint != null
                        ? src.TestRun.Endpoint.Name
                        : string.Empty));

            CreateMap<TestRun, TestRunDto>()
                .ForMember(dest => dest.EndpointName, opt => opt.MapFrom(src => src.Endpoint.Name))
                .ForMember(dest => dest.Results, opt => opt.MapFrom(src => src.Results));
        }
        // Helper methods for mapping from Details JSON  
        private static T? GetFromDetails<T>(string details, string propertyName)
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(details);
                if (doc.RootElement.TryGetProperty(propertyName, out var prop))
                {
                    if (typeof(T) == typeof(string))
                        return (T)(object)prop.GetString()!;
                    if (typeof(T) == typeof(List<int>))
                    {
                        var list = new List<int>();
                        foreach (var item in prop.EnumerateArray())
                            list.Add(item.GetInt32());
                        return (T)(object)list;
                    }
                }
            }
            catch { }
            return default;
        }

        private static int GetActualStatusCode(string details)
        {
            try
            {
                return int.Parse(System.Text.Json.JsonDocument.Parse(details).RootElement.GetProperty("ActualStatusCode").GetRawText());
            }
            catch { return 0; }
        }

        private static string? GetResponseBody(string details)
        {
            try
            {
                return System.Text.Json.JsonDocument.Parse(details).RootElement.GetProperty("ResponseBody").GetString();
            }
            catch { return null; }
        }
    }
}
