﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Testing
{
    internal static class ProjectHelpers
    {
        public const string DefaultProjectName = "TestProject";

        public static (Document document, ImmutableArray<ExpectedDocument> expectedDocuments)
            CreateDocument(Solution solution, TestState state, TestOptions options, ProjectOptions projectOptions)
        {
            CompilationOptions compilationOptions = projectOptions.CompilationOptions;

            if (!options.SpecificDiagnosticOptions.IsEmpty)
            {
                ImmutableDictionary<string, ReportDiagnostic> specificOptions = compilationOptions.SpecificDiagnosticOptions;

                specificOptions = specificOptions.SetItems(options.SpecificDiagnosticOptions);

                compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(specificOptions);
            }

            Project project = solution
                .AddProject(DefaultProjectName, DefaultProjectName, projectOptions.Language)
                .WithMetadataReferences(projectOptions.MetadataReferences)
                .WithCompilationOptions(compilationOptions)
                .WithParseOptions(projectOptions.ParseOptions);

            Document document = project.AddDocument(projectOptions.DocumentName, SourceText.From(state.Source));

            ImmutableArray<ExpectedDocument>.Builder expectedDocuments = null;

            ImmutableArray<AdditionalFile> additionalFiles = state.AdditionalFiles;

            if (!additionalFiles.IsEmpty)
            {
                expectedDocuments = ImmutableArray.CreateBuilder<ExpectedDocument>();
                project = document.Project;

                for (int i = 0; i < additionalFiles.Length; i++)
                {
                    Document additionalDocument = project.AddDocument(AppendNumberToFileName(projectOptions.DocumentName, i + 2), SourceText.From(additionalFiles[i].Source));
                    expectedDocuments.Add(new ExpectedDocument(additionalDocument.Id, additionalFiles[i].ExpectedSource));
                    project = additionalDocument.Project;
                }

                document = project.GetDocument(document.Id);
            }

            return (document, expectedDocuments?.ToImmutableArray() ?? ImmutableArray<ExpectedDocument>.Empty);
        }

        private static string AppendNumberToFileName(string fileName, int number)
        {
            int index = fileName.LastIndexOf(".");

            return fileName.Insert(index, (number).ToString(CultureInfo.InvariantCulture));
        }
    }
}
