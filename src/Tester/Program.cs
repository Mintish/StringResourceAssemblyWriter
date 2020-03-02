using System;
using StringResources;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Llama: {Resource.llama}");
            var llamaAttr = typeof(Resource).GetProperty("llama").GetCustomAttributes(true)[0];
            Console.WriteLine(llamaAttr);
            Console.WriteLine($"Wanda: {Resource.wanda}");
        }
    }
}
