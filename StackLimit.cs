using System;
using System.Threading;

namespace Lokad.StackLimit
{
    public static class StackLimit
    {
        /// <summary> The lowest address we will tolerate on the stack. </summary>
        /// <remarks>
        ///     Each thread has its own stack, so we keep track of this bound separately 
        ///     for each thread. The default value is zero, which means there is no lower 
        ///     bound at all.
        /// </remarks>
        [ThreadStatic]
        private static ulong MinBound;

        /// <summary>
        ///     Set the stack limit to the number of bytes <paramref name="bytes"/>, relative
        ///     to the current stack position. This limit is checked by <see cref="Check"/>
        /// </summary>
        /// <remarks>
        ///     Overwrites any limits previously set with this call. 
        ///     Only applies to the current thread.
        /// </remarks>
        public unsafe static void SetLimit(int bytes)
        {
            var local = 0;
            int* pointer = &local;

            // The stack expands from higher addresses to lower addresses, so to set a limit
            // relative to the current position, we must subtract the bytes. 
            MinBound = checked((ulong)(IntPtr)pointer - (ulong)bytes);
        }

        /// <summary>
        ///     Throw a <see cref="StackOverflowException"/> if the stack has increased
        ///     beyond the limit set by the last call to <see cref="SetLimit"/>
        /// </summary>
        public unsafe static void Check()
        {
            var local = 0;
            int* pointer = &local;

            // The stack expands from higher addresses to lower addresses, so to set a limit
            // relative to the current position, we must subtract the bytes. 
            if ((ulong)(IntPtr)pointer < MinBound)
                throw new StackOverflowException($"Stack exceeded limit set by Lokad.StackLimit.SetLimit on thread {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
