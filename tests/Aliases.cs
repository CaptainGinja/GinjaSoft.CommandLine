namespace GinjaSoft.CommandLine.Tests
{
  using System;
  using Xunit;


  public class Aliases
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
        .AddParameter(new Parameter<string>(name: "string-param", description: "").AddAlias("s"))
        .AddParameter(new Parameter<int>(name: "int-param", description: "").AddAlias("i"))
        .AddParameter(new Parameter<int>(name: "long-param", description: "").AddAlias("l"))
        .AddParameter(new Parameter<float>(name: "float-param", description: "").AddAlias("f"));

      return commandLine.Invoke(args);
    }

    [Fact]
    public void AllAliasesCorrect()
    {
      Main(new string[] { "-s", "foo", "-i", "42", "-l", "999", "-f", "3.14" }, @params =>
      {
        Assert.Equal("foo", @params.StringParam);
        Assert.Equal(42, @params.IntParam);
        Assert.Equal(999, @params.LongParam);
        Assert.Equal(float.Parse("3.14"), @params.FloatParam);
      });
    }

    [Fact]
    public void MixOfNamesAndAliases()
    {
      Main(new string[] { "--string-param", "foo", "-i", "42", "--long-param", "999", "-f", "3.14" }, @params =>
      {
        Assert.Equal("foo", @params.StringParam);
        Assert.Equal(42, @params.IntParam);
        Assert.Equal(999, @params.LongParam);
        Assert.Equal(float.Parse("3.14"), @params.FloatParam);
      });
    }


    private int Handler(Params @params, Action<Params> asserts)
    {
      asserts(@params);
      return 1;
    }
  }
}
