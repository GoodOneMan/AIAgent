using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIAgentLib
{
    public class OllamaClientWrapper
    {
        OllamaApiClient client = null;
        Chat chat = null;
        public OllamaClientWrapper() 
        {

            client = new OllamaApiClient("http://192.168.2.162:11434", "qwen2.5-coder:14b");
            //hf.co/empero-ai/Qwythos-9B-Claude-Mythos-5-1M-GGUF:Q8_0
            chat = new Chat(client);
        }

        public async Task<string> Response(string promt)
        {
            var response = await client.GetResponseAsync(promt);
            return response.Text;
        }


        public async Task<string> ResponseChat(string promt)
        {
            IAsyncEnumerator<string> enumerator = chat.SendAsync(promt).GetAsyncEnumerator();
            string response = "";
            try
            {
                // Перебираем токены вручную
                while (await enumerator.MoveNextAsync())
                {
                    response += enumerator.Current;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }
            return response;
        }

        public void OllamaList()
        {
            OllamaApiClient ollamaApi = new OllamaApiClient("http://192.168.2.162:11434", "qwen2.5-coder:14b");
        }
    }
}
