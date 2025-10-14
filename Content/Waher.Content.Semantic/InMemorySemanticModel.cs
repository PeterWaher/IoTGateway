using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Semantic.Ontologies;
using Waher.Runtime.Collections;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects.Matrices;
using Waher.Script.Operators.Matrices;
using Waher.Script.Operators.Vectors;

namespace Waher.Content.Semantic
{
	/// <summary>
	/// In-memory semantic model.
	/// </summary>
	public class InMemorySemanticModel : ISemanticModel, IToMatrix, IToVector
	{
		/// <summary>
		/// Triples in model.
		/// </summary>
		protected readonly ChunkedList<ISemanticTriple> triples;

		#region Constructors

		/// <summary>
		/// In-memory semantic model.
		/// </summary>
		public InMemorySemanticModel()
		{
			this.triples = new ChunkedList<ISemanticTriple>();
		}

		/// <summary>
		/// In-memory semantic model.
		/// </summary>
		/// <param name="Triples">Triples.</param>
		public InMemorySemanticModel(IEnumerable<ISemanticTriple> Triples)
		{
			this.triples = Triples as ChunkedList<ISemanticTriple>;

			if (this.triples is null)
			{
				this.triples = new ChunkedList<ISemanticTriple>();

				foreach (ISemanticTriple Triple in Triples)
					this.triples.Add(Triple);
			}
		}

		#endregion

		#region Enumerators

		/// <summary>
		/// Gets an enumerator of available triples.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public IEnumerator<ISemanticTriple> GetEnumerator()
		{
			return this.triples.GetEnumerator();
		}

		/// <summary>
		/// Gets an enumerator of available triples.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.triples.GetEnumerator();
		}

		#endregion

