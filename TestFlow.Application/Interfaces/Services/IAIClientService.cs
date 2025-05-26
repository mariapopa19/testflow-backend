namespace TestFlow.Application.Interfaces.Services
{
    public interface IAIClientService
    {
        Task<string> GetPromptResponseAsync(string prompt);
    }
}
