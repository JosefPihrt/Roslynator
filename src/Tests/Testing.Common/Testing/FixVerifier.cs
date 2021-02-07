﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.Testing.Text;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents verifier for a diagnostic produced by <see cref="DiagnosticAnalyzer"/> and a code fix provided by <see cref="CodeFixProvider"/>.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class FixVerifier : DiagnosticVerifier
    {
        private ImmutableArray<string> _fixableDiagnosticIds;

        internal FixVerifier(IAssert assert) : base(assert)
        {
        }

        /// <summary>
        /// Gets a <see cref="CodeFixProvider"/> that can fix specified diagnostic.
        /// </summary>
        public abstract CodeFixProvider FixProvider { get; }

        internal ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                if (_fixableDiagnosticIds.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _fixableDiagnosticIds, FixProvider.FixableDiagnosticIds);

                return _fixableDiagnosticIds;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get { return $"{Descriptor.Id} {string.Join(", ", Analyzers.Select(f => f.GetType().Name))} {FixProvider.GetType().Name}"; }
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic and that the diagnostic will be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="expected"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAndFixAsync(
            string source,
            string expected,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextParser.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAsync(state, options, projectOptions, cancellationToken);

            await VerifyFixAsync(state, options, projectOptions, cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic and that the diagnostic will not be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAndNoFixAsync(
            string source,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextParser.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                null,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAsync(state, options, projectOptions, cancellationToken);

            await VerifyNoFixAsync(state, options, projectOptions, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic and that the diagnostic will be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/> and <paramref name="expectedData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="expectedData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAndFixAsync(
            string source,
            string sourceData,
            string expectedData,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextParser.FindSpansAndReplace(source, sourceData, expectedData);

            var state = new DiagnosticTestState(
                result.Text,
                result.Expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAsync(state, options: options, projectOptions, cancellationToken);

            await VerifyFixAsync(state, options: options, projectOptions, cancellationToken);
        }

        internal async Task VerifyFixAsync(
            string source,
            string sourceData,
            string expectedData,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextParser.FindSpansAndReplace(source, sourceData, expectedData);

            var state = new DiagnosticTestState(
                result.Text,
                result.Expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey);

            await VerifyFixAsync(
                source: result.Text,
                expected: result.Expected,
                equivalenceKey: equivalenceKey,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyFixAsync(
            string source,
            string expected,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextParser.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey);

            await VerifyFixAsync(
                state: state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyFixAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;
            projectOptions ??= ProjectOptions;

            if (!SupportedDiagnostics.Contains(Descriptor, DiagnosticDescriptorComparer.Id))
                Assert.True(false, $"Diagnostic '{Descriptor.Id}' is not supported by analyzer(s) {string.Join(", ", Analyzers.Select(f => f.GetType().Name))}.");

            if (!FixableDiagnosticIds.Contains(Descriptor.Id))
                Assert.True(false, $"Diagnostic '{Descriptor.Id}' is not fixable by code fix provider '{FixProvider.GetType().Name}'.");

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options, projectOptions);

                Project project = document.Project;

                document = project.GetDocument(document.Id);

                Compilation compilation = await project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation);

                ImmutableArray<Diagnostic> previousDiagnostics = ImmutableArray<Diagnostic>.Empty;

                var fixRegistered = false;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    ImmutableArray<Diagnostic> diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, DiagnosticComparer.SpanStart, cancellationToken);

                    int length = diagnostics.Length;

                    if (length == 0)
                        break;

                    if (length == previousDiagnostics.Length
                        && !diagnostics.Except(previousDiagnostics, DiagnosticDeepEqualityComparer.Instance).Any())
                    {
                        Assert.True(false, "Same diagnostics returned before and after the fix was applied.");
                    }

                    Diagnostic diagnostic = null;
                    foreach (Diagnostic d in diagnostics)
                    {
                        if (d.Id == Descriptor.Id)
                        {
                            diagnostic = d;
                            break;
                        }
                    }

                    if (diagnostic == null)
                        break;

                    CodeAction action = null;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if (action == null
                                && (state.EquivalenceKey == null || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                                && d.Contains(diagnostic))
                            {
                                action = a;
                            }
                        },
                        CancellationToken.None);

                    await FixProvider.RegisterCodeFixesAsync(context);

                    if (action == null)
                        break;

                    fixRegistered = true;

                    document = await VerifyAndApplyCodeActionAsync(document, action, state.CodeActionTitle);

                    compilation = await document.Project.GetCompilationAsync(cancellationToken);

                    ImmutableArray<Diagnostic> newCompilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                    VerifyCompilerDiagnostics(newCompilerDiagnostics, options);

                    VerifyNoNewCompilerDiagnostics(compilerDiagnostics, newCompilerDiagnostics, options);

                    compilation = UpdateCompilation(compilation);

                    previousDiagnostics = diagnostics;
                }

                Assert.True(fixRegistered, "No code fix has been registered.");

                string actual = await document.ToFullStringAsync(simplify: true, format: true, cancellationToken);

                Assert.Equal(state.ExpectedSource, actual);

                if (expectedDocuments.Any())
                    await VerifyAdditionalDocumentsAsync(document.Project, expectedDocuments, cancellationToken);
            }
        }

        /// <summary>
        /// Verifies that specified source does not contains diagnostic that can be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoFixAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextParser.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                result.Expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey);

            await VerifyNoFixAsync(
                state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source does not contains diagnostic that can be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoFixAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;
            projectOptions ??= ProjectOptions;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options, projectOptions);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation);

                ImmutableArray<Diagnostic> diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, DiagnosticComparer.SpanStart, cancellationToken);

                foreach (Diagnostic diagnostic in diagnostics)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!string.Equals(diagnostic.Id, Descriptor.Id, StringComparison.Ordinal))
                        continue;

                    if (!FixableDiagnosticIds.Contains(diagnostic.Id))
                        continue;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if ((state.EquivalenceKey == null || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                                && d.Contains(diagnostic))
                            {
                                Assert.True(false, "No code fix expected.");
                            }
                        },
                        CancellationToken.None);

                    await FixProvider.RegisterCodeFixesAsync(context);
                }
            }
        }
    }
}
