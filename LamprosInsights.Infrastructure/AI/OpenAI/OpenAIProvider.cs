using LamprosInsights.Application.Features.Analytics.Abstractions;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Infrastructure.AI.OpenAI
{
    public class OpenAIProvider : IAIProvider
    {
        private readonly ChatClient _chatClient;

        public OpenAIProvider(
            IConfiguration configuration)
        {
            var apiKey =
                configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException(
                    "OpenAI API key not configured.");

            _chatClient = new ChatClient(
                model: "gpt-4.1-mini",
                apiKey: apiKey);
        }

        public async Task<string> GenerateSqlAsync(
            string prompt,
            CancellationToken cancellationToken = default)
        {
            var completion = await _chatClient.CompleteChatAsync(
                [
                    new UserChatMessage(prompt)
                ],
                cancellationToken: cancellationToken);

            return completion.Value.Content[0].Text;
        }
    }
}
