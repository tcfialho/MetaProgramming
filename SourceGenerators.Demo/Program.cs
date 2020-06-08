using SourceGenerators.Domain.Entities;
using System;

namespace SourceGenerators.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var account = new Account() { Id = 123, Name = "Account Test" };

            Console.WriteLine(account.ToString());
        }
    }
}
