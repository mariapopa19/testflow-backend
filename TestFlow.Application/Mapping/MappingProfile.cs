using AutoMapper;
using TestFlow.Domain.Entities;
using TestFlow.Application.Models.Tests;
using TestFlow.Application.Models.Responses;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestFlow.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Example: TestCase to TestCaseDto
            CreateMap<TestCase, TestCaseDto>();
            CreateMap<TestResult, TestResultDto>()
                .ForMember(dest => dest.TestCaseType, opt => opt.MapFrom(src => src.TestCase != null ? src.TestCase.Type : "Unknown"))
                .ForMember(dest => dest.Input, opt => opt.MapFrom(src => src.TestCase != null ? src.TestCase.Input : string.Empty))
                .ForMember(dest => dest.ExpectedStatusCode, opt => opt.MapFrom(src => src.TestCase != null ? src.TestCase.ExpectedStatusCode : new List<int>()));
            CreateMap<TestReport, TestReportDto>();
            CreateMap<TestRun, TestRunDto>()
                .ForMember(dest => dest.EndpointName, opt => opt.MapFrom(src => src.Endpoint.Name))
                .ForMember(dest => dest.Results, opt => opt.MapFrom(src => src.Results));

        }
    }
}
