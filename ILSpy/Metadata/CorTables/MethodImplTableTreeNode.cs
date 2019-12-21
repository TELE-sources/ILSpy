﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.ILSpy.TextView;
using ICSharpCode.ILSpy.TreeNodes;
using System.Reflection;

namespace ICSharpCode.ILSpy.Metadata
{
	internal class MethodImplTableTreeNode : ILSpyTreeNode
	{
		private PEFile module;

		public MethodImplTableTreeNode(PEFile module)
		{
			this.module = module;
		}

		public override object Text => $"19 MethodImpl ({module.Metadata.GetTableRowCount(TableIndex.MethodImpl)})";

		public override object Icon => Images.Literal;

		public override bool View(ViewModels.TabPageModel tabPage)
		{
			tabPage.Title = Text.ToString();
			tabPage.SupportsLanguageSwitching = false;

			var view = Helpers.PrepareDataGrid(tabPage);
			var metadata = module.Metadata;

			var list = new List<MethodImplEntry>();

			for (int row = 1; row <= module.Metadata.GetTableRowCount(TableIndex.MethodImpl); row++) {
				list.Add(new MethodImplEntry(module, MetadataTokens.MethodImplementationHandle(row)));
			}

			view.ItemsSource = list;

			tabPage.Content = view;
			return true;
		}

		struct MethodImplEntry
		{
			readonly int metadataOffset;
			readonly PEFile module;
			readonly MetadataReader metadata;
			readonly MethodImplementationHandle handle;
			readonly MethodImplementation methodImpl;

			public int RID => MetadataTokens.GetToken(handle) & 0xFFFFFF;

			public int Token => MetadataTokens.GetToken(handle);

			public int Offset => metadataOffset
				+ metadata.GetTableMetadataOffset(TableIndex.MethodDef)
				+ metadata.GetTableRowSize(TableIndex.MethodDef) * (RID - 1);

			public int MethodDeclaration => MetadataTokens.GetToken(methodImpl.MethodDeclaration);

			public string MethodDeclarationTooltip {
				get {
					ITextOutput output = new PlainTextOutput();
					methodImpl.MethodDeclaration.WriteTo(module, output, Decompiler.Metadata.GenericContext.Empty);
					return output.ToString();
				}
			}

			public int MethodBody => MetadataTokens.GetToken(methodImpl.MethodBody);

			public string MethodBodyTooltip {
				get {
					ITextOutput output = new PlainTextOutput();
					methodImpl.MethodBody.WriteTo(module, output, Decompiler.Metadata.GenericContext.Empty);
					return output.ToString();
				}
			}

			public int Type => MetadataTokens.GetToken(methodImpl.Type);

			public string TypeTooltip {
				get {
					ITextOutput output = new PlainTextOutput();
					((EntityHandle)methodImpl.Type).WriteTo(module, output, Decompiler.Metadata.GenericContext.Empty);
					return output.ToString();
				}
			}

			public MethodImplEntry(PEFile module, MethodImplementationHandle handle)
			{
				this.metadataOffset = module.Reader.PEHeaders.MetadataStartOffset;
				this.module = module;
				this.metadata = module.Metadata;
				this.handle = handle;
				this.methodImpl = metadata.GetMethodImplementation(handle);
			}
		}

		public override void Decompile(Language language, ITextOutput output, DecompilationOptions options)
		{
			language.WriteCommentLine(output, "MethodImpls");
		}
	}
}