namespace GinjaSoft.CommandLine.Tests
{
  using System;
  using Xunit;


  public class DateTimeParam
  {
    private class Params
    {
      public DateTime DateTime { get; set; }
    }

    private int Main(string[] args, Action<Params> asserts)
    {
      var commandLine = new CommandLine(description: "Example command line with implicit command");
      commandLine.GetImplicitCommand<Params>()
        .SetHandler(@params => Handler(@params, asserts))
        .AddParameter(new Parameter<DateTime>(name: "date-time", description: ""));

      return commandLine.Invoke(args);
    }

    [Fact]
    public void GoodDate()
    {
      Main(new string[] { "--date-time", "1971-07-29" }, @params => {
        Assert.Equal(new DateTime(1971, 7, 29), @params.DateTime);
      });
    }

    [Fact]
    public void GoodDateTime()
    {
      Main(new string[] { "--date-time", "1971-07-29T07:12:12" }, @params =>
      {
        Assert.Equal(new DateTime(1971, 7, 29, 7,12,12), @params.DateTime);
      });
    }


    private int Handler(Params @params, Action<Params> asserts)
    {
      asserts(@params);
      return 1;
    }
  }
}
