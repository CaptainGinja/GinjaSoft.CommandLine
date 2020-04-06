namespace GinjaSoft.CommandLine
{
  using System;


  public class CommandLineException : Exception
  {
    public CommandLineException(string msg) : base(msg) { }
    public CommandLineException(string msg, Exception inner) : base(msg, inner) { }
  }
}