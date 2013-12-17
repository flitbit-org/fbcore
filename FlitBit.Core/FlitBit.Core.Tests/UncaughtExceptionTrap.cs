using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FlitBit.Core.Tests
{
  public class UncaughtExceptionTrap
  {
    public static Object Error { get; private set; }

    [AssemblyInitialize]
    public static void Init()
    {
      AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
      {
        Error = args.ExceptionObject;
      };
    }

    public static void CheckUncaughtException()
    {
      var o = Error;
      Error = null;
      Assert.IsNull(o);
    }
  }
}
