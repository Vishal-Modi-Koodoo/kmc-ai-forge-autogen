namespace KMC_Forge_BTL_API.Enums
{
    /// <summary>
    /// Enum for processing status values
    /// </summary>
    public enum ProcessingStatus
    {
        /// <summary>
        /// Processing is in progress
        /// </summary>
        InProgress,
        
        /// <summary>
        /// Processing completed successfully
        /// </summary>
        Success,
        
        /// <summary>
        /// Processing completed with warnings/alerts
        /// </summary>
        Alert,
        
        /// <summary>
        /// Processing failed
        /// </summary>
        Failure
    }
}
