using System.Net.Http;
using System.IO;
using System.Text.Json;

namespace KMC_Forge_BTL_Core_Agent.Utils
{
    public class DocumentRetrievalTool
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public DocumentRetrievalTool()
        {
            _httpClient = new HttpClient();
            _baseUrl = "http://localhost:5000"; // Default API base URL, can be configured
        }

        public DocumentRetrievalTool(string baseUrl)
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl ?? "http://localhost:5000";
        }

        /// <summary>
        /// Calls the RetrieveDocument API to get a document from Azure Blob Storage
        /// </summary>
        /// <param name="documentUri">The URI of the document to retrieve</param>
        /// <returns>The document content as a byte array</returns>
        public async Task<string> RetrieveDocumentAsync(string documentUri)
        {
            try
            {
                // Build the API URL
                var apiUrl = $"{_baseUrl}/api/RetrieveDocument/retrieve?documentUri={Uri.EscapeDataString(documentUri)}";
                
                // Make the HTTP GET request
                var response = await _httpClient.GetAsync(apiUrl);
                
                // Check if the request was successful
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to retrieve document. Status: {response.StatusCode}, Error: {errorContent}");
                }

                // Read the document content
                var documentBytes = await response.Content.ReadAsStringAsync();
                return documentBytes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve document from {documentUri}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Calls the RetrieveDocument API to get a document and returns it as a stream
        /// </summary>
        /// <param name="documentUri">The URI of the document to retrieve</param>
        /// <returns>The document content as a stream</returns>
        public async Task<Stream> RetrieveDocumentAsStreamAsync(string documentUri)
        {
            try
            {
                // Build the API URL
                var apiUrl = $"{_baseUrl}/api/RetrieveDocument/retrieve?documentUri={Uri.EscapeDataString(documentUri)}";
                
                // Make the HTTP GET request
                var response = await _httpClient.GetAsync(apiUrl);
                
                // Check if the request was successful
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to retrieve document. Status: {response.StatusCode}, Error: {errorContent}");
                }

                // Return the document content as a stream
                return await response.Content.ReadAsStreamAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve document from {documentUri}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a document exists using the DocumentExists API
        /// </summary>
        /// <param name="documentUri">The URI of the document to check</param>
        /// <returns>True if the document exists, false otherwise</returns>
        public async Task<bool> DocumentExistsAsync(string documentUri)
        {
            try
            {
                // Build the API URL
                var apiUrl = $"{_baseUrl}/api/RetrieveDocument/exists?documentUri={Uri.EscapeDataString(documentUri)}";
                
                // Make the HTTP GET request
                var response = await _httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<DocumentExistsResponse>(content);
                    return result?.Exists ?? false;
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets document metadata using the GetDocumentMetadata API
        /// </summary>
        /// <param name="documentUri">The URI of the document</param>
        /// <returns>Document metadata</returns>
        public async Task<DocumentMetadata> GetDocumentMetadataAsync(string documentUri)
        {
            try
            {
                // Build the API URL
                var apiUrl = $"{_baseUrl}/api/RetrieveDocument/metadata?documentUri={Uri.EscapeDataString(documentUri)}";
                
                // Make the HTTP GET request
                var response = await _httpClient.GetAsync(apiUrl);
                
                // Check if the request was successful
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get document metadata. Status: {response.StatusCode}, Error: {errorContent}");
                }

                // Parse the metadata response
                var content = await response.Content.ReadAsStringAsync();
                var metadata = JsonSerializer.Deserialize<DocumentMetadata>(content);
                
                if (metadata == null)
                {
                    throw new InvalidOperationException("Failed to deserialize document metadata");
                }
                
                return metadata;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get document metadata for {documentUri}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Downloads a document with validation
        /// </summary>
        /// <param name="documentUri">The URI of the document to download</param>
        /// <returns>The document content as a byte array</returns>
        public async Task<string> DownloadDocumentWithValidationAsync(string documentUri)
        {
            try
            {
                // First check if document exists
                if (!await DocumentExistsAsync(documentUri))
                {
                    throw new FileNotFoundException($"Document not found: {documentUri}");
                }

                // Retrieve the document
                return await RetrieveDocumentAsync(documentUri);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to download document with validation from {documentUri}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Downloads a document and saves it to a local file
        /// </summary>
        /// <param name="documentUri">The URI of the document to download</param>
        /// <param name="localFilePath">The local file path to save the document</param>
        /// <returns>The document content as a byte array</returns>
        public async Task<string> DownloadDocumentToFileAsync(string documentUri, string localFilePath)
        {
            try
            {
                // Retrieve the document
                var documentBytes = await RetrieveDocumentAsync(documentUri);
                
                // Create directory if it doesn't exist
                var directory = Path.GetDirectoryName(localFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Save the document to local file
                await File.WriteAllTextAsync(localFilePath, documentBytes);
                
                return documentBytes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to download document to file from {documentUri}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Downloads a document to a temporary location
        /// </summary>
        /// <param name="documentUri">The URI of the document to download</param>
        /// <param name="fileName">Optional custom filename for the temporary file</param>
        /// <returns>The path to the temporary file</returns>
        public async Task<string> DownloadDocumentToTempFileAsync(string documentUri, string? fileName = null)
        {
            try
            {
                // Generate temporary file path
                var tempFileName = fileName ?? Path.GetFileName(new Uri(documentUri).AbsolutePath);
                if (string.IsNullOrEmpty(tempFileName))
                {
                    tempFileName = $"temp_{Guid.NewGuid()}.pdf";
                }
                
                var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                // Download and save the document
                await DownloadDocumentToFileAsync(documentUri, tempFilePath);

                return tempFilePath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to download document to temp file from {documentUri}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all documents for a specific portfolio
        /// </summary>
        /// <param name="portfolioId">The portfolio identifier</param>
        /// <returns>List of document URIs for the portfolio</returns>
        public async Task<List<string>> GetDocumentsByPortfolioAsync(string portfolioId)
        {
            try
            {
                // Build the API URL
                var apiUrl = $"{_baseUrl}/api/RetrieveDocument/portfolio/{Uri.EscapeDataString(portfolioId)}";
                
                // Make the HTTP GET request
                var response = await _httpClient.GetAsync(apiUrl);
                
                // Check if the request was successful
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get documents for portfolio. Status: {response.StatusCode}, Error: {errorContent}");
                }

                // Parse the response
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PortfolioDocumentsResponse>(content);
                
                return result?.Documents ?? new List<string>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get documents for portfolio {portfolioId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets documents for a specific portfolio and document type
        /// </summary>
        /// <param name="portfolioId">The portfolio identifier</param>
        /// <param name="documentType">The type of document to retrieve</param>
        /// <returns>List of document URIs for the portfolio and document type</returns>
        public async Task<List<string>> GetDocumentsByPortfolioAndTypeAsync(string portfolioId, string documentType)
        {
            try
            {
                // Build the API URL
                var apiUrl = $"{_baseUrl}/api/RetrieveDocument/portfolio/{Uri.EscapeDataString(portfolioId)}/type/{Uri.EscapeDataString(documentType)}";
                
                // Make the HTTP GET request
                var response = await _httpClient.GetAsync(apiUrl);
                
                // Check if the request was successful
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get documents for portfolio and type. Status: {response.StatusCode}, Error: {errorContent}");
                }

                // Parse the response
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PortfolioDocumentsResponse>(content);
                
                return result?.Documents ?? new List<string>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get documents for portfolio {portfolioId} and type {documentType}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Disposes the HTTP client
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    // Response models for API calls
    public class DocumentExistsResponse
    {
        public string DocumentUri { get; set; } = "";
        public bool Exists { get; set; }
    }

    public class DocumentMetadata
    {
        public string DocumentUri { get; set; } = "";
        public string ContentType { get; set; } = "";
        public string FileName { get; set; } = "";
        public bool Exists { get; set; }
    }

    public class PortfolioDocumentsResponse
    {
        public string PortfolioId { get; set; } = "";
        public int DocumentCount { get; set; }
        public List<string> Documents { get; set; } = new List<string>();
    }
} 