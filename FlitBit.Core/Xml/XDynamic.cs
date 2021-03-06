﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Xml.Linq;

namespace FlitBit.Core.Xml
{
  /// <summary>
  ///   Static class for creating dynamic objects over XML
  /// </summary>
  public static class XDynamic
  {
    /// <summary>
    ///   Parses the input text and returns a dynamic object.
    /// </summary>
    /// <param name="text">source xml text</param>
    /// <param name="includeRootObject">whether or not the root object is included in the structure of the resulting dynamic</param>
    /// <returns>a dynamic object shaped like the input xml</returns>
    public static dynamic Parse(string text, bool includeRootObject)
    {
      Contract.Requires(text != null);
      Contract.Requires(text.Length > 0);

      var xml = XDocument.Parse(text);
      if (includeRootObject)
      {
        var expando = new ExpandoObject() as IDictionary<string, object>;
        if (xml.Root != null)
        {
          expando.Add(xml.Root.Name.LocalName, ToDynamic(xml.Root));
        }
        return expando;
      }
      return ToDynamic(xml.Root);
    }

    /// <summary>
    ///   Parses the input text and returns a dynamic object.
    /// </summary>
    /// <param name="text">source xml text</param>
    /// <returns>a dynamic object shaped like the input xml</returns>
    public static dynamic Parse(string text) { return Parse(text, false); }

    /// <summary>
    ///   Creates an object over the XElement given.
    /// </summary>
    /// <param name="elm">the source element</param>
    /// <returns>an object shaped like the input xml</returns>
    public static object ToDynamic(XElement elm)
    {
      ExpandoObject expando;
      if (elm.IsEmpty)
      {
        expando = new ExpandoObject();
        AddAttributesToDictionary(expando, elm);
        return expando;
      }
      if (!elm.HasAttributes)
      {
        if (elm.HasElements)
        {
          expando = new ExpandoObject();
          AddElementsToDictionary(expando, elm);
          return expando;
        }
        return elm.Value;
      }
      expando = new ExpandoObject();
      AddAttributesToDictionary(expando, elm);
      if (elm.HasElements)
      {
        AddElementsToDictionary(expando, elm);
      }
      else
      {
        (expando as IDictionary<string, object>).Add("Value", elm.Value);
      }
      return expando;
    }

    /// <summary>
    ///   Adds attributes from an element into the dictionary given.
    /// </summary>
    /// <param name="expando">target dictionary</param>
    /// <param name="elm">source element</param>
    static void AddAttributesToDictionary(IDictionary<string, object> expando, XElement elm)
    {
      foreach (var a in elm.Attributes())
      {
        expando.Add(a.Name.LocalName, a.Value);
      }
    }

    /// <summary>
    ///   Adds child elements from an element into the dictionary given.
    /// </summary>
    /// <param name="expando">target dictionary</param>
    /// <param name="elm">source element</param>
    static void AddElementsToDictionary(IDictionary<string, object> expando, XElement elm)
    {
      foreach (var gg in from e in elm.Elements()
                         group e by e.Name.LocalName
                         into g
                         select g)
      {
        if (gg.Count() > 1)
        {
          expando.Add(gg.Key, new List<dynamic>(from item in gg
                                                select ToDynamic(item)));
        }
        else
        {
          expando.Add(gg.Key, ToDynamic(gg.Single()));
        }
      }
    }
  }
}