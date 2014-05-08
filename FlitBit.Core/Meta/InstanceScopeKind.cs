#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Core.Meta
{
  /// <summary>
  ///   Basic enumeration of instance scopes.
  /// </summary>
  public enum InstanceScopeKind
  {
    /// <summary>
    ///   Indicates that an instance has a natural scope.
    /// </summary>
    OnDemand = 0,

    /// <summary>
    ///   Indicates that an instance has container scope.
    /// </summary>
    ContainerScope = 1,

    /// <summary>
    ///   Indicates that an instance is a singleton.
    /// </summary>
    Singleton = 2
  }
}