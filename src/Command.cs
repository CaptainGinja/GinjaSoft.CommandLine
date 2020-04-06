namespace GinjaSoft.CommandLine
{
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using System.Text;
  using System.Text.RegularExpressions;
  using GinjaSoft.Text;


  public abstract class Command
  {
    protected string _name;
    protected string _description;
    protected Dictionary<string, Parameter> _parameters;
    protected Dictionary<string, Parameter> _parametersByAlias;


    protected Command(string name, string description)
    {
      var lowerName = name.Trim().ToLower();
      const string pattern = @"^([a-z0-9]+[-]?)*[a-z0-9]+$";
      if(!Regex.IsMatch(lowerName, pattern))
        throw new CommandLineSpecException($"Bad command name '{name}'. Name must match {pattern}");

      _name = lowerName;
      _description = description;
      _parameters = new Dictionary<string, Parameter>();
      _parametersByAlias = new Dictionary<string, Parameter>();
    }


    public string Name => _name;
    public string Description => _description;
    public IReadOnlyDictionary<string, Parameter> Parameters => _parameters;


    public abstract int Invoke(string[] args);
  }


  public class Command<T> : Command where T : new()
  {
    private Func<T, int> _handler;


    public Command(string name, string description) : base(name, description) { }


    public Command<T> SetHandler(Func<T, int> handler)
    {
      _handler = handler;
      return this;
    }

    public Command<T> AddParameter<TParam>(Parameter<TParam> parameter)
    {
      if(_parameters.ContainsKey(parameter.Name)) throw new CommandLineSpecException("Duplicate parameter name");

      foreach(var alias in parameter.Aliases)
        if(_parametersByAlias.ContainsKey(alias)) throw new CommandLineSpecException("Duplicate alias");

      _parameters.Add(parameter.Name, parameter);
      foreach(var alias in parameter.Aliases) _parametersByAlias.Add(alias, parameter);

      return this;
    }

    public override int Invoke(string[] args)
    {
      var firstArg = args[0];
      if(firstArg == "-h" || firstArg == "--help") {
        Console.WriteLine(CommandLine.Usage(this));
        return 0;
      }

      if(args.Length % 2 != 0) throw new CommandLineException("Mismatch in parameter names and values");

      for(var n = 0; n < args.Length; n += 2) {
        var paramKey = args[n];
        Parameter param;

        const string paramNamePattern = @"^--(.+)$";
        var match = Regex.Match(paramKey, paramNamePattern);
        if(match.Success) {
          var paramName = match.Groups[1].Value;
          if(!_parameters.TryGetValue(paramName, out param))
            throw new CommandLineException($"Unknown parameter '{paramName}'");
        }
        else {
          const string aliasPattern = @"^-(.+)$";
          match = Regex.Match(paramKey, aliasPattern);
          if(!match.Success) throw new CommandLineException("Bad parameter name / alias");

          var alias = match.Groups[1].Value;
          if(!_parametersByAlias.TryGetValue(alias, out param))
            throw new CommandLineException($"Unknown parameter '{alias}'");
        }

        var paramValue = args[n + 1];
        param.Parse(paramValue);
        param.Validate();
      }

      var handlerParams = new T();

      foreach(var param in _parameters)
        param.Value.SetPropertyValue(handlerParams);

      return _handler(handlerParams);
    }
  }
}