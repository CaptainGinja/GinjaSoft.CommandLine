namespace GinjaSoft.CommandLine
{
  using System;


  public class CommandLineSpecException : Exception
  {
    public CommandLineSpecException(string msg) : base(msg) { }
    public CommandLineSpecException(string msg, Exception inner) : base(msg, inner) { }
  }
}