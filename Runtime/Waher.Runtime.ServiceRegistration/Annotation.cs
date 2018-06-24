using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.ServiceRegistration
{
	/// <summary>
	/// Registry annotation.
	/// </summary>
	public class Annotation
	{
		private string tag;
		private string value;

		/// <summary>
		/// Registry annotation.
		/// </summary>
		/// <param name="Tag">Tag name</param>
		/// <param name="value">Value</param>
		public Annotation(string Tag, string value)
		{
			this.tag = Tag;
			this.value = value;
		}

		/// <summary>
		/// Tag name
		/// </summary>
		public string Tag
		{
			get => this.tag;
			set => this.tag = value;
		}

		/// <summary>
		/// Value
		/// </summary>
		public string Value
		{
			get => this.value;
			set => this.value = value;
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.tag + "=" + this.value;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return (obj is Annotation A &&
				this.tag == A.tag &&
				this.value == A.value);
		}

		/// <summary>
		/// <see cref="Object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = 0;

			if (this.tag != null)
				Result = this.tag.GetHashCode();

			if (this.value != null)
				Result ^= Result << 5 ^ this.value.GetHashCode();

			return Result;
		}

		/// <summary>
		/// Merges two arrays of annotations.
		/// </summary>
		/// <param name="Annotations1">First array of annotations.</param>
		/// <param name="Annotations2">Second array of annotations.</param>
		/// <returns>Merged array of annotations.</returns>
		public static Annotation[] Merge(Annotation[] Annotations1, params Annotation[] Annotations2)
		{
			int c, d;

			if (Annotations1 == null || (c = Annotations1.Length) == 0)
				return Annotations2;

			if (Annotations2 == null || (d = Annotations2.Length) == 0)
				return Annotations1;

			Annotation[] Result = new Annotation[c + d];
			Array.Copy(Annotations1, 0, Result, 0, c);
			Array.Copy(Annotations2, 0, Result, c, d);
			
			return Result;
		}

	}
}
