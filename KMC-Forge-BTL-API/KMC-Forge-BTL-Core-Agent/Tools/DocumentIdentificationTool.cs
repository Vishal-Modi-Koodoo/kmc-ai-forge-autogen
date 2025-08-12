using AutoGen;
using AutoGen.Core;
using AutoGen.OpenAI;
using KMC_Forge_BTL_Configurations;
using KMC_Forge_BTL_Core_Agent.Utils;
using KMC_Forge_BTL_Models.DocumentIdentificationResponse;
using KMC_Forge_BTL_Models.Enums;

namespace KMC_Forge_BTL_Core_Agent.Tools
{
    /// <summary>
    /// Tool for identifying document types using LLM Model
    /// </summary>
    public class DocumentIdentificationTool
    {
        private readonly MiddlewareStreamingAgent<OpenAIChatAgent> _documentIdentificationAgent;
        private readonly AppConfiguration _config;

        public DocumentIdentificationTool(MiddlewareStreamingAgent<OpenAIChatAgent> documentIdentificationAgent)
        {
            _documentIdentificationAgent = documentIdentificationAgent;
            _config = AppConfiguration.Instance;
        }

        /// <summary>
        /// Identifies the type of document from the provided content
        /// </summary>
        /// <param name="documentContent">The text content of the document to identify</param>
        /// <returns>Document identification result</returns>
        public async Task<DocumentIdentificationResult> IdentifyDocumentTypeAsync(string documentContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(documentContent))
                {
                    return new DocumentIdentificationResult
                    {
                        DocumentType = DocumentType.Unknown,
                        Confidence = 0.0,
                        IsSuccessful = false,
                        ErrorMessage = "Document content is empty or null"
                    };
                }

                var userProxy = new UserProxyAgent(
                    name: "user",
                    systemMessage: "check the values",
                    defaultReply: "Thank you for the output",
                    humanInputMode: HumanInputMode.NEVER)
                    .RegisterPrintMessage();

                Console.WriteLine("\nAnalyzing document content for type identification...\n");

                // Retry logic with exponential backoff using configuration
                int maxRetries = _config.MaxRetries;
                int currentRetry = 0;
                
                while (currentRetry < maxRetries)
                {
                    try
                    {
                        var messages = await userProxy.InitiateChatAsync(
                            receiver: _documentIdentificationAgent,
                            message: documentContent,
                            maxRound: 1);

                        string aiJson = "";
                        foreach (var message in messages)
                        {
                            if (message is TextMessage textMessage && textMessage.Role == Role.Assistant)
                            {
                                aiJson = textMessage.Content;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(aiJson))
                        {
                            return new DocumentIdentificationResult
                            {
                                DocumentType = DocumentType.Unknown,
                                Confidence = 0.0,
                                IsSuccessful = false,
                                ErrorMessage = "No response received from LLM"
                            };
                        }

                        return ParseLLMResponse(aiJson);
                    }
                    catch (Exception ex) when (ex.Message.Contains("rate limit") || ex.Message.Contains("429"))
                    {
                        currentRetry++;
                        if (currentRetry >= maxRetries)
                        {
                            Console.WriteLine($"Rate limit exceeded after {maxRetries} retries. Returning unknown result.");
                            return new DocumentIdentificationResult
                            {
                                DocumentType = DocumentType.Unknown,
                                Confidence = 0.0,
                                IsSuccessful = false,
                                ErrorMessage = "Rate limit exceeded"
                            };
                        }
                        
                        int delayMs = (int)Math.Pow(2, currentRetry) * _config.RetryDelayMs;
                        Console.WriteLine($"Rate limit hit. Retrying in {delayMs/1000} seconds... (Attempt {currentRetry}/{maxRetries})");
                        await Task.Delay(delayMs);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected error: {ex.Message}");
                        return new DocumentIdentificationResult
                        {
                            DocumentType = DocumentType.Unknown,
                            Confidence = 0.0,
                            IsSuccessful = false,
                            ErrorMessage = $"Error during document identification: {ex.Message}"
                        };
                    }
                }

                return new DocumentIdentificationResult
                {
                    DocumentType = DocumentType.Unknown,
                    Confidence = 0.0,
                    IsSuccessful = false,
                    ErrorMessage = "Failed to identify document type after all retries"
                };
            }
            catch (Exception ex)
            {
                return new DocumentIdentificationResult
                {
                    DocumentType = DocumentType.Unknown,
                    Confidence = 0.0,
                    IsSuccessful = false,
                    ErrorMessage = $"Error during document identification: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Identifies the type of document from a file path (PDF, etc.)
        /// </summary>
        /// <param name="filePath">Path to the document file</param>
        /// <returns>Document identification result</returns>
        public async Task<DocumentIdentificationResult> IdentifyDocumentTypeFromFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    return new DocumentIdentificationResult
                    {
                        DocumentType = DocumentType.Unknown,
                        Confidence = 0.0,
                        IsSuccessful = false,
                        ErrorMessage = "File does not exist or path is invalid"
                    };
                }

                string extractedText = "";
                string fileExtension = Path.GetExtension(filePath).ToLower();

                // Extract text based on file type
                switch (fileExtension)
                {
                    case ".pdf":
                        extractedText = PdfExtractor.ExtractTextFromPdf(filePath);
                        break;
                    case ".txt":
                        extractedText = await File.ReadAllTextAsync(filePath);
                        break;
                    default:
                        return new DocumentIdentificationResult
                        {
                            DocumentType = DocumentType.Unknown,
                            Confidence = 0.0,
                            IsSuccessful = false,
                            ErrorMessage = $"Unsupported file type: {fileExtension}"
                        };
                }

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    return new DocumentIdentificationResult
                    {
                        DocumentType = DocumentType.Unknown,
                        Confidence = 0.0,
                        IsSuccessful = false,
                        ErrorMessage = "No text could be extracted from the document"
                    };
                }

