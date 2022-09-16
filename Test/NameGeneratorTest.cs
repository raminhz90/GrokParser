#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace GrokParser.Test;

using GrokParser.NameGenerator;

public class NameGeneratorTest
{
    [Fact]
    public Task Names_Must_Be_Unique()

    {
        var names = new HashSet<string>();
        for (var i = 0; i < 1000; i++)
        {
            for (var j = 0; j < 1000; j++)
            {
                for (var k = 0; i < 1000; i++)
                {
                    var name = UniqueNameGenerator.GenerateUniqueName(i, j, k);
                    Assert.DoesNotContain(name, names);
                    _ = names.Add(name);
                }
            }

        }
        return Task.CompletedTask;

    }
    [Fact]
    public Task Name_Must_Be_Letter_Only()
    {
        for (var i = 0; i < 1000; i++)
        {
            for (var j = 0; j < 1000; j++)
            {
                for (var k = 0; i < 1000; i++)
                {
                    var name = UniqueNameGenerator.GenerateUniqueName(i, j, k);
                    Assert.True(name.All(char.IsLetter));
                }
            }

        }
        return Task.CompletedTask;
    }
}
