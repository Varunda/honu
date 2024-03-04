using System;
using System.Threading.Tasks;

namespace watchtower.Code.ExtensionMethods {

    // a lot of the ideas come from Raymond Chen's "On awaiting a task with a timeout in C#"
    // https://devblogs.microsoft.com/oldnewthing/20220505-00/?p=106585
    public static class TaskExtentionMethods {

        static async Task<T> DelayedResultTask<T>(TimeSpan delay, T result) {
            await Task.Delay(delay);
            return result;
        }

        static async Task<T> DelayedResultTimeoutTask<T>(TimeSpan delay) {
            await Task.Delay(delay);
            throw new TimeoutException();
        }

        /// <summary>
        ///     run a task, and if a delay of <paramref name="delay"/> passes with <paramref name="task"/>
        ///     not completing, instead return <paramref name="fallback"/>
        /// </summary>
        /// <typeparam name="T">parameter type that <paramref name="task"/> will return</typeparam>
        /// <param name="task">extension instance</param>
        /// <param name="delay">how long to delay before returning <paramref name="fallback"/></param>
        /// <param name="fallback">fallback value returned if <paramref name="delay"/> of delay</param>
        /// <returns>
        ///      a task that will contain the result of <paramref name="task"/> if it completed
        ///      within the time span <paramref name="delay"/>, or <paramref name="fallback"/>
        ///      if it took longer than <paramref name="delay"/>
        /// </returns>
        public static async Task<T> TimeoutWithDefault<T>(this Task<T> task, TimeSpan delay, T fallback) {
            // who knew await await was a valid thing
            return await await Task.WhenAny(task, DelayedResultTask<T>(delay, fallback));
        }

        /// <summary>
        ///     run a task, and if a delay of <paramref name="delay"/> passes with <paramref name="task"/>
        ///     not completing, throw a <see cref="TimeoutException"/>
        /// </summary>
        /// <typeparam name="T">parameter type that <paramref name="task"/> will return</typeparam>
        /// <param name="task">extension instance</param>
        /// <param name="delay">how long to delay before throwing a timeout exception</param>
        /// <returns>
        ///      a task that will contain the result of <paramref name="task"/> if it completed
        ///      within the time span <paramref name="delay"/>,
        ///      or a <see cref="TimeoutException"/>
        /// </returns>
        public static async Task<T> TimeoutWithThrow<T>(this Task<T> task, TimeSpan delay) {
            return await await Task.WhenAny(task, DelayedResultTimeoutTask<T>(delay));
        }

    }
}
