namespace GinjaSoft.CommandLine.Tests
{
  using System;
  using Xunit;


  public class SubCommands
  {
    private class Command1Params
    {
      public string Foo { get; set; }
    }

    private class Command2Params
    {
      public int Bar { get; set; }
    }

    private int Main(string[] args, Action<dynamic> asserts)
    {
      var commandLine = new CommandLine(description: "Example two command command line");
      
      var command1 =
        new Command<Command1Params>(name: "cmd1", description: "Command 1")
        .AddParameter(new Parameter<string>(name: "foo", description: "The foo string value"))
        .SetHandler(@params => Handler<Command1Params>(@params, asserts));

      var command2 =
        new Command<Command2Params>(name: "cmd2", description: "Command 2")
        .AddParameter(new Parameter<int>(name: "bar", description: "The bar int value"))
        .SetHandler(@params => Handler<Command2Params>(@params, asserts));

      commandLine.AddCommand(command1);
      commandLine.AddCommand(command2);

      return commandLine.Invoke(args);
    }

    [Fact]
    public void NoCommandLine()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { }, x => { })
      );
    }

    [Fact]
    public void EmptyStringOnCommandLine()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "" }, x => { })
      );
    }

    [Fact]
    public void UnknownCommand()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "asdf" }, x => { })
      );
    }

    [Fact]
    public void MissingCommand()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "--foo" }, x => { })
      );
    }

    [Fact]
    public void UnknownParameter()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "cmd1", "--baz", "sdfsd" }, x => { })
      );
    }

    [Fact]
    public void BadCommandCommandLine()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "cmd1", "asdf" }, x => { })
      );
    }

    [Fact]
    public void Command1Correct()
    {
      Main(new string[] { "cmd1", "--foo", "oof" }, @params =>
      {
        Assert.Equal(typeof(Command1Params), @params.GetType());
        Assert.Equal("oof", @params.Foo);
      });
    }

    [Fact]
    public void Command2Correct()
    {
      Main(new string[] { "cmd2", "--bar", "42" }, @params =>
      {
        Assert.Equal(typeof(Command2Params), @params.GetType());
        Assert.Equal(42, @params.Bar);
      });
    }


    private int Handler<T>(T @params, Action<dynamic> asserts)
    {
      asserts(@params);
      return 1;
    }
  }
}
