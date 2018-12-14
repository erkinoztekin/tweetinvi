﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testinvi.TestObjects;
using Tweetinvi;
using Tweetinvi.Core.Exceptions;

namespace Testinvi.Tweetinvi
{
    [TestClass]
    public class SyncTests
    {
        #region Task ExecuteTaskAsync(Action)

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ExecuteTaskAsyncActionNullThrows()
        {
            await Sync.ExecuteTaskAsync(null);
        }

        [TestMethod]
        public async Task ExecuteTaskAsyncActionCallsAction()
        {
            // Arrange
            bool called = false;

            // Act
            await Sync.ExecuteTaskAsync(() => { called = true; });

            // Assert
            Assert.IsTrue(called);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task ExecuteTaskAsyncActionThrowsNonTwitterExceptionIfSwallowingDisabled()
        {
            // Arrange
            ExceptionHandler.SwallowWebExceptions = false;

            // Act
            await Sync.ExecuteTaskAsync(() => throw new TestException());
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task ExecuteTaskAsyncActionThrowsNonTwitterExceptionIfSwallowingEnabled()
        {
            // Arrange
            ExceptionHandler.SwallowWebExceptions = true;

            // Act
            await Sync.ExecuteTaskAsync(() => throw new TestException());
        }

        [TestMethod]
        public async Task ExecuteTaskAsyncActionExceptionsWithinActionAvailableOnMainThreadContextIfSwallowingEnabled()
        {
            // Arrange
            ExceptionHandler.SwallowWebExceptions = true;
            ExceptionHandler.ClearLoggedExceptions();
            ITwitterException expectedException = A.Fake<ITwitterException>();
            ITwitterException[] expectedExceptions = new ITwitterException[] { expectedException };

            // Act
            await Sync.ExecuteTaskAsync(() => ExceptionHandler.AddTwitterException(expectedException));

            // Assert
            ITwitterException[] exceptions = ExceptionHandler.GetExceptions().ToArray();
            CollectionAssert.AreEqual(expectedExceptions, exceptions);
        }

        [TestMethod]
        public async Task ExecuteTaskAsyncActionExceptionsWithinActionsAreIndependent()
        {
            // Arrange
            ExceptionHandler.SwallowWebExceptions = true;
            ExceptionHandler.ClearLoggedExceptions();
            ITwitterException expectedException = A.Fake<ITwitterException>();
            ITwitterException[] expectedExceptions = new ITwitterException[] { expectedException };

            // Make one async call to add an exception
            await Sync.ExecuteTaskAsync(() => ExceptionHandler.AddTwitterException(expectedException));

            // Act: Make another call that doesn't add any exception
            await Sync.ExecuteTaskAsync(() => {  });

            // Assert
            ITwitterException[] exceptions = ExceptionHandler.GetExceptions().ToArray();
            CollectionAssert.AreEqual(expectedExceptions, exceptions);
        }

        [ThreadStatic]
        private static string strThreadStatic_ExecuteTaskAsyncActionCallsActionInAnotherThread;

        [TestMethod]
        public async Task ExecuteTaskAsyncActionCallsActionInAnotherThread()
        {
            // Arrange
            strThreadStatic_ExecuteTaskAsyncActionCallsActionInAnotherThread = "a";

            // Act
            await Sync.ExecuteTaskAsync(() =>
                // Assert (can be done within async due to regular exceptions still being thrown)
                Assert.AreNotEqual("a", strThreadStatic_ExecuteTaskAsyncActionCallsActionInAnotherThread));
        }

        [TestMethod]
        public async Task ExecuteTaskAsyncActionCarriesExceptionSwallowingSettingIntoNewThread()
        {
            // Arrange
            const bool expected = true;
            ExceptionHandler.SwallowWebExceptions = expected;

            // Act
            await Sync.ExecuteTaskAsync(() => 
                // Assert
                Assert.AreEqual(expected, ExceptionHandler.SwallowWebExceptions));
        }

        [TestMethod]
        public async Task ExecuteTaskAsyncActionWithNoExceptionHandlerOnCallingThreadStillGetsExceptionHandlerUpdatesFromInnerThread()
        {
            // Arrange
            ITwitterException exception = A.Fake<ITwitterException>();

            // Act
            await Sync.ExecuteTaskAsync(() => ExceptionHandler.AddTwitterException(exception));

            // Assert
            bool hasException = ExceptionHandler.TryPopException(out ITwitterException actual);
            Assert.IsTrue(hasException);
            Assert.AreEqual(exception, actual);
        }

        #endregion

        #region ExecuteTaskAsync<T>(Func<T>)

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ExecuteTaskAsyncFuncNullThrows()
        {
            await Sync.ExecuteTaskAsync<int>(null);
        }

        [TestMethod]
        public async Task ExecuteTaskAsyncFuncCallsFunc()
        {
            // Arrange
            bool called = false;
            const string expectedResult = "dummy value";

            // Act
            string actualResult = await Sync.ExecuteTaskAsync(() =>
            {
                called = true;
                return expectedResult;
            });

            // Assert
            Assert.IsTrue(called);
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public async Task ExecuteTaskAsyncFuncExceptionsWithinActionsAreIndependent()
        {
            // Arrange
            ExceptionHandler.SwallowWebExceptions = true;
            ExceptionHandler.ClearLoggedExceptions();
            ITwitterException expectedException = A.Fake<ITwitterException>();
            ITwitterException[] expectedExceptions = new ITwitterException[] { expectedException };

            // Make one async call to add an exception
            await Sync.ExecuteTaskAsync(() =>
            {
                ExceptionHandler.AddTwitterException(expectedException);
                return 0;
            });

            // Act: Make another call that doesn't add any exception
            await Sync.ExecuteTaskAsync(() => 0);

            // Assert
            ITwitterException[] exceptions = ExceptionHandler.GetExceptions().ToArray();
            CollectionAssert.AreEqual(expectedExceptions, exceptions);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task ExecuteTaskAsyncFuncThrowsNonTwitterExceptionIfSwallowingDisabled()
        {
            // Arrange
            ExceptionHandler.SwallowWebExceptions = false;

            // Act
            await Sync.ExecuteTaskAsync(() =>
            {
                throw new TestException();
#pragma warning disable 162
                return 0;
#pragma warning restore 162
            });
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task ExecuteTaskAsyncFuncThrowsNonTwitterExceptionIfSwallowingEnabled()
        {
            // Arrange
            ExceptionHandler.SwallowWebExceptions = true;

            // Act
            await Sync.ExecuteTaskAsync(() =>
            {
                throw new TestException();
#pragma warning disable 162
                return 0;
#pragma warning restore 162
            });
        }

        [TestMethod]
        public async Task ExecuteTaskAsyncFuncExceptionsWithinFuncAvailableOnMainThreadContextIfSwallowingEnabled()
        {
            // Arrange
            ExceptionHandler.SwallowWebExceptions = true;
            ExceptionHandler.ClearLoggedExceptions();
            ITwitterException expectedException = A.Fake<ITwitterException>();
            ITwitterException[] expectedExceptions = new ITwitterException[] { expectedException };

            // Act
            await Sync.ExecuteTaskAsync(() =>
            {
                ExceptionHandler.AddTwitterException(expectedException);
                return 0;
            });

            // Assert
            ITwitterException[] exceptions = ExceptionHandler.GetExceptions().ToArray();
            CollectionAssert.AreEqual(expectedExceptions, exceptions);
        }

        [ThreadStatic]
        private static string strThreadStatic_ExecuteTaskAsyncFuncCallsFuncInAnotherThread;

        [TestMethod]
        public async Task ExecuteTaskAsyncFuncCallsFuncInAnotherThread()
        {
            // Arrange
            strThreadStatic_ExecuteTaskAsyncFuncCallsFuncInAnotherThread = "a";
            ExceptionHandler.SwallowWebExceptions = false;

            // Act
            await Sync.ExecuteTaskAsync(() =>
            {
                // Assert (can be done within async due to regular exceptions still being thrown)
                Assert.AreNotEqual("a", strThreadStatic_ExecuteTaskAsyncFuncCallsFuncInAnotherThread);
                return 0;
            });
        }

        [TestMethod]
        public async Task ExecuteTaskAsyncFuncCarriesExceptionSwallowingSettingIntoNewThread()
        {
            // Arrange
            const bool expected = true;
            ExceptionHandler.SwallowWebExceptions = expected;

            // Act
            await Sync.ExecuteTaskAsync(() =>
            {
                // Assert
                Assert.AreEqual(expected, ExceptionHandler.SwallowWebExceptions);
                return 0;
            });

        }

        [TestMethod]
        public async Task ExecuteTaskAsyncFuncWithNoExceptionHandlerOnCallingThreadStillGetsExceptionHandlerUpdatesFromInnerThread()
        {
            // Arrange
            ITwitterException exception = A.Fake<ITwitterException>();

            // Act
            await Sync.ExecuteTaskAsync(() =>
            {
                ExceptionHandler.AddTwitterException(exception);

                return 0;
            });

            // Assert
            bool hasException = ExceptionHandler.TryPopException(out ITwitterException actual);
            Assert.IsTrue(hasException);
            Assert.AreEqual(exception, actual);
        }

        #endregion

        #region ExecuteIsolatedTaskAsync(Action)

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ExecuteIsolatedTaskAsyncActionNullThrows()
        {
            await Sync.ExecuteIsolatedTaskAsync(null);
        }

        [TestMethod]
        public async Task ExecuteIsolatedTaskAsyncActionCallsAction()
        {
            // Arrange
            bool called = false;

            // Act
            await Sync.ExecuteIsolatedTaskAsync(() => { called = true; });

            // Assert
            Assert.IsTrue(called);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task ExecuteIsolatedTaskAsyncActionThrowsNonTwitterException()
        {
            await Sync.ExecuteIsolatedTaskAsync(() => throw new TestException());
        }

        [TestMethod]
        public async Task ExecuteIsolatedTaskAsyncActionGetsOwnExecutionContext()
        {
            // Arrange
            AsyncLocal<string> asyncLocal = new AsyncLocal<string>()
            {
                Value = new Fixture().Create<string>()
            };

            // Act
            await Sync.ExecuteIsolatedTaskAsync(() =>
            {
                // Assert
                Assert.IsNull(asyncLocal.Value);
            });
        }

        [TestMethod]
        public async Task ExecuteIsolatedTaskAsyncActionCallsActionInAnotherThread()
        {
            // Arrange
            ThreadLocal<string> threadLocal = new ThreadLocal<string>()
            {
                Value = new Fixture().Create<string>()
            };

            // Act
            await Sync.ExecuteIsolatedTaskAsync(() =>
            {
                // Assert
                Assert.IsNull(threadLocal.Value);
            });
        }

        [TestMethod]
        public async Task ExecuteIsolatedTaskAsyncActionGetsOwnExceptionHandler()
        {
            // Arrange: Ensure we have an Exception Handler on the calling context
            Sync.PrepareForAsync();

            // Act: Use the Exception Handler within ExecuteIsolatedTaskAsync
            await Sync.ExecuteIsolatedTaskAsync(() =>
            {
                ITwitterException exception = A.Fake<ITwitterException>();
                ExceptionHandler.AddTwitterException(exception);
            });

            // Assert
            bool hasException = ExceptionHandler.TryPopException(out _);
            Assert.IsFalse(hasException);
        }

        #endregion

        #region ExecuteIsolatedTaskAsync<T>(Func<T)

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ExecuteIsolatedTaskAsyncFuncNullThrows()
        {
            await Sync.ExecuteIsolatedTaskAsync<int>(null);
        }

        [TestMethod]
        public async Task ExecuteIsolatedTaskAsyncFuncCallsAction()
        {
            // Arrange
            bool called = false;

            // Act
            await Sync.ExecuteIsolatedTaskAsync(() =>
            {
                called = true;
                return 0;
            });

            // Assert
            Assert.IsTrue(called);
        }

        [TestMethod]
        [ExpectedException(typeof(TestException))]
        public async Task ExecuteIsolatedTaskAsyncFuncThrowsNonTwitterException()
        {
            await Sync.ExecuteIsolatedTaskAsync(() =>
            {
                throw new TestException();
#pragma warning disable 162
                return 0;
#pragma warning restore 162
            });
        }

        [TestMethod]
        public async Task ExecuteIsolatedTaskAsyncFuncGetsOwnExecutionContext()
        {
            // Arrange
            AsyncLocal<string> asyncLocal = new AsyncLocal<string>()
            {
                Value = new Fixture().Create<string>()
            };

            // Act
            await Sync.ExecuteIsolatedTaskAsync(() =>
            {
                // Assert
                Assert.IsNull(asyncLocal.Value);
                return 0;
            });
        }

        [TestMethod]
        public async Task ExecuteIsolatedTaskAsyncFuncCallsActionInAnotherThread()
        {
            // Arrange
            ThreadLocal<string> threadLocal = new ThreadLocal<string>()
            {
                Value = new Fixture().Create<string>()
            };

            // Act
            await Sync.ExecuteIsolatedTaskAsync(() =>
            {
                // Assert
                Assert.IsNull(threadLocal.Value);
                return 0;
            });
        }

        [TestMethod]
        public async Task ExecuteIsolatedTaskAsyncFuncGetsOwnExceptionHandler()
        {
            // Arrange: Ensure we have an Exception Handler on the calling context
            Sync.PrepareForAsync();

            // Act: Use the Exception Handler within ExecuteIsolatedTaskAsync
            await Sync.ExecuteIsolatedTaskAsync(() =>
            {
                ITwitterException exception = A.Fake<ITwitterException>();
                ExceptionHandler.AddTwitterException(exception);
                return 0;
            });

            // Assert
            bool hasException = ExceptionHandler.TryPopException(out _);
            Assert.IsFalse(hasException);
        }

        #endregion
    }
}