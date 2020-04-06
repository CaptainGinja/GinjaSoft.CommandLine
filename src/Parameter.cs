namespace GinjaSoft.CommandLine
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Text.RegularExpressions;


  public abstract class Parameter
  {
    protected readonly string _name;
    protected readonly string _description;
    protected readonly string _propertyName;
    protected List<string> _aliases;
    protected string _stringValue;
    protected bool _wasParsed;
    protected bool _optional;


    protected Parameter(string name, string description)
    {
      var lowerName = name.Trim().ToLower();
      const string pattern = @"^([a-z0-9]+[-]?)*[a-z0-9]+$";
      if(!Regex.IsMatch(lowerName, pattern))
        throw new CommandLineSpecException($"Bad parameter name '{name}'. Name must match {pattern}");

      _name = lowerName;
      _description = description;
      _aliases = new List<string>();
      _wasParsed = false;

      var tokens = _name.Split('-');
      var builder = new StringBuilder();
      foreach(var token in tokens) builder.Append(UpperCaseFirstLetter(token));
      _propertyName = builder.ToString();
    }


    public string Name => _name;
    public string Description => _description;
    public string PropertyName => _propertyName;
    public bool WasParsed => _wasParsed;
    public bool Required => !_optional;
    public bool Optional => _optional;
    public object DefaultValue => GetDefaultValue();
    public ICollection<string> Aliases => _aliases;


    public void Parse(string stringValue)
    {
      _stringValue = stringValue;
      Parse();
    }

    public abstract void Validate();

    public abstract void SetPropertyValue(object obj);


    protected abstract void Parse();

    protected abstract object GetDefaultValue();


    private string UpperCaseFirstLetter(string s)
    {
      var array = s.ToCharArray();
      array[0] = char.ToUpper(array[0]);
      return new string(array);
    }
  }


  public class Parameter<T> : Parameter
  {
    private T _defaultValue;
    private Func<string, T> _factoryFn;
    private Func<T, bool> _validateFn;
    private T _value;


    public Parameter(string name, string description) : base(name, description)
    {
      _optional = false;
      _defaultValue = default(T);
    }

    public new T DefaultValue => _defaultValue;


    public Parameter<T> SetOptional(T defaultValue)
    {
      _optional = true;
      _defaultValue = defaultValue;
      return this;
    }

    public Parameter<T> SetFactoryFn(Func<string, T> factoryFn)
    {
      _factoryFn = factoryFn;
      return this;
    }

    public Parameter<T> SetValidateFn(Func<T, bool> validateFn)
    {
      _validateFn = validateFn;
      return this;
    }

    public Parameter<T> AddAlias(string alias)
    {
      var lowerAlias = alias.Trim().ToLower();
      const string pattern = @"^[a-z0-9]+$";
      if(!Regex.IsMatch(lowerAlias, pattern)) {
        var msg = $"Bad parameter alias '{alias}'. Alias must match {pattern}";
        throw new CommandLineSpecException($"Parameter '{_name}': {msg}");
      }

      _aliases.Add(alias);
      return this;
    }

    public override void Validate()
    {
      if(_validateFn == null) return;
      try {
        if(!_validateFn(_value))
          throw new CommandLineException($"Parameter '{_name}': Validate function failed");
      }
      catch(Exception e) {
        throw new CommandLineException($"Parameter '{_name}': Exception in validate function", e);
      }
    }

    public override void SetPropertyValue(object obj)
    {
      var property = obj.GetType().GetProperty(_propertyName);
      if(property == null) {
        var msg = $"Property '{_propertyName}' not found on handler params object";
        throw new CommandLineException($"Parameter '{_name}': {msg}");
      }

      var value = _value;
      if(!_wasParsed) {
        if(!_optional) throw new CommandLineException($"Missing required parameter '{_name}'");
        value = _defaultValue;
      }

      try {
        property.SetValue(obj, value);
      }
      catch(Exception e) {
        var msg = $"Exception when setting property '{_propertyName}' on handler params object";
        throw new CommandLineException($"Parameter '{_name}': {msg}", e);
      }
    }


    protected override void Parse()
    {
      try {
        if(_wasParsed) throw new CommandLineException($"Repeated parameter");
        _wasParsed = true;

        if(_factoryFn != null) {
          _value = ParseViaFactoryFunction();
          return;
        }

        var obj = TryParseBasicTypes();
        if(obj != null) {
          _value = (T)obj;
          return;
        }

        obj = TryParseEnum();
        if(obj != null) {
          _value = (T)obj;
          return;
        }

        _value = TryParseViaConstructor();
      }
      catch(CommandLineException e) {
        throw new CommandLineException($"Parameter '{_name}', value='{_stringValue}': {e.Message}", e.InnerException);
      }
      catch(Exception e) {
        throw new CommandLineException($"Parameter '{_name}': Unexpected exception", e);
      }
    }

    protected override object GetDefaultValue()
    {
      return _defaultValue;
    }


    private T ParseViaFactoryFunction()
    {
      try {
        return _factoryFn(_stringValue);
      }
      catch(Exception e) {
        throw new CommandLineException($"Exception in factory function", e);
      }
    }

    private object TryParseBasicTypes()
    {
      try {
        if(typeof(T) == typeof(string)) return _stringValue;

        switch(default(T)) {
          case sbyte x: return sbyte.Parse(_stringValue);
          case byte x: return byte.Parse(_stringValue);
          case short x: return short.Parse(_stringValue);
          case ushort x: return ushort.Parse(_stringValue);
          case int x: return int.Parse(_stringValue);
          case uint x: return uint.Parse(_stringValue);
          case long x: return long.Parse(_stringValue);
          case ulong x: return ulong.Parse(_stringValue);
          case float x: return float.Parse(_stringValue);
          case double x: return double.Parse(_stringValue);
          case decimal x: return decimal.Parse(_stringValue);
          case bool x: return ParseBool(_stringValue);
          case DateTime x: return DateTime.Parse(_stringValue);
          case TimeSpan x: return TimeSpan.Parse(_stringValue);
          case string x: return _stringValue;
          default:
            return null;
        }
      }
      catch(Exception e) {
        throw new CommandLineException("Exception trying to parse basic types", e);
      }
    }

    private object TryParseEnum()
    {
      try {
        if(typeof(T).IsEnum) return Enum.Parse(typeof(T), _stringValue);
        return null;
      }
      catch(Exception e) {
        throw new CommandLineException("Exception trying to parse Enum", e);
      }
    }

    private T TryParseViaConstructor()
    {
      var type = typeof(T);
      var ctor = type.GetConstructor(new[] { typeof(string) });
      if(ctor == null)
        throw new CommandLineException($"Type does not have a string param constructor");

      try {
        return (T)ctor.Invoke(new object[] { _stringValue });
      }
      catch(Exception e) {
        throw new CommandLineException($"Exception in string param constructor", e);
      }
    }

    private bool ParseBool(string s)
    {
      switch(s.ToLower()) {
        case "yes": return true;
        case "y": return true;
        case "true": return true;
        case "t": return true;
        case "1": return true;
        case "no": return false;
        case "n": return false;
        case "false": return false;
        case "f": return false;
        case "0": return false;
        default:
          throw new FormatException("Unrecognized bool format");
      }
    }
  }
}