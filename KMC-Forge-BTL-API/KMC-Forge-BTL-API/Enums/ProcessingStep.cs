namespace KMC_Forge_BTL_API.Enums
{
    /// <summary>
    /// Enum for document processing steps
    /// </summary>
    public enum ProcessingStep
    {
        /// <summary>
        /// Initial upload and validation
        /// </summary>
        DocumentValidation,
        
        /// <summary>
        /// Portfolio form processing
        /// </summary>
        PortfolioCompletion,
        
        /// <summary>
        /// Company house validation
        /// </summary>
        CompanyHouseValidation,
        
        /// <summary>
        /// Final processing and completion
        /// </summary>
        ProcessingComplete
    }
}
