using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests
{

  [TestClass]
  public class DataGeneratorTests
  {
    [TestMethod]
    public void DataGenerator_CanGetStrings()
    {
      const int Length = 2000;
      var gen = new DataGenerator();
      for (int i = 0; i < 100; i++)
      {
        var s = gen.GetString(Length);
        Assert.IsNotNull(s);
        Assert.AreEqual(Length, s.Length);
      }
    }

    [TestMethod]
    public void DataGenerator_GetByte()
    {
      var datagen = new DataGenerator();
      for (int i = 0; i < 100; i++)
      {
        var value = datagen.GetByte();
        Assert.IsNotNull(value);
      }
    }

    [TestMethod]
    public void DataGenerator_GetBoolean()
    {
      var datagen = new DataGenerator();
      for (int i = 0; i < 100; i++)
      {
        var value = datagen.GetBoolean();
        Assert.IsNotNull(value);
      }
    }

    [TestMethod]
    public void DataGenerator_GetDateTime()
    {
      var datagen = new DataGenerator();
      for (int i = 0; i < 1000; i++)
      {
        var value = datagen.GetDateTime();
        Assert.IsNotNull(value);
      }
    }

    [TestMethod]
    public void DataGenerator_GetDateTimeOffset()
    {
      var datagen = new DataGenerator();
      for (int i = 0; i < 1000000; i++)
      {
        var value = datagen.GetDateTimeOffset();
        Assert.IsNotNull(value);
      }
    }
  }
}