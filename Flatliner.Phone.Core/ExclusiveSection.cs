namespace Flatliner.Phone
{
    public sealed class ExclusiveSection
    {
        private volatile int entryAttempts;

        /// <summary>
        /// Returns true if there was more than one call to TryEnter before exiting
        /// </summary>
        public bool HasContention
        {
            get
            {
                return (this.entryAttempts > 1);
            }
        }
        /// <summary>
        /// Ensures the caller was the first to enter the Exclusive Section (Thread-safe)
        /// </summary>
        /// <returns>Returns True if the Exclusive Section was entered</returns>
        public bool TryEnter()
        {
            this.entryAttempts++;
            return this.entryAttempts == 1;
        }
        /// <summary>
        /// Attempt to exit, useful when exits and enters are called abitrarily,
        /// ensures only one exit ever succeeds.
        /// </summary>
        /// <returns>Returns True if the caller was the last to exit (based on the
        /// number of TryEnter calls that were made)</returns>
        public bool TryExit()
        {
            long result = this.entryAttempts--;
            if (result == 0)
            {
                return true;
            }
            else if (result < 0)
            {
                // Someone beat us to the punch, reset to the counter
                // to Zero to ensure we are able to enter again properly
                ExitClean();
                return false;
            }
            return false;
        }
        /// <summary>
        /// Exits an Exclusive Section immediately regardless of how many calls
        /// had previously been made to TryEnter and returns true if there
        /// was no contention.
        /// </summary>
        /// <returns>Returns True if there was no more than one call to
        /// TryEnter before the call to ExitClean was made</returns>
        public bool ExitClean()
        {
            if (this.entryAttempts == 1)
            {
                this.entryAttempts = 0;
                return true;
            }

            this.entryAttempts = 0;
            return false;
        }
    }
}
