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