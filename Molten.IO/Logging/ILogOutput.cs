﻿namespace Molten
{
    public interface ILogOutput : IDisposable
    {
        /// <summary>Writes the specified text to the log output.</summary>
        /// <param name="text">The text to be added to the current log position.</param>
        /// <param name="entry">The entry representing the full line of text that has been written so far, including the additional <paramref name="text"/></param>
        /// <param name="timestamp">If true, a timestamp will be written.</param>
        void Write(string text, Logger.Entry entry, bool timestamp = true);

        /// <summary>Clears the log output.</summary>
        void Clear();
    }
}
