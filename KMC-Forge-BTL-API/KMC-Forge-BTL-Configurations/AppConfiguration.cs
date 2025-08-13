using Microsoft.Extensions.Configuration;

namespace KMC_Forge_BTL_Configurations
{
    public class AppConfiguration
    {
        private static AppConfiguration _instance;
        private static readonly object _lock = new object();
        private readonly IConfiguration _configuration;

        private AppConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static AppConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            throw new InvalidOperationException("AppConfiguration has not been initialized. Call Initialize() first.");
                        }
                    }
                }
                return _instance;
            }
        }

        public static void Initialize(IConfiguration configuration)
        {
            lock (_lock)
            {
                _instance = new AppConfiguration(configuration);
            }
        }

        // Azure OpenAI Configuration
        public string AzureOpenAIEndpoint => _configuration["AzureOpenAI:Endpoint"] ?? "https://kmc-ai-forge.openai.azure.com/";
        public string AzureOpenAIApiKey => _configuration["AzureOpenAI:ApiKey"] ?? throw new ArgumentNullException("AzureOpenAI:ApiKey");
        public string AzureOpenAIModel => _configuration["AzureOpenAI:Model"] ?? "gpt-4.1";

        // Azure Blob Storage Configuration
        public string AzureBlobStorageConnectionString => _configuration["AzureBlobStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureBlobStorage:ConnectionString");
        public string AzureBlobStorageContainerName => _configuration["AzureBlobStorage:ContainerName"] ?? throw new ArgumentNullException("AzureBlobStorage:ContainerName");

        // OpenAI Configuration (if using OpenAI directly)
        public string OpenAIApiKey => _configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey");

        // Application Configuration
        public string AllowedHosts => _configuration["AllowedHosts"] ?? "*";
        public string Environment => _configuration["Environment"] ?? "Development";

        // Logging Configuration
        public string DefaultLogLevel => _configuration["Logging:LogLevel:Default"] ?? "Information";
        public string MicrosoftAspNetCoreLogLevel => _configuration["Logging:LogLevel:Microsoft.AspNetCore"] ?? "Warning";

        // File Paths
        public string PdfDataExtractorPromptPath => _configuration["FilePaths:PdfDataExtractorPrompt"] ?? "Prompts/PDFDataExtractorPrompt.txt";
        public string ImageDataExtractorPromptPath => _configuration["FilePaths:ImageDataExtractorPrompt"] ?? "Prompts/ImageDataExtractorPrompt.txt";
        public string DocumentIdentifierPromptPath => _configuration["FilePaths:DocumentIdentifierPrompt"] ?? "Prompts/DocumentIdentifierPrompt.txt";
        public string PortfolioValidatorPrompt => _configuration["FilePaths:PortfolioValidatorPrompt"] ?? "Prompts/PortfolioValidatorPrompt.txt";

        // Retry Configuration
        public int MaxRetries => int.TryParse(_configuration["RetrySettings:MaxRetries"], out int maxRetries) ? maxRetries : 3;
        public int RetryDelayMs => int.TryParse(_configuration["RetrySettings:RetryDelayMs"], out int delay) ? delay : 2000;
    }
}
