﻿using System;
using System.Text;
using ICD.Common.Utils.IO;
#if SIMPLSHARP
using Crestron.SimplSharp.CrestronXml;
#else
using System.Xml;
#endif

namespace ICD.Common.Utils.Xml
{
	public sealed class IcdXmlTextWriter : IDisposable
	{
#if SIMPLSHARP
		private readonly XmlTextWriter m_Writer;
#else
		private readonly XmlWriter m_Writer;
#endif

		public XmlWriter WrappedWriter { get { return m_Writer; } }

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="encoding"></param>
		public IcdXmlTextWriter(IcdStream stream, Encoding encoding)
#if SIMPLSHARP
			: this(new XmlTextWriter(stream.WrappedStream, encoding))
#else
			: this(XmlWriter.Create(stream.WrappedStream, GetSettings(encoding)))
#endif
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="textWriter"></param>
		public IcdXmlTextWriter(IcdTextWriter textWriter)
#if SIMPLSHARP
			: this(new XmlTextWriter(textWriter.WrappedTextWriter))
#else
			: this(XmlWriter.Create(textWriter.WrappedTextWriter))
#endif
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="writer"></param>
#if SIMPLSHARP
		private IcdXmlTextWriter(XmlTextWriter writer)
#else
		private IcdXmlTextWriter(XmlWriter writer)
#endif
		{
			m_Writer = writer;

#if SIMPLSHARP
			m_Writer.Formatting = Formatting.Indented;
#endif
		}

		#endregion

		#region Methods

		public void WriteStartElement(string elementName)
		{
			m_Writer.WriteStartElement(elementName);
		}

		public void WriteElementString(string elementName, string value)
		{
			m_Writer.WriteElementString(elementName, value);
		}

		public void WriteEndElement()
		{
			m_Writer.WriteEndElement();
		}

		public void WriteComment(string comment)
		{
			m_Writer.WriteComment(comment);
		}

		public void Dispose()
		{
#if SIMPLSHARP
			m_Writer.Dispose(true);
#else
			m_Writer.Dispose();
#endif
		}

		public void WriteAttributeString(string attributeName, string value)
		{
			m_Writer.WriteAttributeString(attributeName, value);
		}

		public void Flush()
		{
			m_Writer.Flush();
		}

		public void Close()
		{
#if SIMPLSHARP
			m_Writer.Close();
#else
			m_Writer.Dispose();
#endif
		}

		public void WriteRaw(string xml)
		{
			m_Writer.WriteRaw(xml);
		}

#endregion

#region Private Methods

#if STANDARD
		private static XmlWriterSettings GetSettings(Encoding encoding)
		{
			return new XmlWriterSettings
			{
				Encoding = encoding,
				Indent = true
			};
		}
#endif

#endregion
	}
}
