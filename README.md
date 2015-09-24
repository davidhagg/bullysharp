# bullysharp
A C# implementation of the Bully leader election algorithm

###Usage

```c#
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
```
