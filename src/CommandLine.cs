namespace GinjaSoft.CommandLine
{
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.Text;
  using System.Reflection;
  using GinjaSoft.Text;


  public class CommandLine
  {
    private readonly string _description;
    private Command _implicitCommand;
    private Dictionary<string, Command> _commands;
    private bool _debug;
    private Action<string> _traceFn;


    public CommandLine(string description)
    {
      _description = description;
      _commands = new Dictionary<string, Command>();
      _debug = false;
    }


    public Command<T> GetImplicitCommand<T>() where T : new()
    {
      if(_implicitCommand != null) throw new CommandLineSpecException("Implicit command already exists");

      var command = new Command<T>("implicit", _description);
      _implicitCommand = command;
      return command;
    }

    public CommandLine AddCommand(Command command)
    {
      if(_implicitCommand != null)
        throw new CommandLineException("Can't add commands to a command line with an implicit command");

      if(_commands.ContainsKey(command.Name)) throw new CommandLineSpecException("Duplicate command name");

      _commands.Add(command.Name, command);
      return this;
    }

    public CommandLine SetTraceFn(Action<string> traceFn)
    {
      _traceFn = traceFn;
      return this;
    }

    public int Invoke(string[] args)
    {
      var newArgs = args.Select(s => s.ToLower()).Where(s => s != "--command-line-debug").ToArray();
      if(newArgs.Length < args.Length) _debug = true;

      if(_debug) {
        Trace(">> Trace");
        Trace("  Invoke(string[] args):");
        for(var n = 0; n < newArgs.Length; ++n) Trace($"  [{n}]: '{newArgs[n]}'");
        Trace("<< Trace");
      }

      if(newArgs.Length == 0) throw new CommandLineException("Expected command is missing");

      var firstArg = newArgs[0];
      if(firstArg == "-h" || firstArg == "--help") {
        Console.WriteLine(Usage());
        return 0;
      }

      if(_implicitCommand != null) return _implicitCommand.Invoke(newArgs);

      Command command;
      if(!_commands.TryGetValue(firstArg, out command))
        throw new CommandLineException($"Unknown command: {firstArg}");

      var remainingArgs = newArgs.Skip(1).Take(newArgs.Length - 1).ToArray();
      return command.Invoke(remainingArgs);
    }

    public static string Usage(Command command)
    {
      var builder = new StringBuilder();

      builder.AppendLine(UsageHeader($"{command.Name}"));

      builder.AppendLine(command.Description);
      builder.AppendLine();

      builder.AppendLine(UsageParameters(command, null));
      builder.AppendLine();

      builder.Append(UsageFooter());

      return builder.ToString();
    }

    public string Usage()
    {
      var builder = new StringBuilder();

      builder.Append(UsageHeader(_implicitCommand == null ? "<command>" : null));
      builder.AppendLine();

      if(_implicitCommand == null) {
        builder.AppendLine("Commands:");
        var commandNames = _commands.Keys.OrderBy(s => s).ToArray();
        builder.AppendLine(UsageCommands(commandNames));
        builder.AppendLine();
        foreach(var commandName in commandNames) {
          builder.AppendLine(UsageParameters(_commands[commandName], commandName));
          builder.AppendLine();
        }
      }
      else {
        builder.AppendLine(UsageParameters(_implicitCommand, null));
        builder.AppendLine();
      }

      builder.Append(UsageFooter());

      return builder.ToString();
    }


    private static string UsageHeader(string commandName)
    {
      var builder = new StringBuilder();

      builder.AppendLine();
      var programName = Assembly.GetEntryAssembly().GetName().Name;
      var tag = commandName == null ? "" : $"{commandName} ";

      builder.AppendLine($"Usage: {programName} {tag}-h|--help");
      builder.Append($"Usage: {programName} ");
      builder.AppendLine($"{tag}<parameters>");

      return builder.ToString();
    }

    private string UsageCommands(string[] commandNames)
    {
      var table = new StringTableBuilder();
      table.AddColumn("col1");
      table.AddColumn("col2");
      foreach(var commandName in commandNames) {
        var command = _commands[commandName];
        var row = table.AddRow();
        row.SetCell("col1", $"  {command.Name}");
        row.SetCell("col2", $"  {command.Description}");
      }

      return table.ToString();
    }

    private static string UsageParameters(Command command, string commandName)
    {
      var builder = new StringBuilder();
      builder.AppendLine(commandName == null ? "Parameters:" : $"{commandName} parameters:");

      var paramNames = command.Parameters.Keys.OrderBy(s => s).ToList();
      var table = new StringTableBuilder();
      table.AddColumn("col1");
      table.AddColumn("col2");
      table.AddColumn("col3");
      foreach(var paramName in paramNames) {
        var param = command.Parameters[paramName];
        var row = table.AddRow();
        row.SetCell("col1", $"  {ParamKey(param)}");
        row.SetCell("col2", param.Optional ? $"  ?:{param.DefaultValue}" : "");
        row.SetCell("col3", $"  {param.Description}");
      }
      builder.Append(table.ToString());

      return builder.ToString();
    }

    private static string ParamKey(Parameter param)
    {
      var builder = new StringBuilder();
      var count = 0;
      foreach(var alias in param.Aliases) {
        if(count > 0) builder.Append("|");
        builder.Append($"-{alias}");
        ++count;
      }
      if(count > 0) builder.Append("|");
      builder.Append($"--{param.Name}");
      return builder.ToString();
    }

    private static string UsageFooter()
    {
      var table = new StringTableBuilder().SetInnerCellColumnPadding(1);
      table.AddColumn("col1");
      table.AddColumn("col2");
      table.AddColumn("col3");
      var row = table.AddRow();
      row.SetCell("col1", "<parameters>");
      row.SetCell("col2", ":=");
      row.SetCell("col3", "<p1-key> <p1-value> ... <pN-key> <pN-value>");
      row = table.AddRow();
      row.SetCell("col1", "<p-key>");
      row.SetCell("col2", ":=");
      row.SetCell("col3", "-<p-alias>|--<p-name>");

      return table.ToString();
    }

    private void Trace(string msg)
    {
      if(_traceFn == null) return;
      _traceFn(msg);
    }
  }
}