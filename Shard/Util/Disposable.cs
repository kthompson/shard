using System;
using System.Threading;

namespace Shard.Util
{
    /// <summary>
    /// Provides a set of static methods for creating Disposables.
    /// </summary>
    public static class Disposable
    {
        /// <summary>
        /// Gets the disposable that does nothing when disposed.
        /// </summary>
        public static IDisposable Empty
        {
            get
            {
                return DefaultDisposable.Instance;
            }
        }
        /// <summary>
        /// Creates a disposable object that invokes the specified action when disposed.
        /// </summary>
        /// <param name="dispose">Action to run during the first call to <see cref="M:System.IDisposable.Dispose" />. The action is guaranteed to be run at most once.</param>
        /// <returns>The disposable object that runs the given action upon disposal.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="dispose" /> is null.</exception>
        public static IDisposable Create(Action dispose)
        {
            if (dispose == null)
            {
                throw new ArgumentNullException("dispose");
            }
            return new AnonymousDisposable(dispose);
        }

        class AnonymousDisposable : IDisposable
        {
            private Action _dispose;

            /// <summary>
            /// Constructs a new disposable with the given action used for disposal.
            /// </summary>
            /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
            public AnonymousDisposable(Action dispose)
            {
                this._dispose = dispose;
            }

            /// <summary>
            /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
            /// </summary>
            public void Dispose()
            {
                var action = Interlocked.Exchange(ref this._dispose, null);
                if (action != null)
                {
                    action();
                }
            }
        }

        class DefaultDisposable : IDisposable
        {
            /// <summary>
            /// Singleton default disposable.
            /// </summary>
            public static readonly DefaultDisposable Instance = new DefaultDisposable();

            private DefaultDisposable()
            {
            }
            /// <summary>
            /// Does nothing.
            /// </summary>
            public void Dispose()
            {
            }
        }
    }
}