		#region Adding triples

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, Uri Object)
		{
			this.Add(Subject, new UriNode(Predicate), new UriNode(Object));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, ISemanticElement Object)
		{
			this.Add(Subject, new UriNode(Predicate), Object);
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, object Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate), 
				SemanticElements.EncapsulateLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, string Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new StringLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <param name="Language">Language</param>
		public void Add(ISemanticElement Subject, Uri Predicate, string Object, string Language)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new StringLiteral(Object, Language)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <param name="Type">Type</param>
		public void Add(ISemanticElement Subject, Uri Predicate, string Object, Uri Type)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new CustomLiteral(Object, Type)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <param name="Type">Type</param>
		/// <param name="Language">Langauge</param>
		public void Add(ISemanticElement Subject, Uri Predicate, string Object, Uri Type,
			string Language)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new CustomLiteral(Object, Type, Language)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, DateTime Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new DateTimeLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, bool Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new BooleanLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, byte[] Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new Base64Literal(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, decimal Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new DecimalLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, double Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new DoubleLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, float Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new SingleLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, Duration Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new DurationLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, sbyte Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new Int8Literal(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, short Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new Int16Literal(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, int Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new Int32Literal(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, long Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new Int64Literal(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, byte Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new UInt8Literal(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, ushort Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new UInt16Literal(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, uint Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new UInt32Literal(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, ulong Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new UInt64Literal(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, BigInteger Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new IntegerLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, Uri Predicate, TimeSpan Object)
		{
			this.triples.Add(new SemanticTriple(Subject, new UriNode(Predicate),
				new TimeLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, ISemanticElement Predicate, ISemanticElement Object)
		{
			this.triples.Add(new SemanticTriple(Subject, Predicate, Object));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, ISemanticElement Predicate, object Object)
		{
			this.triples.Add(new SemanticTriple(Subject, Predicate, 
				SemanticElements.EncapsulateLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		public void Add(ISemanticElement Subject, ISemanticElement Predicate, string Object)
		{
			this.triples.Add(new SemanticTriple(Subject, Predicate,
				new StringLiteral(Object)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <param name="Language">Language</param>
		public void Add(ISemanticElement Subject, ISemanticElement Predicate, string Object,
			string Language)
		{
			this.triples.Add(new SemanticTriple(Subject, Predicate,
				new StringLiteral(Object, Language)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <param name="Type">Type</param>
		public void Add(ISemanticElement Subject, ISemanticElement Predicate, string Object,
			Uri Type)
		{
			this.triples.Add(new SemanticTriple(Subject, Predicate,
				new CustomLiteral(Object, Type)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Object">Object</param>
		/// <param name="Type">Type</param>
		/// <param name="Language">Language</param>
		public void Add(ISemanticElement Subject, ISemanticElement Predicate, string Object,
			Uri Type, string Language)
		{
			this.triples.Add(new SemanticTriple(Subject, Predicate,
				new CustomLiteral(Object, Type, Language)));
		}

		/// <summary>
		/// Adds a triple to the model.
		/// </summary>
		/// <param name="Triple">Semantic triple to add.</param>
		public virtual void Add(ISemanticTriple Triple)
		{
			this.triples.Add(Triple);
		}

		#endregion

		#region Adding Linked Lists

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, Uri Predicate,
			params object[] Items)
		{
			this.AddLinkedList(Subject, new UriNode(Predicate), Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params object[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++) 
				Literals[i] = SemanticElements.EncapsulateLiteral(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, Uri Predicate,
			params string[] Items)
		{
			this.AddLinkedList(Subject, new UriNode(Predicate), Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params string[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new StringLiteral(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params bool[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new BooleanLiteral(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params byte[][] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new Base64Literal(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params decimal[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new DecimalLiteral(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params double[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new DoubleLiteral(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params float[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new SingleLiteral(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params Duration[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new DurationLiteral(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params sbyte[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new Int8Literal(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params short[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new Int16Literal(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params int[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new Int32Literal(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params long[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new Int64Literal(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params byte[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new UInt8Literal(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params ushort[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new UInt16Literal(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params uint[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new UInt32Literal(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params ulong[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new UInt64Literal(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params BigInteger[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new IntegerLiteral(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params TimeSpan[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new TimeLiteral(Items[i]);

			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, Uri Predicate,
			IEnumerable Items)
		{
			this.AddLinkedList(Subject, new UriNode(Predicate), Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			IEnumerable Items)
		{
			ChunkedList<ISemanticElement> Literals = new ChunkedList<ISemanticElement>();

			foreach (object Item in Items)
				Literals.Add(SemanticElements.EncapsulateLiteral(Item));

			this.AddLinkedList(Subject, Predicate, Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, Uri Predicate,
			params ISemanticElement[] Items)
		{
			this.AddLinkedList(Subject, new UriNode(Predicate), Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			params ISemanticElement[] Items)
		{
			this.AddLinkedList(Subject, Predicate, (IEnumerable<ISemanticElement>)Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddLinkedList(ISemanticElement Subject, Uri Predicate,
			IEnumerable<ISemanticElement> Items)
		{
			this.AddLinkedList(Subject, new UriNode(Predicate), Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public virtual void AddLinkedList(ISemanticElement Subject, ISemanticElement Predicate,
			IEnumerable<ISemanticElement> Items)
		{
			BlankNode Container = new BlankNode("n" + Guid.NewGuid().ToString());
			ISemanticElement Loop = null;

			this.Add(Subject, Predicate, Container);
			this.Add(Container, Rdf.type, Rdf.List);

			foreach (ISemanticElement Item in Items)
			{
				if (Loop is null)
				{
					this.Add(Container, Rdf.first, Item);
					Loop = Container;
				}
				else
				{
					BlankNode Node = new BlankNode("n" + Guid.NewGuid().ToString());
					this.Add(Container, Rdf.rest, Node);
					this.Add(Node, Rdf.first, Item);
					Loop = Node;
				}
			}

			this.Add(Loop ?? Container, Rdf.rest, Rdf.nil);
		}

		#endregion

		#region Adding Containers

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, Uri Predicate,
			params object[] Items)
		{
			this.AddContainer(Subject, Type, new UriNode(Predicate), Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params object[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = SemanticElements.EncapsulateLiteral(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, Uri Predicate,
			params string[] Items)
		{
			this.AddContainer(Subject, Type, new UriNode(Predicate), Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params string[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new StringLiteral(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params bool[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new BooleanLiteral(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params byte[][] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new Base64Literal(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params decimal[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new DecimalLiteral(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params double[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new DoubleLiteral(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params float[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new SingleLiteral(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params Duration[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new DurationLiteral(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params sbyte[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new Int8Literal(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params short[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new Int16Literal(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params int[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new Int32Literal(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params long[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new Int64Literal(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params byte[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new UInt8Literal(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params ushort[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new UInt16Literal(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params uint[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new UInt32Literal(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params ulong[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new UInt64Literal(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params BigInteger[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new IntegerLiteral(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params TimeSpan[] Items)
		{
			int i, c = Items.Length;
			ISemanticElement[] Literals = new ISemanticElement[c];

			for (i = 0; i < c; i++)
				Literals[i] = new TimeLiteral(Items[i]);

			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, Uri Predicate,
			IEnumerable Items)
		{
			this.AddContainer(Subject, Type, new UriNode(Predicate), Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			IEnumerable Items)
		{
			ChunkedList<ISemanticElement> Literals = new ChunkedList<ISemanticElement>();

			foreach (object Item in Items)
				Literals.Add(SemanticElements.EncapsulateLiteral(Item));

			this.AddContainer(Subject, Type, Predicate, Literals);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, Uri Predicate,
			params ISemanticElement[] Items)
		{
			this.AddContainer(Subject, Type, new UriNode(Predicate), Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			params ISemanticElement[] Items)
		{
			this.AddContainer(Subject, Type, Predicate, (IEnumerable<ISemanticElement>)Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public void AddContainer(ISemanticElement Subject, Uri Type, Uri Predicate,
			IEnumerable<ISemanticElement> Items)
		{
			this.AddContainer(Subject, Type, new UriNode(Predicate), Items);
		}

		/// <summary>
		/// Adds a linked list of items to the graph.
		/// </summary>
		/// <param name="Subject">Subject</param>
		/// <param name="Type">Container type (rdf:Seq, rdf:Bag, rdf:Alt).</param>
		/// <param name="Predicate">Predicate</param>
		/// <param name="Items">Items to add</param>
		public virtual void AddContainer(ISemanticElement Subject, Uri Type, ISemanticElement Predicate,
			IEnumerable<ISemanticElement> Items)
		{
			BlankNode Container = new BlankNode("n" + Guid.NewGuid().ToString());
			int i = 0;

			this.Add(Subject, Predicate, Container);
			this.Add(Container, Rdf.type, Type);

			foreach (ISemanticElement Item in Items)
				this.Add(Container, Rdf.ListItem(++i), Item);
		}

		#endregion

		#region Type Conversions

		/// <summary>
		/// Converts the object to a matrix.
		/// </summary>
		/// <returns>Matrix.</returns>
		public IMatrix ToMatrix()
		{
			ChunkedList<IElement> Elements = new ChunkedList<IElement>();
			int Rows = 0;

			foreach (ISemanticTriple T in this.triples)
			{
				Elements.Add(T.Subject);
				Elements.Add(T.Predicate);
				Elements.Add(T.Object);
				Rows++;
			}

			return new ObjectMatrix(Rows, 3, Elements)
			{
				ColumnNames = new string[] { "Subject", "Predicate", "Object" }
			};
		}

		/// <summary>
		/// Converts the object to a vector.
		/// </summary>
		/// <returns>Matrix.</returns>
		public IElement ToVector()
		{
			return VectorDefinition.Encapsulate(this.triples, false, null);
		}

		#endregion
	}
}
