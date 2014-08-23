namespace Tavis.PrivateCache
{
    using System;

    /// <summary>
    /// A trivial implementation of <see cref="IClock"/> that uses <c>DateTimeOffset.UtcNow</c>
    /// </summary>
    public class RealClock : IClock
    {
        /// <inheritdoc cref="IClock.UtcNow"/>
        public virtual DateTimeOffset UtcNow
        {
            get { return DateTimeOffset.UtcNow; }
        }
    }
}
