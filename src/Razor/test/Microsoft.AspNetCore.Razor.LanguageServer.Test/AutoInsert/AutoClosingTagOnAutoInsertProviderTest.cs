// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Test.Common;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Razor.LanguageServer.AutoInsert
{
    public class AutoClosingTagOnAutoInsertProviderTest : RazorOnAutoInsertProviderTestBase
    {
        private RazorLSPOptions Options { get; set; } = RazorLSPOptions.Default;

        private static TagHelperDescriptor UnspecifiedTagHelper
        {
            get
            {
                var descriptor = TagHelperDescriptorBuilder.Create("TestTagHelper", "TestAssembly");
                descriptor.SetTypeName("TestNamespace.TestTagHelper");
                descriptor.TagMatchingRule(builder => builder.RequireTagName("test").RequireTagStructure(TagStructure.Unspecified));

                return descriptor.Build();
            }
        }

        private static TagHelperDescriptor UnspecifiedInputTagHelper
        {
            get
            {
                var descriptor = TagHelperDescriptorBuilder.Create("TestInputTagHelper", "TestAssembly");
                descriptor.SetTypeName("TestNamespace.TestInputTagHelper");
                descriptor.TagMatchingRule(builder => builder.RequireTagName("input").RequireTagStructure(TagStructure.Unspecified));

                return descriptor.Build();
            }
        }

        private static TagHelperDescriptor NormalOrSelfclosingInputTagHelper
        {
            get
            {
                var descriptor = TagHelperDescriptorBuilder.Create("TestInputTagHelper", "TestAssembly");
                descriptor.SetTypeName("TestNamespace.TestInputTagHelper");
                descriptor.TagMatchingRule(builder => builder.RequireTagName("input").RequireTagStructure(TagStructure.NormalOrSelfClosing));

                return descriptor.Build();
            }
        }

        private static TagHelperDescriptor NormalOrSelfClosingTagHelper
        {
            get
            {
                var descriptor = TagHelperDescriptorBuilder.Create("TestTagHelper2", "TestAssembly");
                descriptor.SetTypeName("TestNamespace.TestTagHelper2");
                descriptor.TagMatchingRule(builder => builder.RequireTagName("test").RequireTagStructure(TagStructure.NormalOrSelfClosing));

                return descriptor.Build();
            }
        }

        private static TagHelperDescriptor WithoutEndTagTagHelper
        {
            get
            {
                var descriptor = TagHelperDescriptorBuilder.Create("TestTagHelper3", "TestAssembly");
                descriptor.SetTypeName("TestNamespace.TestTagHelper3");
                descriptor.TagMatchingRule(builder => builder.RequireTagName("test").RequireTagStructure(TagStructure.WithoutEndTag));

                return descriptor.Build();
            }
        }

        [Fact]
        public void OnTypeCloseAngle_NormalOrSelfClosingStructureOverridesVoidTagBehavior()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

<input>$$
",
expected: @"
@addTagHelper *, TestAssembly

<input>$0</input>
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { NormalOrSelfclosingInputTagHelper });
        }

        [Fact]
        public void OnTypeCloseAngle_UnspeccifiedStructureInheritsVoidTagBehavior()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

<input>$$
",
expected: @"
@addTagHelper *, TestAssembly

<input />
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { UnspecifiedInputTagHelper });
        }

        [Fact]
        public void OnTypeCloseAngle_UnspeccifiedTagHelperTagStructure()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

<test>$$
",
expected: @"
@addTagHelper *, TestAssembly

<test>$0</test>
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { UnspecifiedTagHelper });
        }

        [Fact]
        public void OnTypeCloseAngle_NormalOrSelfClosingTagHelperTagStructure()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

<test>$$
",
expected: @"
@addTagHelper *, TestAssembly

<test>$0</test>
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { NormalOrSelfClosingTagHelper });
        }

        [Fact]
        [WorkItem("https://github.com/dotnet/aspnetcore/issues/33930")]
        public void OnTypeCloseAngle_TagHelperInHtml_NestedStatement()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

@if (true)
{
<div><test>$$</div>
}
",
expected: @"
@addTagHelper *, TestAssembly

@if (true)
{
<div><test>$0</test></div>
}
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { NormalOrSelfClosingTagHelper });
        }

        [Fact]
        [WorkItem("https://github.com/dotnet/aspnetcore/issues/33930")]
        public void OnTypeCloseAngle_TagHelperInTagHelper_NestedStatement()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

@if (true)
{
<test><input>$$</test>
}
",
expected: @"
@addTagHelper *, TestAssembly

@if (true)
{
<test><input /></test>
}
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { NormalOrSelfClosingTagHelper, UnspecifiedInputTagHelper });
        }
        [Fact]
        public void OnTypeCloseAngle_NormalOrSelfClosingTagHelperTagStructure_CodeBlock()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

@{
    <test>$$
}
",
expected: @"
@addTagHelper *, TestAssembly

@{
    <test>$0</test>
}
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { NormalOrSelfClosingTagHelper });
        }

        [Fact]
        public void OnTypeCloseAngle_WithSlash_WithoutEndTagTagHelperTagStructure()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

<test />$$
",
expected: @"
@addTagHelper *, TestAssembly

