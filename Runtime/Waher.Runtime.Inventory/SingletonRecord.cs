using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// A record of a singleton instance in memory.
	/// </summary>
	public class SingletonRecord
	{
		private readonly SingletonKey key;
		private readonly bool instantiated;
		private readonly object instance;

		/// <summary>
		/// A record of a singleton instance in memory.
		/// </summary>
		/// <param name="Key">Singleton key.</param>
		/// <param name="Instantiated">If the instance was instantiated by the library (true) or externally (by caller).</param>
		/// <param name="Instance">Object instance.</param>
		public SingletonRecord(SingletonKey Key, bool Instantiated, object Instance)
		{
			this.key = Key;
			this.instantiated = Instantiated;
			this.instance = Instance;
		}

		/// <summary>
		/// Singleton key.
		/// </summary>
		public SingletonKey Key => this.key;

		/// <summary>
		/// If the instance was instantiated by the library (true) or externally (by caller).
		/// </summary>
		public bool Instantiated => this.instantiated;

		/// <summary>
		/// Object instance.
		/// </summary>
		public object Instance => this.instance;
	}
}
