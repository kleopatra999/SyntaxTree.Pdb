﻿//
// PdbTest.cs
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SyntaxTree.Pdb.Test
{
	[TestFixture]
	public class MappingTest : PdbTestBase
	{
		public int Answer()
		{
			int a = 4;
			int b = 10;

			const int c = 2;

			return a * b + c;
		}

		[Test]
		public void MapSourceFiles()
		{
			var pdb = ProgramDatabase.Read(pdbFileName);

			var mappingFiles = new Dictionary<string, string>
			{
				{"MappingTest.cs", "NewMappingTest.cs"},
			};

			var documents = pdb.Functions.SelectMany(f => f.SequencePoints).Select(s => s.Document);
			foreach (var document in documents)
			{
				string mappedFile;
				if (mappingFiles.TryGetValue(Path.GetFileName(document.Name), out mappedFile))
					document.Name = Path.Combine(Path.GetDirectoryName(document.Name), mappedFile);
			}

			pdb.Write(pdbFileName, new CecilMetadataProvider(module));

			pdb = ProgramDatabase.Read(pdbFileName);
			documents = pdb.Functions.SelectMany(f => f.SequencePoints).Select(s => s.Document).ToList();

			Assert.IsFalse(documents.Any(d => Path.GetFileName(d.Name) == "MappingTest.cs"));
		}
	}
}
