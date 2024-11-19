using System;
using System.Collections.Generic;
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
		private readonly SortedDictionary<string, Namespace> namespacesByName = new SortedDictionary<string, Namespace>(StringComparer.CurrentCultureIgnoreCase);
		private readonly object synchObject = new object();
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
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// Language code.
		/// </summary>
		public string Code
		{
			get => this.code;
			set => this.code = value;
		}

		/// <summary>
		/// Language name.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Language flag.
		/// </summary>
		[DefaultValueNull]
		public byte[] Flag
		{
			get => this.flag;
			set => this.flag = value;
		}

		/// <summary>
		/// Width of flag.
		/// </summary>
		[DefaultValue(0)]
		public int FlagWidth
		{
			get => this.flagWidth;
			set => this.flagWidth = value;
		}

		/// <summary>
		/// Height of flag.
		/// </summary>
		[DefaultValue(0)]
		public int FlagHeight
		{
			get => this.flagHeight;
			set => this.flagHeight = value;
		}

		/// <summary>
		/// Gets the namespace object, given its name, if available.
		/// </summary>
		/// <param name="Name">Namespace.</param>
		/// <returns>Namespace object, if found, or null if not found.</returns>
		public async Task<Namespace> GetNamespaceAsync(string Name)
		{
			lock (this.synchObject)
			{
				if (this.namespacesByName.TryGetValue(Name, out Namespace Result))
					return Result;
			}

			Namespace Namespace = await Database.FindFirstIgnoreRest<Namespace>(new FilterAnd(
				new FilterFieldEqualTo("LanguageId", this.objectId), new FilterFieldEqualTo("Name", Name)));

			if (!(Namespace is null))
			{
				lock (this.synchObject)
				{
					if (this.namespacesByName.TryGetValue(Name, out Namespace Result))
						return Result;
					else
						this.namespacesByName[Namespace.Name] = Namespace;
				}
			}

			return Namespace;
		}

		/// <summary>
		/// Gets the namespace object, given its name, if available.
		/// </summary>
		/// <param name="Name">Namespace.</param>
		/// <param name="CreateIfNotFound">If a new <see cref="Namespace"/> object should be created, if one is not found.</param>
		/// <returns>Namespace object, if found or created, or null if not found.</returns>
		public async Task<Namespace> GetNamespaceAsync(string Name, bool CreateIfNotFound)
		{
			Namespace ns = await this.GetNamespaceAsync(Name);
			if (ns is null && CreateIfNotFound)
				ns = await this.CreateNamespaceAsync(Name);

			return ns;
		}

		/// <summary>
		/// Gets available namespaces.
		/// </summary>
		/// <returns>Namespaces.</returns>
		public async Task<Namespace[]> GetNamespacesAsync()
		{
			if (!this.namespacesLoaded)
			{
				IEnumerable<Namespace> Namespaces = await Database.Find<Namespace>(
					new FilterFieldEqualTo("LanguageId", this.objectId));

				lock (this.synchObject)
				{
					foreach (Namespace Namespace in Namespaces)
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
			if (!(Result is null))
				return Result;
			else
			{
				lock (this.synchObject)
				{
					if (this.namespacesByName.TryGetValue(Name, out Result))
						return Result;

					Result = new Namespace()
					{
						LanguageId = this.objectId,
						Name = Name
					};

					this.namespacesByName[Name] = Result;
				}

				await Database.Insert(Result);

				return Result;
			}
		}

		/// <summary>
		/// Gets the string value of a string ID. If no such string exists, a string is created with the default value.
		/// </summary>
		/// <param name="Type">Type, whose namespace defines in what language namespace the string will be fetched.</param>
		/// <param name="Id">String ID</param>
		/// <param name="Default">Default (untranslated) string.</param>
		/// <returns>Localized string.</returns>
		public Task<string> GetStringAsync(Type Type, int Id, string Default)
		{
			return this.GetStringAsync(Type, Id.ToString(), Default);
		}

		/// <summary>
		/// Gets the string value of a string ID. If no such string exists, a string is created with the default value.
		/// </summary>
		/// <param name="Type">Type, whose namespace defines in what language namespace the string will be fetched.</param>
		/// <param name="Id">String ID</param>
		/// <param name="Default">Default (untranslated) string.</param>
		/// <returns>Localized string.</returns>
		public async Task<string> GetStringAsync(Type Type, string Id, string Default)
		{
			Namespace Namespace = await this.GetNamespaceAsync(Type.Namespace)
				?? await this.CreateNamespaceAsync(Type.Namespace);

			return await Namespace.GetStringAsync(Id, Default);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.name))
				return this.code;
			else
				return this.name + " (" + this.code + ")";
		}
	}
}
