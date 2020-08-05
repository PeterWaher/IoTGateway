using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using SkiaSharp;
using Waher.Content;
using Waher.Events;
using Waher.Script;
using Waher.Runtime.Inventory;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model;

namespace Waher.Layout.Layout2D
{
	/// <summary>
	/// Contains a 2D layout document.
	/// </summary>
	public class Layout2DDocument : IDisposable
	{
		/// <summary>
		/// Layout2D
		/// </summary>
		public const string LocalName = "Layout2D";

		/// <summary>
		/// http://waher.se/Layout/Layout2D.xsd
		/// </summary>
		public const string Namespace = "http://waher.se/Layout/Layout2D.xsd";

		private static Dictionary<string, ILayoutElement> elementTypes = new Dictionary<string, ILayoutElement>();
		private static bool initialized = false;

		private readonly Variables session;
		private readonly ILayoutElement root;

		#region Construction

		/// <summary>
		/// Contains a 2D layout document.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public Layout2DDocument(XmlDocument Xml)
			: this(Xml, new Variables())
		{
		}

		/// <summary>
		/// Contains a 2D layout document.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Session">Session variables</param>
		public Layout2DDocument(XmlDocument Xml, Variables Session)
			: this(Xml.DocumentElement, Session)
		{
		}

		/// <summary>
		/// Contains a 2D layout document.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public Layout2DDocument(XmlElement Xml)
			: this(Xml, new Variables())
		{
		}

		/// <summary>
		/// Contains a 2D layout document.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Session">Session variables</param>
		public Layout2DDocument(XmlElement Xml, Variables Session)
		{
			this.session = Session;

			if (Xml.LocalName != LocalName || Xml.NamespaceURI != Namespace)
				throw new ArgumentException("XML does not represend a 2D layout document.", nameof(Xml));

			lock (elementTypes)
			{
				if (!initialized)
				{
					Type[] LayoutElementTypes = Types.GetTypesImplementingInterface(typeof(ILayoutElement));
					Dictionary<string, ILayoutElement> TypesPerKey = new Dictionary<string, ILayoutElement>();

					foreach (Type T in LayoutElementTypes)
					{
						TypeInfo TI = T.GetTypeInfo();
						if (TI.IsAbstract)
							continue;

						try
						{
							ILayoutElement E = (ILayoutElement)Activator.CreateInstance(T, this, null);
							string Key = E.Namespace + "#" + E.LocalName;

							if (TypesPerKey.ContainsKey(Key))
								Log.Error("Layout element type already defined: " + Key);
							else
								TypesPerKey[Key] = E;
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					elementTypes = TypesPerKey;
					initialized = true;

					Types.OnInvalidated += (sender, e) => initialized = false;
				}
			}

			root = this.CreateElement(Xml, null);
		}

		internal ILayoutElement CreateElement(XmlElement Xml, ILayoutElement Parent)
		{
			string Key = Xml.NamespaceURI + "#" + Xml.LocalName;

			if (!elementTypes.TryGetValue(Key, out ILayoutElement E))
				throw new LayoutSyntaxException("Layout element not recognized: " + Key);

			ILayoutElement Result = E.Create(this, Parent);
			Result.FromXml(Xml);

			return Result;
		}

		/// <summary>
		/// Loads a 2D layout document from a file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		public static Layout2DDocument FromFile(string FileName)
		{
			return FromFile(FileName, true);
		}

		/// <summary>
		/// Loads a 2D layout document from a file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		public static Layout2DDocument FromFile(string FileName, bool Preprocess)
		{
			return FromFile(FileName, Preprocess, new Variables());
		}

		/// <summary>
		/// Loads a 2D layout document from a file.
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		/// <param name="Session">Session variables.</param>
		public static Layout2DDocument FromFile(string FileName, bool Preprocess, Variables Session)
		{
			string Xml = File.ReadAllText(FileName);
			return FromXml(Xml, Preprocess, Session);
		}

		/// <summary>
		/// Loads a 2D layout document from a stream.
		/// </summary>
		/// <param name="Input">Stream input.</param>
		/// <param name="DefaultEncoding">Default text encoding to use, if not deduced from a Byte-Order-Mark (BOM) of the file.</param>
		public static Layout2DDocument FromStream(Stream Input, Encoding DefaultEncoding)
		{
			return FromStream(Input, DefaultEncoding, true);
		}

		/// <summary>
		/// Loads a 2D layout document from a stream.
		/// </summary>
		/// <param name="Input">Stream input.</param>
		/// <param name="DefaultEncoding">Default text encoding to use, if not deduced from a Byte-Order-Mark (BOM) of the file.</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		public static Layout2DDocument FromStream(Stream Input, Encoding DefaultEncoding, bool Preprocess)
		{
			return FromStream(Input, DefaultEncoding, Preprocess, new Variables());
		}

		/// <summary>
		/// Loads a 2D layout document from a stream.
		/// </summary>
		/// <param name="Input">Stream input.</param>
		/// <param name="DefaultEncoding">Default text encoding to use, if not deduced from a Byte-Order-Mark (BOM) of the file.</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		/// <param name="Session">Session variables.</param>
		public static Layout2DDocument FromStream(Stream Input, Encoding DefaultEncoding, bool Preprocess, Variables Session)
		{
			long c = Input.Length - Input.Position;
			if (c > int.MaxValue)
				throw new OutOfMemoryException("Input too large");

			int c2 = (int)c;
			byte[] Bin = new byte[c2];
			int i = Input.Read(Bin, 0, c2);

			if (i != c2)
				throw new IOException("Unexpected end of file.");

			string Xml = CommonTypes.GetString(Bin, DefaultEncoding);

			return FromXml(Xml, Preprocess, Session);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public static Layout2DDocument FromXml(string Xml)
		{
			return FromXml(Xml, true);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		public static Layout2DDocument FromXml(string Xml, bool Preprocess)
		{
			return FromXml(Xml, Preprocess, new Variables());
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Preprocess">If embedded script should be preprocessed.</param>
		/// <param name="Session">Session variables.</param>
		public static Layout2DDocument FromXml(string Xml, bool Preprocess, Variables Session)
		{
			if (Preprocess)
				Xml = Expression.Transform(Xml, "{{", "}}", Session);

			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Xml);

			return FromXml(Doc, Session);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public static Layout2DDocument FromXml(XmlDocument Xml)
		{
			return FromXml(Xml, new Variables());
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Session">Session variables.</param>
		public static Layout2DDocument FromXml(XmlDocument Xml, Variables Session)
		{
			return new Layout2DDocument(Xml, Session);
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		public static Layout2DDocument FromXml(XmlElement Xml)
		{
			return FromXml(Xml, new Variables());
		}

		/// <summary>
		/// Parses a 2D layout document from its XML definition.
		/// </summary>
		/// <param name="Xml">XML Definition</param>
		/// <param name="Session">Session variables.</param>
		public static Layout2DDocument FromXml(XmlElement Xml, Variables Session)
		{
			return new Layout2DDocument(Xml, Session);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.root?.Dispose();
		}

		#endregion

		#region Rendering

		public SKImage Render(int Width, int Height, double Zoom, int OffsetX, int OffsetY, out Map[] Maps)
		{
			throw new NotImplementedException();
		}

		#endregion

		/// <summary>
		/// Event raised when the internal state of the layout has been updated.
		/// </summary>
		public event EventHandler OnUpdated = null;

	}

	/* TODO:
	 * 
     * {}
     * 
     * Tree layout
     * Radix/circular
     * Directed graphs
     * 
     * Blur when too small, and dont continue rendering
     * Clip optimization
     * 
	 */

}
