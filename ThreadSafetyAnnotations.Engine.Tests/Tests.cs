﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using NUnit.Framework;

namespace ThreadSafetyAnnotations.Engine.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void ClassWithPublicLock_CausesIssue()
        {
            List<Issue> issues = Analyze(@"    
                [ThreadSafe]
                public class ClassUnderTest
                {
                    [Lock]
                    public object _lock1;
                }");

            Assert.IsNotNull(issues);
            Assert.GreaterOrEqual(issues.Count, 1);
            Assert.IsTrue(issues.Any(i => i.ErrorCode == ErrorCode.LOCK_IS_NOT_PRIVATE));
        }

        [Test]
        public void ClassWithProtectedLock_CausesIssue()
        {
            List<Issue> issues = Analyze(@"    
                [ThreadSafe]
                protected class ClassUnderTest
                {
                    [Lock]
                    public object _lock1;
                }");

            Assert.IsNotNull(issues);
            Assert.GreaterOrEqual(issues.Count, 1);
            Assert.IsTrue(issues.Any(i => i.ErrorCode == ErrorCode.LOCK_IS_NOT_PRIVATE));
        }

        [Test]
        public void ClassWithNonObjectLock_CausesIssue()
        {
            List<Issue> issues = Analyze(@"   
                [ThreadSafe]
                public class ClassUnderTest
                {
                    [Lock]
                    public SomeLockType _lock1;
                }

                public class SomeLockType : Object { }");

            Assert.IsNotNull(issues);
            Assert.GreaterOrEqual(issues.Count, 1);
            Assert.IsTrue(issues.Any(i => i.ErrorCode == ErrorCode.LOCK_MUST_BE_SYSTEM_OBJECT));
        }

        [Test]
        public void GuardedMemberInNonThreadSafeClass_CausesIssue()
        {
            List<Issue> issues = Analyze(@"    
                public class ClassUnderTest
                {
                    [GuardedByAttribute(""_lock1"")]
                    public int _data1;
                }");

            Assert.IsNotNull(issues);
            Assert.GreaterOrEqual(issues.Count, 1);
            Assert.IsTrue(issues.Any(i => i.ErrorCode == ErrorCode.GUARDED_MEMBER_IN_A_NON_THREAD_SAFE_CLASS));
        }

        [Test]
        public void LockInNonThreadSafeClass_CausesIssue()
        {
            List<Issue> issues = Analyze(@"    
                public class ClassUnderTest
                {
                    [Lock]
                    public SomeLockType _lock1;
                }");

            Assert.IsNotNull(issues);
            Assert.GreaterOrEqual(issues.Count, 1);
            Assert.IsTrue(issues.Any(i => i.ErrorCode == ErrorCode.LOCK_IN_A_NON_THREAD_SAFE_CLASS));
        }

        [Test]
        public void ClassWithPublicDataMember_CausesIssue()
        {
            List<Issue> issues = Analyze(@"    
                [ThreadSafe]
                public class ClassUnderTest
                {
                    [GuardedByAttribute("")]
                    public int _data1;
                }");

            Assert.IsNotNull(issues);
            Assert.GreaterOrEqual(issues.Count, 1);
            Assert.IsTrue(issues.Any(i => i.ErrorCode == ErrorCode.GUARDED_MEMBER_IS_NOT_PRIVATE));
        }

        [Test]
        public void ClassWithProtectedDataMember_CausesIssue()
        {
            List<Issue> issues = Analyze(@"    
                [ThreadSafe]
                public class ClassUnderTest
                {
                    //Lock is public, invalid
                    [GuardedByAttribute("")]
                    protected int _data1;
                }");

            Assert.IsNotNull(issues);
            Assert.GreaterOrEqual(issues.Count, 1);
            Assert.IsTrue(issues.Any(i => i.ErrorCode == ErrorCode.GUARDED_MEMBER_IS_NOT_PRIVATE));
        }

        [Test]
        public void ClassWithUnknownLockName_CausesIssue()
        {
            List<Issue> issues = Analyze(@"    
                [ThreadSafe]
                public class ClassUnderTest
                {
                    //Valid lock
                    [Lock]
                    private object _lock1;

                    //Unknown lock name.
                    [GuardedBy(""_lock2"")]
                    private int _data1;
                }");

            Assert.IsNotNull(issues);
            Assert.GreaterOrEqual(issues.Count, 1);
            Assert.IsTrue(issues.Any(i => i.ErrorCode == ErrorCode.GUARDED_MEMBER_REFERENCES_UNKNOWN_LOCK));
        }

        [Test]
        public void ClassWithEmptyLockName_CausesIssue()
        {
            List<Issue> issues = Analyze(@"    
                [ThreadSafe]
                public class ClassUnderTest
                {
                    //Valid lock
                    [Lock]
                    private object _lock1;

                    //Unknown lock name.
                    [GuardedBy("""")]
                    private int _data1;
                }");

            Assert.IsNotNull(issues);
            Assert.GreaterOrEqual(issues.Count, 1);
            Assert.IsTrue(issues.Any(i => i.ErrorCode == ErrorCode.GUARDED_MEMBER_REFERENCES_UNKNOWN_LOCK));
        }

        private List<Issue> Analyze(string testClassString)
        {
            Compilation compilation = CompilationHelper.Create(testClassString);

            AnalysisEngine engine = new AnalysisEngine(
                compilation.SyntaxTrees[0],
                compilation.GetSemanticModel(compilation.SyntaxTrees[0]));

            return engine.Analzye();
        }
    }
}
