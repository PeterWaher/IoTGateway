using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Persistence.Filters;

namespace Waher.Runtime.Language
{
	/// <summary>
	/// Contains information about a language.
	/// </summary>
	[CollectionName("Languages")]
	[Index("Code")]
	[TypeName(TypeNameSerialization.None)]
	public class Language
	{
		private SortedDictionary<string, Namespace> namespacesByName = new SortedDictionary<string, Namespace>(StringComparer.CurrentCultureIgnoreCase);
		private object synchObject = new object();
		private Guid objectId = Guid.Empty;
		private string code = string.Empty;
		private string name = string.Empty;
		private byte[] flag = null;
		private int flagWidth = 0;
		private int flagHeight = 0;
		private bool namespacesLoaded = false;

		/// <summary>
		/// Contains information about a language.
		/// </summary>
		public Language()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public Guid ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// Language code.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Code
		{
			get { return this.code; }
			set { this.code = value; }
		}

		/// <summary>
		/// Language name.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// Language flag.
		/// </summary>
		[DefaultValueNull]
		public byte[] Flag
		{
			get { return this.flag; }
			set { this.flag = value; }
		}

		/// <summary>
		/// Width of flag.
		/// </summary>
		[DefaultValue(0)]
		public int FlagWidth
		{
			get { return this.flagWidth; }
			set { this.flagWidth = value; }
		}

		/// <summary>
		/// Height of flag.
		/// </summary>
		[DefaultValue(0)]
		public int FlagHeight
		{
			get { return this.flagHeight; }
			set { this.flagHeight = value; }
		}

		/// <summary>
		/// Gets the namespace object, given its name, if available.
		/// </summary>
		/// <param name="Name">Namespace.</param>
		/// <returns>Namespace object, if found, or null if not found.</returns>
		public async Task<Namespace> GetNamespaceAsync(string Name)
		{
			Namespace Result;

			lock (this.synchObject)
			{
				if (this.namespacesByName.TryGetValue(Name, out Result))
					return Result;
			}

			foreach (Namespace Namespace in await Database.Find<Namespace>(new FilterAnd(
				new FilterFieldEqualTo("LanguageId", this.objectId), new FilterFieldEqualTo("Name", Name))))
			{
				lock (this.synchObject)
				{
					this.namespacesByName[Namespace.Name] = Namespace;
				}

				return Namespace;
			}

			return null;
		}

		/// <summary>
		/// Gets available namespaces.
		/// </summary>
		/// <returns>Namespaces.</returns>
		public async Task<Namespace[]> GetNamespacesAsync()
		{
			if (!this.namespacesLoaded)
			{
				foreach (Namespace Namespace in await Database.Find<Namespace>(new FilterFieldEqualTo("Code", this.code)))
				{
					lock (this.synchObject)
					{
						if (!this.namespacesByName.ContainsKey(Namespace.Name))
							this.namespacesByName[Namespace.Name] = Namespace;
					}
				}

				this.namespacesLoaded = true;
			}

			lock (this.synchObject)
			{
				Namespace[] Result = new Namespace[this.namespacesByName.Count];
				this.namespacesByName.Values.CopyTo(Result, 0);
				return Result;
			}
		}

		/// <summary>
		/// Creates a new language namespace, or updates an existing language namespace, if one exist with the same properties.
		/// </summary>
		/// <param name="Name">Namespace.</param>
		/// <returns>Namespace object.</returns>
		public async Task<Namespace> CreateNamespaceAsync(string Name)
		{
			Namespace Result = await this.GetNamespaceAsync(Name);
			if (Result != null)
				return Result;
			else
			{
				Result = new Namespace();
				Result.LanguageId = this.objectId;
				Result.Name = Name;

				lock (synchObject)
				{
					this.namespacesByName[Code] = Result;
				}

				await Database.Insert(Result);

				return Result;
			}
		}

	}
}
