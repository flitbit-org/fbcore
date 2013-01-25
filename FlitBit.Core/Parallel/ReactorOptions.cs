using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace FlitBit.Core.Parallel
{
	/// <summary>
	/// Options for the Reactor class.
	/// </summary>
	public sealed class ReactorOptions
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public ReactorOptions()
			: this(Convert.ToInt32(Environment.ProcessorCount * 1.5))
		{
		}

		/// <summary>
		/// Creates a new instance with a max degree of parallelism.
		/// </summary>
		/// <param name="maxDegreeOfParallelism">a max degree of parallelism</param>
		public ReactorOptions(int maxDegreeOfParallelism)
			: this(maxDegreeOfParallelism, false, 0)
		{
		}

		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="maxDegreeOfParallelism">a max degree of parallelism</param>
		/// <param name="yieldBusyReactor">indicates whether to occasionally yield a busy reactor</param>
		/// <param name="yieldFrequency">indicates yield frequency when yielding a busy reactor</param>
		public ReactorOptions(int maxDegreeOfParallelism, bool yieldBusyReactor, int yieldFrequency)
		{
			Contract.Requires<ArgumentOutOfRangeException>(maxDegreeOfParallelism >= 1);
			Contract.Requires<ArgumentOutOfRangeException>(!yieldBusyReactor || yieldFrequency >= 1);

			MaxDegreeOfParallelism = maxDegreeOfParallelism;
			YieldBusyReactor = yieldBusyReactor;
			YieldFrequency = yieldFrequency;
		}

		/// <summary>
		/// The reactor's max degree of parallelism. This option controls the maximum number of concurrent threads
		/// used to react to items pushed to the reactor.
		/// </summary>
		public int MaxDegreeOfParallelism { get; private set; }
		/// <summary>
		/// Whether the reactor yields busy reactor threads. This option can provide better parallelism when the
		/// entire thread pool is busy.
		/// </summary>
		public bool YieldBusyReactor { get; private set; }
		/// <summary>
		/// Indicates the frequency at which a reactor thread yields.
		/// </summary>
		/// <remarks>Generally speaking, when a reactor is configured to yield, each thread pool thread will
		/// react to at most YieldFrequency items before yielding the thread back to the pool.</remarks>
		public int YieldFrequency { get; private set; }
	}

}
