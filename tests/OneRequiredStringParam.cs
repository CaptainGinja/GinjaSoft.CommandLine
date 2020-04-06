namespace GinjaSoft.CommandLine.Tests
{
  using System;
  using Xunit;


  public class OneRequiredStringParam
  {
    private class Params
    {
      public string Foo { get; set; }
    }

    private int Main(string[] args, Action<Params> asserts)
    {
      var commandLine = new CommandLine(description: "Example command line with implicit command");
      commandLine.GetImplicitCommand<Params>()
        .SetHandler(@params => Handler(@params, asserts))
        .AddParameter(new Parameter<string>(name: "foo", description: "The foo string value"));

      return commandLine.Invoke(args);
    }

    [Fact]
    public void NoCommandLine()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "" }, args => { })
      );
    }

    [Fact]
    public void UnknownToken()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "asdf" }, args => { })
      );
    }

    [Fact]
    public void UnknownParameter()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "--zoo" }, args => { })
      );
    }

    [Fact]
    public void MissingParameterValue()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "--foo" }, args => { })
      );
    }

    [Fact]
    public void Correct()
    {
      Main(new string[] { "--foo", "oof" }, args =>
      {
        Assert.Equal("oof", args.Foo);
      });
    }


    private int Handler(Params @params, Action<Params> asserts)
    {
      asserts(@params);
      return 1;
    }
  }
}
