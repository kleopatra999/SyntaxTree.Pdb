﻿//
// ProgramDatabase.cs
//
// Copyright (c) 2011 SyntaxTree
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Cci.Pdb;

namespace SyntaxTree.Pdb
{
	/// <summary>
	/// Representation of a pdb file for a managed assembly
	/// </summary>
	public sealed class ProgramDatabase
	{
		private readonly Collection<Function> functions;

		/// <summary>
		/// The functions defined in the pdb file.
		/// </summary>
		public Collection<Function> Functions { get { return functions; } }

		public ProgramDatabase()
		{
			this.functions = new Collection<Function>();
		}

		internal ProgramDatabase(IEnumerable<PdbFunction> pdbFunctions) : this()
		{
			this.functions.AddRange(pdbFunctions.Select(f => new Function(f)));
		}

		internal void Write(PdbWriter writer)
		{
			foreach (var function in functions)
				function.Write(writer);
		}

		/// <summary>
		/// Write the pdb to a file.
		/// </summary>
		/// <param name="fileName">The file name to write the pdb into.</param>
		/// <param name="metadataProvider">The metadata provider for the managed metadata.</param>
		public void Write(string fileName, IMetadataProvider metadataProvider)
		{
			if (fileName == null)
				throw new ArgumentNullException("fileName");
			if (metadataProvider == null)
				throw new ArgumentNullException("metadataProvider");

			using (var writer = new PdbWriter(fileName, metadataProvider))
				Write(writer);
		}

		/// <summary>
		/// Write the pdb to a stream.
		/// </summary>
		/// <param name="stream">The stream to write the pdb into.</param>
		/// <param name="metadataProvider">The metadata provider for the managed metadata.</param>
		public void Write(Stream stream, IMetadataProvider metadataProvider)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			if (metadataProvider == null)
				throw new ArgumentNullException("metadataProvider");

			var tempPath = Path.GetTempFileName();
			try
			{
				Write(tempPath, metadataProvider);
				using (var file = File.OpenRead(tempPath))
					file.CopyTo(stream);
			}
			finally
			{
				File.Delete(tempPath);
			}
		}

		/// <summary>
		/// Read a pdb from a file.
		/// </summary>
		/// <param name="fileName">The file name of the pdb.</param>
		/// <returns>A representation of the pdb.</returns>
		public static ProgramDatabase Read(string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException("fileName");
			if (!File.Exists(fileName))
				throw new FileNotFoundException("Pdb not found", fileName);

			using (var file = File.OpenRead(fileName))
				return Read(file);
		}

		/// <summary>
		/// Read a pdb from a stream.
		/// </summary>
		/// <param name="stream">The stream containing the pdb.</param>
		/// <returns>A representation of the pdb.</returns>
		public static ProgramDatabase Read(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			if (!stream.CanRead)
				throw new ArgumentException("Can not read from stream", "stream");

			return new ProgramDatabase(PdbFile.LoadFunctions(stream, readAllStrings: true));
		}
	}
}