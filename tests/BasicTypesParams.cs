namespace GinjaSoft.CommandLine.Tests
{
  using System;
  using Xunit;


  public class BasicTypesParams
  {
    private class Params
    {
      public string StringParam { get; set; }
      public int IntParam { get; set; }
      public long LongParam { get; set; }
      public float FloatParam { get; set; }
    }

    private int Main(string[] args, Action<Params> asserts)
    {
      var commandLine = new CommandLine(description: "Example command line with implicit command");
      commandLine.GetImplicitCommand<Params>()
        .SetHandler(@params => Handler(@params, asserts))
        .AddParameter(new Parameter<string>(name: "string-param", description: ""))
        .AddParameter(new Parameter<int>(name: "int-param", description: ""))
        .AddParameter(new Parameter<int>(name: "long-param", description: ""))
        .AddParameter(new Parameter<float>(name: "float-param", description: ""));

      return commandLine.Invoke(args);
    }

    [Fact]
    public void Correct()
    {
      Main(new string[]
      {
        "--string-param", "foo", "--int-param", "42", "--long-param", "999", "--float-param", "3.14"
      },
      @params =>
      {
        Assert.Equal("foo", @params.StringParam);
        Assert.Equal(42, @params.IntParam);
        Assert.Equal(999, @params.LongParam);
        Assert.Equal(float.Parse("3.14"), @params.FloatParam);
      });
    }

    [Fact]
    public void SpecificLiterals()
    {
      Main(new string[]
      {
        "--string-param", "foo", "--int-param", "0032", "--long-param", "999", "--float-param", "0.3e-2"
      },
      @params =>
      {
        Assert.Equal("foo", @params.StringParam);
        Assert.Equal(32, @params.IntParam);
        Assert.Equal(999, @params.LongParam);
        Assert.Equal(float.Parse("0.3e-2"), @params.FloatParam);
      });
    }


    private int Handler(Params @params, Action<Params> asserts)
    {
      asserts(@params);
      return 1;
    }
  }
}
