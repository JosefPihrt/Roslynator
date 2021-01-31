﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents a verifier for a C# diagnostic that is produced by <see cref="DiagnosticAnalyzer"/>.
    /// </summary>
    public abstract class CSharpDiagnosticVerifier : DiagnosticVerifier
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpDiagnosticVerifier"/>.
        /// </summary>
        /// <param name="assert"></param>
        protected CSharpDiagnosticVerifier(IAssert assert) : base(CSharpWorkspaceFactory.Instance, assert)
        {
        }

        /// <summary>
        /// Gets a code verification options.
        /// </summary>
        new public virtual CSharpCodeVerificationOptions Options => CSharpCodeVerificationOptions.Default;

        /// <summary>
        /// Gets a common code verification options.
        /// </summary>
        protected override CodeVerificationOptions CommonOptions => Options;
    }
}
