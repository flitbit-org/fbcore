#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Core.Log
{
  internal sealed class NullLogEventWriter : LogEventWriter
  {
    public override void Initialize(string sourceName)
    {}

    public override void WriteLogEvent(LogEvent evt)
    {
	    
    }
  }
}