<test />
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { WithoutEndTagTagHelper });
        }

        [Fact]
        [WorkItem("https://github.com/dotnet/aspnetcore/issues/33930")]
        public void OnTypeCloseAngle_NestedStatement()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

@if (true)
{
<div><test />$$</div>
}
",
expected: @"
@addTagHelper *, TestAssembly

@if (true)
{
<div><test /></div>
}
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { WithoutEndTagTagHelper });
        }

        [Fact]
        public void OnTypeCloseAngle_WithSpace_WithoutEndTagTagHelperTagStructure()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

<test >$$
",
expected: @"
@addTagHelper *, TestAssembly

<test />
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { WithoutEndTagTagHelper });
        }

        [Fact]
        public void OnTypeCloseAngle_WithoutEndTagTagHelperTagStructure()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

<test>$$
",
expected: @"
@addTagHelper *, TestAssembly

<test />
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { WithoutEndTagTagHelper });
        }

        [Fact]
        public void OnTypeCloseAngle_WithoutEndTagTagHelperTagStructure_CodeBlock()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

@{
    <test>$$
}
",
expected: @"
@addTagHelper *, TestAssembly

@{
    <test>
}
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { WithoutEndTagTagHelper });
        }

        [Fact]
        public void OnTypeCloseAngle_MultipleApplicableTagHelperTagStructures()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

<test>$$
",
expected: @"
@addTagHelper *, TestAssembly

<test>$0</test>
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { UnspecifiedTagHelper, NormalOrSelfClosingTagHelper, WithoutEndTagTagHelper });
        }

        [Fact]
        public void OnTypeCloseAngle_EscapedTagTagHelperAutoCompletesWithEscape()
        {
            RunAutoInsertTest(
input: @"
@addTagHelper *, TestAssembly

<!test>$$
",
expected: @"
@addTagHelper *, TestAssembly

<!test>$0</!test>
",
fileKind: FileKinds.Legacy,
tagHelpers: new[] { NormalOrSelfClosingTagHelper });
        }

        [Fact]
        public void OnTypeCloseAngle_AlwaysClosesStandardHTMLTag()
        {
            RunAutoInsertTest(
input: @"
    <div><div>$$</div>
",
expected: @"
    <div><div>$0</div></div>
");
        }

        [Fact]
        [WorkItem("https://github.com/dotnet/aspnetcore/issues/33930")]
        public void OnTypeCloseAngle_ClosesStandardHTMLTag_NestedStatement()
        {
            RunAutoInsertTest(
input: @"
@if (true)
{
    <div><p>$$</div>
}
",
expected: @"
@if (true)
{
    <div><p>$0</p></div>
}
");
        }

        [Fact]
        public void OnTypeCloseAngle_ClosesStandardHTMLTag()
        {
            RunAutoInsertTest(
input: @"
    <div>$$
",
expected: @"
    <div>$0</div>
");
        }

        [Fact]
        public void OnTypeCloseAngle_ClosesStandardHTMLTag_CodeBlock()
        {
            RunAutoInsertTest(
input: @"
@{
    <div>$$
}
",
expected: @"
@{
    <div>$0</div>
}
");
        }

        [Fact]
        public void OnTypeCloseAngle_ClosesVoidHTMLTag()
        {
            RunAutoInsertTest(
input: @"
    <input>$$
",
expected: @"
    <input />
");
        }

        [Fact]
        [WorkItem("https://github.com/dotnet/aspnetcore/issues/33930")]
        public void OnTypeCloseAngle_ClosesVoidHTMLTag_NestedStatement()
        {
            RunAutoInsertTest(
input: @"
@if (true)
{
    <strong><input>$$</strong>
}
",
expected: @"
@if (true)
{
    <strong><input /></strong>
}
");
        }

        [Fact]
        public void OnTypeCloseAngle_ClosesVoidHTMLTag_CodeBlock()
        {
            RunAutoInsertTest(
input: @"
@{
    <input>$$
}
",
expected: @"
@{
    <input />
}
");
        }

        [Fact]
        public void OnTypeCloseAngle_WithSlash_ClosesVoidHTMLTag()
        {
            RunAutoInsertTest(
input: @"
    <input />$$
",
expected: @"
    <input />
");
        }

        [Fact]
        public void OnTypeCloseAngle_WithSpace_ClosesVoidHTMLTag()
        {
            RunAutoInsertTest(
input: @"
    <input >$$
",
expected: @"
    <input />
");
        }

        [Fact]
        public void OnTypeCloseAngle_AutoInsertDisabled_Noops()
        {
            Options = new RazorLSPOptions(Trace.Off, enableFormatting: true, autoClosingTags: false, insertSpaces: true, tabSize: 4);
            RunAutoInsertTest(
input: @"
    <div>$$
",
expected: @"
    <div>
");
        }

        internal override RazorOnAutoInsertProvider CreateProvider()
        {
            var optionsMonitor = new Mock<RazorLSPOptionsMonitor>(MockBehavior.Strict);
            optionsMonitor.SetupGet(o => o.GetCurrentOptions).Returns(Options);

            var provider = new AutoClosingTagOnAutoInsertProvider(optionsMonitor.Object);
            return provider;
        }
    }
}
