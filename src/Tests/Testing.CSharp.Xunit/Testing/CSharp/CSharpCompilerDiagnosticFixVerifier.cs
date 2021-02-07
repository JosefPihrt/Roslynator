﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents a verifier for C# compiler diagnostics.
    /// </summary>
    public abstract class XunitCSharpCompilerDiagnosticFixVerifier : CSharpCompilerDiagnosticFixVerifier
    {
        /// <summary>
        /// Initializes a new instance of <see cref="XunitCSharpCompilerDiagnosticFixVerifier"/>
        /// </summary>
        protected XunitCSharpCompilerDiagnosticFixVerifier() : base(XunitAssert.Instance)
        {
        }
    }
}