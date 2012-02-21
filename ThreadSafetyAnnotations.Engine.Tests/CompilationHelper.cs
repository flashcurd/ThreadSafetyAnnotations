﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers;
using ThreadSafetyAnnotations.Attributes;

namespace ThreadSafetyAnnotations.Engine.Tests
{
    public static class CompilationHelper
    {
        private const string PROGRAM_BOILERPLATE_PROLOG =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
namespace ThreadSafetyAnnotations.Engine.Tests.Boilerplate
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class GuardedByAttribute : Attribute
    {
        private string[] _lockNames;
        public GuardedByAttribute(params string[] lockNames)
        {
            _lockNames = lockNames;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class LockAttribute : Attribute
    {
    }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ThreadSafeAttribute : Attribute
    {
    }
";

        private const string PROGRAM_BOILERPLATE_EPILOG =
    @"
}";

        public static Compilation Create(string testClassString)
        {
            SyntaxTree tree = SyntaxTree.ParseCompilationUnit(GetProgramText(testClassString));

            return Compilation.Create("TestCompilation")
                                .AddReferences(new AssemblyFileReference(typeof(object).Assembly.Location))                                
                                .AddSyntaxTrees(tree);
        }


        private static string GetProgramText(string testClassString)
        {
            return PROGRAM_BOILERPLATE_PROLOG + testClassString + PROGRAM_BOILERPLATE_EPILOG;
        }
    }
}