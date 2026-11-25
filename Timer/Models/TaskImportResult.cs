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
        public int ValidCount { get; set; }

        /// <summary>
        /// Gets or sets the number of records that contain invalid data.
        /// </summary>
        public int InvalidCount { get; set; }

        /// <summary>
        /// Gets or sets the number of duplicate records skipped during import.
        /// </summary>
        public int DuplicateCount { get; set; }
    }
}
