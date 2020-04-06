namespace GinjaSoft.CommandLine.Tests
{
  using System;
  using Xunit;


  public class BoolParam
  {
    private class Params
    {
      public bool BoolParam { get; set; }
    }

    private int Main(string[] args, Action<Params> asserts)
    {
      var commandLine = new CommandLine(description: "Example command line with implicit command");
      commandLine.GetImplicitCommand<Params>()
        .SetHandler(@params => Handler(@params, asserts))
        .AddParameter(new Parameter<bool>(name: "bool-param", description: ""));

      return commandLine.Invoke(args);
    }

    [Fact]
    public void BoolYes()
    {
      Main(new string[] { "--bool-param", "yes" }, @params => { Assert.True(@params.BoolParam); });
    }

    [Fact]
    public void BoolY()
    {
      Main(new string[] { "--bool-param", "y" }, @params => { Assert.True(@params.BoolParam); });
    }

    [Fact]
    public void BoolTrue()
    {
      Main(new string[] { "--bool-param", "true" }, @params => { Assert.True(@params.BoolParam); });
    }

    [Fact]
    public void BoolT()
    {
      Main(new string[] { "--bool-param", "t" }, @params => { Assert.True(@params.BoolParam); });
    }

    [Fact]
    public void Bool1()
    {
      Main(new string[] { "--bool-param", "1" }, @params => { Assert.True(@params.BoolParam); });
    }

    [Fact]
    public void BoolNo()
    {
      Main(new string[] { "--bool-param", "no" }, @params => { Assert.False(@params.BoolParam); });
    }

    [Fact]
    public void BoolN()
    {
      Main(new string[] { "--bool-param", "n" }, @params => { Assert.False(@params.BoolParam); });
    }

    [Fact]
    public void BoolFalse()
    {
      Main(new string[] { "--bool-param", "false" }, @params => { Assert.False(@params.BoolParam); });
    }

    [Fact]
    public void BoolF()
    {
      Main(new string[] { "--bool-param", "f" }, @params => { Assert.False(@params.BoolParam); });
    }

    [Fact]
    public void Bool0()
    {
      Main(new string[] { "--bool-param", "0" }, @params => { Assert.False(@params.BoolParam); });
    }


    private int Handler(Params @params, Action<Params> asserts)
    {
      asserts(@params);
      return 1;
    }
  }
}
