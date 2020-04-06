namespace GinjaSoft.CommandLine.Tests
{
  using System;
  using Xunit;
  using Xunit.Abstractions;


  public class OneOptionalStringParam
  {
    private readonly ITestOutputHelper _output;


    public OneOptionalStringParam(ITestOutputHelper output)
    {
      _output = output;
    }


    private class Params
    {
      public string Foo { get; set; }
    }


    [Fact]
    public void NoCommandLine()
    {
      Main(new string[] { }, args =>
      {
        Assert.Equal("oof", args.Foo);
      });
    }

    [Fact]
    public void UnknownToken()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "asdf" }, args => { })
      );
    }

    [Fact]
    public void UnknownParam()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "--bar" }, args => { })
      );
    }

    [Fact]
    public void MissingParamValue()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "--foo" }, args => { })
      );
    }

    [Fact]
    public void Correct()
    {
      Main(new string[] { "--foo", "asdf" }, args =>
      {
        Assert.Equal("asdf", args.Foo);
      });
    }


    private int Main(string[] args, Action<Params> asserts)
    {
      var commandLine = new CommandLine(description: "Example command line with implicit command");
      commandLine.GetImplicitCommand<Params>()
        .SetHandler(@params => Handler(@params, asserts))
        .AddParameter(new Parameter<string>(name: "foo", description: "The foo string value")
                      .SetOptional("oof"));

      return commandLine.Invoke(args);
    }

    private int Handler(Params @params, Action<Params> asserts)
    {
      asserts(@params);
      return 1;
    }
  }
}
