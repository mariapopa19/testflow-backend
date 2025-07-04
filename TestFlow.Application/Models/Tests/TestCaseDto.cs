﻿namespace TestFlow.Application.Models.Tests
{
    public class TestCaseDto
    {
        public string Type { get; set; } = null!;
        public string? CustomUrl { get; set; }
        public string Input { get; set; } = null!;
        public List<int>? ExpectedStatusCode { get; set; }
        public string? ExpectedResponse { get; set; } = null!;
    }

}
