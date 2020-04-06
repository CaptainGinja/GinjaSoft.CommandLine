namespace GinjaSoft.CommandLine.Tests
{
  using System;
  using Xunit;


  public class OneRequiredIntParam
  {
    private class Params
    {
      public int Foo { get; set; }
    }

    private int Main(string[] args, Action<Params> asserts)
    {
      var commandLine = new CommandLine(description: "Example command line with implicit command");
      commandLine.GetImplicitCommand<Params>()
        .SetHandler(@params => Handler(@params, asserts))
        .AddParameter(new Parameter<int>(name: "foo", description: "The foo int value"));

      return commandLine.Invoke(args);
    }

    [Fact]
    public void BadParameterType()
    {
      Assert.Throws<CommandLineException>(
        () => Main(new string[] { "--foo asdf" }, args => { })
      );
    }

    [Fact]
    public void Correct()
    {
      Main(new string[] { "--foo", "123" }, args =>
      {
        Assert.Equal(123, args.Foo);
      });
    }


    private int Handler(Params @params, Action<Params> asserts)
    {
      asserts(@params);
      return 1;
    }
  }
}
