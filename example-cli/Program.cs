namespace CommandLineTestApp
{
  using System;
  using GinjaSoft.CommandLine;
  using GinjaSoft.Text;


  class Bar
  {
    private readonly string _first;
    private readonly string _second;


    public Bar(string s)
    {
      var tokens = s.Split('|');
      if(tokens.Length != 2) throw new InvalidOperationException("A Bar can only have two parts");
      _first = tokens[0];
      _second = tokens[1];
    }

    public override string ToString()
    {
      return $"{{ First={_first}, Second={_second} }}";
    }
  }


  class CommandAParams
  {
    public string Name { get; set; }
    public int Count { get; set; }
    public float Factor { get; set; }
    public DateTime DateTime { get; set; }
    public bool Debug { get; set; }
    public Bar Bar { get; set; }
  }

  class CommandBParams
  {
    public string Thing { get; set; }
    public TimeSpan Duration { get; set; }
  }


  class Program
  {
    static int Main(string[] args)
    {
      CommandLine commandLine = null;
      try {
        commandLine = new CommandLine(description: "Example command line app")
          .SetTraceFn(Console.WriteLine);

        var commandA = new Command<CommandAParams>(name: "cmd-a", description: "A command")
          .SetHandler(HandlerA)
          .AddParameter(new Parameter<string>(name: "name", description: "A name")
                        .AddAlias("n"))
          .AddParameter(new Parameter<int>(name: "count", description: "A count")
                        .AddAlias("c"))
          .AddParameter(new Parameter<float>(name: "factor", description: "A factor")
                        .AddAlias("f"))
          .AddParameter(new Parameter<DateTime>(name: "date-time", description: "A date-time")
                        .AddAlias("dt")
                        .SetOptional(DateTime.Now))
          .AddParameter(new Parameter<bool>(name: "debug", description: "A debug flag")
                        .AddAlias("d")
                        .SetOptional(false))
          .AddParameter(new Parameter<Bar>(name: "bar", description: "A bar")
                        .AddAlias("b"));

        var commandB = new Command<CommandBParams>(name: "cmd-b", description: "Another command")
                  .SetHandler(HandlerB)
                  .AddParameter(new Parameter<string>(name: "thing", description: "A thing")
                                .AddAlias("t"))
                  .AddParameter(new Parameter<TimeSpan>(name: "duration", description: "An amount of time")
                                .AddAlias("d"));

        commandLine.AddCommand(commandA);
        commandLine.AddCommand(commandB);

        return commandLine.Invoke(args);
      }
      catch(CommandLineSpecException e) {
        Console.WriteLine();
        Console.WriteLine(e.ToPrettyString(types: false, stackTrace: false));
        return 1;
      }
      catch(CommandLineException e) {
        Console.WriteLine();
        Console.WriteLine(e.ToPrettyString(types: false, stackTrace: false));
        Console.WriteLine(commandLine.Usage());
        return 1;
      }
      catch(Exception e) {
        Console.WriteLine();
        Console.WriteLine(e.ToPrettyString());
        return 1;
      }
    }


    private static int HandlerA(CommandAParams @params)
    {
      Console.WriteLine("HandlerA params:");
      Console.WriteLine($"Name={@params.Name}");
      Console.WriteLine($"Count={@params.Count}");
      Console.WriteLine($"Factor={@params.Factor}");
      Console.WriteLine($"DateTime={@params.DateTime}");
      Console.WriteLine($"Debug={@params.Debug}");
      Console.WriteLine($"Bar={@params.Bar}");
      return 0;
    }

    private static int HandlerB(CommandBParams @params)
    {
      Console.WriteLine("HandlerB params:");
      Console.WriteLine($"Thing={@params.Thing}");
      Console.WriteLine($"Duration={@params.Duration}");
      return 0;
    }
  }
}