                return await IdentifyDocumentTypeAsync(extractedText);
            }
            catch (Exception ex)
            {
                return new DocumentIdentificationResult
                {
                    DocumentType = DocumentType.Unknown,
                    Confidence = 0.0,
                    IsSuccessful = false,
                    ErrorMessage = $"Error processing file: {ex.Message}"
                };
            }
        }

        private DocumentIdentificationResult ParseLLMResponse(string llmResponse)
        {
            try
            {
                // Clean the response - remove any extra whitespace and quotes
                var cleanResponse = llmResponse.Trim().Trim('"', '\'', '`');
                
                if (string.IsNullOrWhiteSpace(cleanResponse))
                {
                    return new DocumentIdentificationResult
                    {
                        DocumentType = DocumentType.Unknown,
                        Confidence = 0.0,
                        IsSuccessful = false,
                        ErrorMessage = "Empty response from LLM"
                    };
                }

                // Try to parse the document type directly from the string
                if (Enum.TryParse<DocumentType>(cleanResponse, true, out var documentType))
                {
                    return new DocumentIdentificationResult
                    {
                        DocumentType = documentType,
                        Confidence = 0.8, // Default confidence for successful parsing
                        Reasoning = $"Document type identified as: {documentType}",
                        IsSuccessful = true
                    };
                }
                else
                {
                    // If direct parsing fails, try to extract document type from common patterns
                    var lowerResponse = cleanResponse.ToLower();
                    
                    // Map common document type descriptions to enum values
                    DocumentType mappedType = DocumentType.Unknown;
                    double confidence = 0.6; // Lower confidence for mapped types
                    
                    if (lowerResponse.Contains("application form") || lowerResponse.Contains("applicationform"))
                        mappedType = DocumentType.ApplicationForm;
                    else if (lowerResponse.Contains("portfolio form") || lowerResponse.Contains("portfolioform"))
                        mappedType = DocumentType.PortfolioForm;
                    else if (lowerResponse.Contains("credit search") || lowerResponse.Contains("creditsearch") || lowerResponse.Contains("equifax"))
                        mappedType = DocumentType.CreditSearchForm;
                    else if (lowerResponse.Contains("mortgage statement") || lowerResponse.Contains("mortgagestatement"))
                        mappedType = DocumentType.MortgageStatement;
                    else if (lowerResponse.Contains("asts") || lowerResponse.Contains("assured shorthold tenancy"))
                        mappedType = DocumentType.ASTS;
                    else if (lowerResponse.Contains("unknown") || lowerResponse.Contains("unrecognized"))
                        mappedType = DocumentType.Unknown;
                    
                    return new DocumentIdentificationResult
                    {
                        DocumentType = mappedType,
                        Confidence = confidence,
                        Reasoning = $"Document type mapped from response: '{cleanResponse}' to {mappedType}",
                        IsSuccessful = mappedType != DocumentType.Unknown
                    };
                }
            }
            catch (Exception ex)
            {
                return new DocumentIdentificationResult
                {
                    DocumentType = DocumentType.Unknown,
                    Confidence = 0.0,
                    IsSuccessful = false,
                    ErrorMessage = $"Error parsing LLM response: {ex.Message}"
                };
            }
        }
    }
}
