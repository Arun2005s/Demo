namespace Timer.Models
{
    /// <summary>
    /// Stores the result of importing task records from a CSV file.
    /// </summary>
    public class TaskImportResult
    {
        /// <summary>
        /// Gets or sets the number of successfully imported records.
        /// </summary>
        /// <value>
        /// The number of successfully imported records.
        /// </value>
        public int ValidCount { get; set; }

        /// <summary>
        /// Gets or sets the number of records that contain invalid data.
        /// </summary>
        /// <value>
        /// The number of records that contain invalid data.
        /// </value>
        public int InvalidCount { get; set; }

        /// <summary>
        /// Gets or sets the number of duplicate records skipped during import.
        /// </summary>
        /// <value>
        /// The number of duplicate records skipped during import.
        /// </value>
        public int DuplicateCount { get; set; }
    }
}