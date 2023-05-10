using Chatter.Model;

namespace Chatter.UnitTests.Support;

public class PlayerFake : IPlayer
{
    public PlayerFake(string name, World? homeWorld = null)
    {
        Name = name;
        // ReSharper disable once StringLiteralTypo
        HomeWorld = homeWorld ?? new World(1, "Zalera", "Crystal");
    }

    public string Name { get; }

    public World HomeWorld { get; }

    public string FullName => $"{Name}@{HomeWorld.Name}";
}