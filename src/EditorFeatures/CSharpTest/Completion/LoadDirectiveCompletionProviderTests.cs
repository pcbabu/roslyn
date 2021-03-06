﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Editor.CSharp.Completion.FileSystem;
using Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Completion.CompletionProviders;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.Completion
{
    public class LoadDirectiveCompletionProviderTests : AbstractCSharpCompletionProviderTests
    {
        public LoadDirectiveCompletionProviderTests(CSharpTestWorkspaceFixture workspaceFixture) : base(workspaceFixture)
        {
        }

        internal override CompletionListProvider CreateCompletionProvider()
        {
            return new LoadDirectiveCompletionProvider();
        }

        protected override bool CompareItems(string actualItem, string expectedItem)
        {
            return actualItem.Equals(expectedItem, StringComparison.OrdinalIgnoreCase);
        }

        protected override Task VerifyWorkerAsync(string code, int position, string expectedItemOrNull, string expectedDescriptionOrNull, SourceCodeKind sourceCodeKind, bool usePreviousCharAsTrigger, bool checkForAbsence, bool experimental, int? glyph)
        {
            return BaseVerifyWorkerAsync(code,
                position,
                expectedItemOrNull,
                expectedDescriptionOrNull,
                sourceCodeKind,
                usePreviousCharAsTrigger,
                checkForAbsence,
                glyph);
        }

        private async Task VerifyItemExistsInScriptAsync(string markup, string expected)
        {
            await VerifyItemExistsAsync(markup, expected, expectedDescriptionOrNull: null, sourceCodeKind: SourceCodeKind.Script, usePreviousCharAsTrigger: false);
        }

        private async Task VerifyItemIsAbsentInInteractiveAsync(string markup, string expected)
        {
            await VerifyItemIsAbsentAsync(markup, expected, expectedDescriptionOrNull: null, sourceCodeKind: SourceCodeKind.Script, usePreviousCharAsTrigger: false);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task NetworkPath()
        {
            await VerifyItemExistsInScriptAsync(
                @"#load ""$$",
                @"\\");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task NetworkPathAfterInitialBackslash()
        {
            await VerifyItemExistsInScriptAsync(
                @"#load ""\$$",
                @"\\");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Completion)]
        public async Task UpOneDirectoryNotShownAtRoot()
        {
            // after so many ".." we should be at the root drive an should no longer suggest the parent.  we can determine
            // our current directory depth by counting the number of backslashes present in the current working directory
            // and after that many references to "..", we are at the root.
            int depth = Directory.GetCurrentDirectory().Count(c => c == Path.DirectorySeparatorChar);
            var pathToRoot = string.Concat(Enumerable.Repeat(@"..\", depth));

            await VerifyItemExistsInScriptAsync(
                @"#load ""$$",
                "..");
            await VerifyItemIsAbsentInInteractiveAsync(
                @"#load """ + pathToRoot + "$$",
                "..");
        }
    }
}
