using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIAgentLib
{
    public class EntryPoint
    {
        OllamaApiClient client = null;
        Chat chat = null;
        public EntryPoint() 
        {
            client = new OllamaApiClient("http://192.168.2.162:11434", "qwen2.5-coder:14b");
            chat = new Chat(client);
        }

        public async Task<string> Start()
        {
            IChatClient chatClient = new OllamaApiClient("http://192.168.2.162:11434", "qwen2.5-coder:14b");

            var response = await chatClient.GetResponseAsync("как дела");

            return response.Text;
        }

        public async Task<string> Response(string promt)
        {
            IChatClient chatClient = new OllamaApiClient("http://192.168.2.162:11434", "qwen2.5-coder:14b");

            var response = await chatClient.GetResponseAsync(promt);

            return response.Text;
        }

        //public Task<string> ResponseChat(string promt)
        //{
        //    var response = chat.SendAsync(promt);
        //    return response.StreamToEndAsync();
        //}

        public async Task<string> ResponseChat(string promt)
        {
            IAsyncEnumerator<string> enumerator = chat.SendAsync(promt).GetAsyncEnumerator();
            string response = "";
            try
            {
                // Перебираем токены вручную
                while (await enumerator.MoveNextAsync())
                {
                    //Console.Write(enumerator.Current);
                    response += enumerator.Current;
                }
            }
            finally
            {
                // Освобождаем ресурсы перечислителя
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
