using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlitBit.Core.Meta
{
	/// <summary>
	/// Basic enumeration of instance scopes.
	/// </summary>
	public enum InstanceScopeKind
	{
		/// <summary>
		/// Indicates that an instance has a natural scope.
		/// </summary>
		OnDemand = 0,
		/// <summary>
		/// Indicates that an instance has container scope.
		/// </summary>
		ContainerScope = 1,
		/// <summary>
		/// Indicates that an instance is a singleton.
		/// </summary>
		Singleton = 2
	}
}
