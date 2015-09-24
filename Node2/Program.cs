using System;
using BullySharp.Core;
using BullySharp.Core.Logging;

namespace Node2
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            var bully = new Bully(logger);
            bully.LeaderChanged += Bully_LeaderChanged;

            bully.Start();

            Console.ReadLine();

            bully.Stop();
        }

        private static void Bully_LeaderChanged(object sender, int e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"NEW LEADER: {e}");
            Console.ResetColor();
        }
    }
}
