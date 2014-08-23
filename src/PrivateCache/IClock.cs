namespace Tavis.PrivateCache
{
    using System;

    /// <summary>
    /// Abstracts away the dependency on a real time clock.
    /// Primarily useful for Testing
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// The current time, according to this clock, in universal coordinated time (UTC)
        /// </summary>
        DateTimeOffset UtcNow { get; }
    }
}
