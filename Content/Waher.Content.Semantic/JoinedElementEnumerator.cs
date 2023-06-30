using System.Collections;
using System.Collections.Generic;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// Enumerator of elements from a collection of semantic data sources.
	/// The enumerator can remove duplicates available in different sources.
	/// </summary>
	public class JoinedElementEnumerator : IEnumerator<ISemanticElement>
	{
		private readonly Dictionary<ISemanticElement, bool> reported;
		private readonly IEnumerable<IEnumerator<ISemanticElement>> enumerators;
		private IEnumerator<IEnumerator<ISemanticElement>> e;
		private IEnumerator<ISemanticElement> current;
		private readonly bool removeDuplicates;

		/// <summary>
		/// Enumerator of elements from a collection of semantic data sources.
		/// The enumerator can remove duplicates available in different sources.
		/// </summary>
		/// <param name="Enumerators">Set of enumerators</param>
		/// <param name="RemoveDuplicates">If duplicates should be removed.</param>
		public JoinedElementEnumerator(IEnumerable<IEnumerator<ISemanticElement>> Enumerators,
			bool RemoveDuplicates)
		{
			this.enumerators = Enumerators;
			this.removeDuplicates = RemoveDuplicates;

			if (this.removeDuplicates)
				this.reported = new Dictionary<ISemanticElement, bool>();
		}

		/// <summary>
		/// Current element
		/// </summary>
		public ISemanticElement Current => this.current?.Current;

		/// <summary>
		/// Current element
		/// </summary>
		object IEnumerator.Current => this.current?.Current;

		/// <summary>
		/// Disposes of the enumerator and sub-enumerators.
		/// </summary>
		public void Dispose()
		{
			foreach (IEnumerator<ISemanticElement> Enumerator in this.enumerators)
				Enumerator.Dispose();
		}

		/// <summary>
		/// Moves to next elements.
		/// </summary>
		/// <returns>If a new element is found.</returns>
		public bool MoveNext()
		{
			if (this.e is null)
			{
				this.e = this.enumerators.GetEnumerator();

				if (!this.e.MoveNext())
					return false;

				this.current = this.e.Current;
			}

			while (true)
			{
				if (this.current.MoveNext())
				{
					if (this.removeDuplicates)
					{
						if (this.reported.ContainsKey(this.current.Current))
							continue;

						this.reported[this.current.Current] = true;
					}

					return true;
				}

				if (!this.e.MoveNext())
					return false;

				this.current = this.e.Current;
			}
		}

		/// <summary>
		/// Resets the enumerator.
		/// </summary>
		public void Reset()
		{
			this.current = null;
			this.e = null;
		}
	}
}
