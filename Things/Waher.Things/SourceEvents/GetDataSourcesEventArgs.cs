using System;
using System.Collections.Generic;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Event arguments for events collecting data sources.
	/// </summary>
	public class GetDataSourcesEventArgs : EventArgs
	{
		private readonly List<IDataSource> sources = new List<IDataSource>();

		/// <summary>
		/// Event arguments for events collecting data sources.
		/// </summary>
		/// <param name="DataSources">Initial data sources.</param>
		public GetDataSourcesEventArgs(params IDataSource[] DataSources)
		{
			if (!(DataSources is null))
				this.sources.AddRange(DataSources);
		}

		/// <summary>
		/// Adds a data source to the list of data sources, if not already registered.
		/// </summary>
		/// <param name="Source">Source to register</param>
		/// <returns>If the source was added (true), or if it was not because one
		/// with the source ID already exists (false).</returns>
		public bool Add(IDataSource Source)
		{
			if (Source is null)
				return false;

			foreach (IDataSource Source2 in this.sources)
			{
				if (Source2.SourceID == Source.SourceID)
					return false;
			}

			this.sources.Add(Source);

			return true;
		}

		/// <summary>
		/// Added sources.
		/// </summary>
		public IDataSource[] Sources => this.sources.ToArray();
	}
}
