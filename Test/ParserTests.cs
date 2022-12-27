#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace GrokParser.Test;
using System.Threading.Tasks;

public class ParserTests
{
    [Fact]
    public Task Parseing_Empty_Log_Should_Not_Throw()
    {
        // Arrange
        const string grokPattern = "";
        const string logs = "";
        var parser = new GrokBuilder(grokPattern).Build();

        // Act
        var grokResult = parser.Parse(logs);

        // Assert
        Assert.NotNull(grokResult);
        Assert.Empty(grokResult);
        return Task.CompletedTask;
    }

    [Fact]
    public Task Parse_Should_Return_Expected_Count()
    {
        // Arrange
        const string grokPattern = "%{MONTHDAY:month:int}-%{MONTHDAY:day:int}-%{MONTHDAY:year:int} %{TIME:@timestamp}";
        var sut = new GrokBuilder(grokPattern).Build();
        const string logs = @"25-26-3 8:49:36.48423254";
        // Act
        var grokResult = sut.Parse(logs);
        // Assert
        Assert.NotNull(grokResult);
        Assert.Equal(4, grokResult.Count);
        return Task.CompletedTask;
    }
    [Fact]
    public Task Parse_Should_Return_Expected_Result()
    {
        // Arrange
        const string grokPattern = "%{MONTHDAY:month}-%{MONTHDAY:day}-%{MONTHDAY:year} %{TIME:@timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}";
        var sut = new GrokBuilder(grokPattern).Build();
        const string logs = @"04-29-02 1:33:04,94034419;ydza3IBYKHf9Pz04Oz5;WAR;mwBlI3gRzliyZI;The quick brown fox jumps over the lazy dog";
        //this.output.WriteLine(sut.GetRegex());
        // Act
        var grokResult = sut.Parse(logs);
        // Assert
        Assert.NotNull(grokResult);
        Assert.Equal(8, grokResult.Count);
        Assert.Equal("04", grokResult["month"]);
        Assert.Equal("29", grokResult["day"]);
        Assert.Equal("02", grokResult["year"]);
        Assert.Equal("1:33:04,94034419", grokResult["@timestamp"]);
        Assert.Equal("ydza3IBYKHf9Pz04Oz5", grokResult["id"]);
        Assert.Equal("WAR", grokResult["loglevel"]);
        Assert.Equal("mwBlI3gRzliyZI", grokResult["func"]);
        Assert.Equal("The quick brown fox jumps over the lazy dog", grokResult["msg"]);
        return Task.CompletedTask;
    }
    [Fact]
    public Task ParseWithType_Should_Return_Expected_Result()
    {
        // Arrange
        const string grokPattern = "%{MONTHDAY:month:int}-%{MONTHDAY:day:int}-%{MONTHDAY:year:int} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}";
        var sut = new GrokBuilder(grokPattern).Build();
        const string logs = @"04-29-02 1:33:04,94034419;ydza3IBYKHf9Pz04Oz5;WAR;mwBlI3gRzliyZI;The quick brown fox jumps over the lazy dog";
        // Act
        var grokResult = sut.Parse(logs);
        // Assert
        Assert.NotNull(grokResult);
        Assert.Equal(8, grokResult.Count);
        Assert.Equal(4, grokResult["month"]);
        Assert.Equal(29, grokResult["day"]);
        Assert.Equal(2, grokResult["year"]);
        Assert.Equal("1:33:04,94034419", grokResult["timestamp"]);
        Assert.Equal("ydza3IBYKHf9Pz04Oz5", grokResult["id"]);
        Assert.Equal("WAR", grokResult["loglevel"]);
        Assert.Equal("mwBlI3gRzliyZI", grokResult["func"]);
        Assert.Equal("The quick brown fox jumps over the lazy dog", grokResult["msg"]);
        return Task.CompletedTask;
    }
    [Fact]
    public Task ParseWithWrongType_Should_Return_Expected_Result()
    {
        // Arrange
        const string grokPattern = "%{MONTHDAY:month:datetime}-%{MONTHDAY:day:bool}-%{MONTHDAY:year:datetime} %{TIME:timestamp};%{WORD:id};%{LOGLEVEL:loglevel};%{WORD:func};%{GREEDYDATA:msg}";
        var sut = new GrokBuilder(grokPattern).Build();
        const string logs = @"04-29-02 1:33:04,94034419;ydza3IBYKHf9Pz04Oz5;WAR;mwBlI3gRzliyZI;The quick brown fox jumps over the lazy dog";
        // Act
        var grokResult = sut.Parse(logs);
        // Assert
        Assert.NotNull(grokResult);
        Assert.Equal(8, grokResult.Count);
        Assert.Equal("04", grokResult["month"]);
        Assert.Equal("29", grokResult["day"]);
        Assert.Equal("02", grokResult["year"]);
        Assert.Equal("1:33:04,94034419", grokResult["timestamp"]);
        Assert.Equal("ydza3IBYKHf9Pz04Oz5", grokResult["id"]);
        Assert.Equal("WAR", grokResult["loglevel"]);
        Assert.Equal("mwBlI3gRzliyZI", grokResult["func"]);
        Assert.Equal("The quick brown fox jumps over the lazy dog", grokResult["msg"]);
        return Task.CompletedTask;
    }
}
