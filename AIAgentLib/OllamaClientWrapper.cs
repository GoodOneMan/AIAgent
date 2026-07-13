using Microsoft.Extensions.AI;
using OllamaSharp;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIAgentLib
{
    public class OllamaClientWrapper
    {
        private readonly OllamaApiClient _client;
        private readonly Chat _chat;

        public OllamaClientWrapper(string url, string model)
        {
            _client = new OllamaApiClient(url, model);
            _chat = new Chat(_client);
        }

        public async Task<string> GetResponseAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetResponseAsync(prompt, null, cancellationToken).ConfigureAwait(false);
            return response?.Text ?? string.Empty;
        }

        public async Task<string> GetStreamingResponseAsync(
            string prompt,
            Action<string> onTokenReceived,
            CancellationToken cancellationToken = default)
        {
            var fullResponse = new StringBuilder();
            var enumerator = _chat.SendAsync(prompt, cancellationToken).GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var token = enumerator.Current;
                    fullResponse.Append(token);
                    onTokenReceived?.Invoke(token);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
            return fullResponse.ToString();
        }
    }
}