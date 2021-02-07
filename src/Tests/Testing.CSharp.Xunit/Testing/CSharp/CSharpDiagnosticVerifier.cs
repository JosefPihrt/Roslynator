﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents a verifier for a C# diagnostic that is produced by <see cref="DiagnosticAnalyzer"/>.
    /// </summary>
    public abstract class XunitCSharpDiagnosticVerifier : CSharpDiagnosticVerifier
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpDiagnosticVerifier"/>.
        /// </summary>
        protected XunitCSharpDiagnosticVerifier() : base(XunitAssert.Instance)
        {
        }
    }
}