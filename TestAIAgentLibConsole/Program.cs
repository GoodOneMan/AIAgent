using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAIAgentLibConsole
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            AIAgentLib.EntryPoint entryPoint = new AIAgentLib.EntryPoint();
            var result = await entryPoint.Start();
            Console.WriteLine(result);
        }
    }
}
