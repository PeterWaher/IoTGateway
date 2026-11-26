namespace Waher.Content.Asn1.Model.Classes
{
	/// <summary>
	/// Contains a reference to a class.
	/// </summary>
	public class Asn1ClassReference
	{
		private readonly string className;
		private readonly string classSet;

		/// <summary>
		/// Contains a reference to a class.
		/// </summary>
		/// <param name="ClassName">Class Name</param>
		public Asn1ClassReference(string ClassName)
			: this(ClassName, null)
		{
		}

		/// <summary>
		/// Contains a reference to a class.
		/// </summary>
		/// <param name="ClassName">Class Name</param>
		/// <param name="ClassSet">Optional Class Set</param>
		public Asn1ClassReference(string ClassName, string ClassSet)
		{
			this.className = ClassName;
			this.classSet = ClassSet;
		}

		/// <summary>
		/// Class Name
		/// </summary>
		public string ClassName => this.className;

		/// <summary>
		/// Optional Class Set
		/// </summary>
		public string ClassSet => this.classSet;

		/// <inheritdoc/>
		public override string ToString()
		{
			if (string.IsNullOrEmpty(this.classSet))
				return this.className;
			else
				return this.className + ":" + this.classSet;
		}
	}
}
