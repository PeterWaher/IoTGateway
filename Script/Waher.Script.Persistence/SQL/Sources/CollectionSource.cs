using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Settings;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Sources
{
	/// <summary>
	/// Data Source defined by a collection name
	/// </summary>
	public class CollectionSource : IDataSource
	{
		private readonly Dictionary<string, bool> isLabel = new Dictionary<string, bool>();
		private readonly string collectionName;
		private readonly string alias;

		/// <summary>
		/// Data Source defined by a collection name
		/// </summary>
		/// <param name="CollectionName">Collection Name</param>
		/// <param name="Alias">Alias for source.</param>
		public CollectionSource(string CollectionName, string Alias)
		{
			this.collectionName = CollectionName;
			this.alias = Alias;
		}

		/// <summary>
		/// Finds objects matching filter conditions in <paramref name="Where"/>.
		/// </summary>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Enumerator.</returns>
		public async Task<IResultSetEnumerator> Find(int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			IEnumerable<object> Objects = await Database.Find(this.collectionName, Offset, Top,
				TypeSource.Convert(Where, Variables, this.Name), TypeSource.Convert(Order));

			return new SynchEnumerator(Objects.GetEnumerator());
		}

		/// <summary>
		/// Finds and Deletes a set of objects.
		/// </summary>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Offset">Offset at which to return elements.</param>
		/// <param name="Top">Maximum number of elements to return.</param>
		/// <param name="Where">Filter conditions.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Order at which to order the result set.</param>
		/// <param name="Node">Script node performing the evaluation.</param>
		/// <returns>Number of objects deleted, if known.</returns>
		public async Task<int?> FindDelete(bool Lazy, int Offset, int Top, ScriptNode Where, Variables Variables,
			KeyValuePair<VariableReference, bool>[] Order, ScriptNode Node)
		{
			Filter Filter = TypeSource.Convert(Where, Variables, this.Name);
			if (Lazy)
			{
				await Database.DeleteLazy(this.collectionName, Offset, Top, Filter, TypeSource.Convert(Order));
				return null;
			}
			else
			{
				IEnumerable<object> Objects = await Database.FindDelete(this.collectionName, Offset, Top, Filter, TypeSource.Convert(Order));
				int Count = 0;

				foreach (object Obj in Objects)
					Count++;

				return Count;
			}
		}

		/// <summary>
		/// Updates a set of objects.
		/// </summary>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Objects">Objects to update</param>
		public Task Update(bool Lazy, IEnumerable<object> Objects)
		{
			return Lazy ? Database.UpdateLazy(Objects) : Database.Update(Objects);
		}

		/// <summary>
		/// Inserts an object.
		/// </summary>
		/// <param name="Lazy">If operation can be completed at next opportune time.</param>
		/// <param name="Object">Object to insert.</param>
		public Task Insert(bool Lazy, object Object)
		{
			return Lazy ? Database.InsertLazy(Object) : Database.Insert(Object);
		}

		/// <summary>
		/// Name of corresponding collection.
		/// </summary>
		public string CollectionName
		{
			get { return this.collectionName; }
		}

		/// <summary>
		/// Name of corresponding type.
		/// </summary>
		public string TypeName
		{
			get { return string.Empty; }
		}

		/// <summary>
		/// Collection name or alias.
		/// </summary>
		public string Name
		{
			get => string.IsNullOrEmpty(this.alias) ? this.collectionName : this.alias;
		}

		/// <summary>
		/// Checks if the name refers to the source.
		/// </summary>
		/// <param name="Name">Name to check.</param>
		/// <returns>If the name refers to the source.</returns>
		public bool IsSource(string Name)
		{
			return
				string.Compare(this.collectionName, Name, true) == 0 ||
				string.Compare(this.alias, Name, true) == 0;
		}

		/// <summary>
		/// Checks if the label is a label in the source.
		/// </summary>
		/// <param name="Label">Label</param>
		/// <returns>If the label is a label in the source.</returns>
		public async Task<bool> IsLabel(string Label)
		{
			bool Result;

			lock (this.isLabel)
			{
				if (this.isLabel.TryGetValue(Label, out Result))
					return Result;
			}

			Result = await Database.IsLabel(this.collectionName, Label);

			lock (this.isLabel)
			{
				this.isLabel[Label] = Result;
			}

			return Result;
		}

		/// <summary>
		/// Creates an index in the source.
		/// </summary>
		/// <param name="Name">Name of index.</param>
		/// <param name="Fields">Field names. Prefix with hyphen (-) to define descending order.</param>
		public async Task CreateIndex(string Name, string[] Fields)
		{
			await Database.AddIndex(this.collectionName, Fields);

			StringBuilder sb = new StringBuilder();
			foreach (string Field in Fields)
				sb.AppendLine(Field);

			await RuntimeSettings.SetAsync("SQL.INDEX." + this.collectionName + "." + Name, sb.ToString());
		}

		/// <summary>
		/// Drops an index from the source.
		/// </summary>
		/// <param name="Name">Name of index.</param>
		/// <returns>If an index was found and dropped.</returns>
		public async Task<bool> DropIndex(string Name)
		{
			string Key = "SQL.INDEX." + this.collectionName + "." + Name;
			string s = await RuntimeSettings.GetAsync(Key, string.Empty);
			string[] Fields = s.Split(crlf, StringSplitOptions.RemoveEmptyEntries);
			if (Fields.Length == 0)
				return false;

			await Database.RemoveIndex(this.collectionName, Fields);
			await RuntimeSettings.SetAsync(Key, string.Empty);

			return true;
		}

		private static readonly char[] crlf = new char[] { '\r', '\n' };

		/// <summary>
		/// Drops the collection from the source.
		/// </summary>
		public Task DropCollection()
		{
			return Database.DropCollection(this.collectionName);
		}

	}
}
