using Microsoft.Playwright;
using System.Text.Json;

namespace KMC_Forge_BTL_API
{
    public class ScreenshotAgent
    {
        private readonly string _baseUrl = "https://find-and-update.company-information.service.gov.uk/company/";
        private readonly string[] _tabNames = ["Overview", "Filing History", "People", "Charges"];
        private readonly string _companyNumber = "03489004";
        private readonly string _companyName = "PARATUS AMC LIMITED";
        
        // Agent prompts and responses
        private readonly Dictionary<string, string> _prompts = new()
        {
            ["greeting"] = "🤖 Hello! I'm your Screenshot Agent. I can capture screenshots of the 4 tabs on the company information page: Overview, Filing History, People, and Charges. How can I help you today?",
            ["ready"] = "🤖 I'm ready to capture screenshots. Just say 'capture' or 'take screenshots' to begin.",
            ["processing"] = "🤖 Processing your request... I'll navigate to the page and capture screenshots of all 4 tabs.",
            ["success"] = "🎉 Screenshot capture completed successfully! All tabs have been captured and saved.",
            ["error"] = "❌ I encountered an error while capturing screenshots. Please try again.",
            ["help"] = "📋 I can help you with:\n• Capturing screenshots of all 4 tabs\n• Individual tab screenshots\n• Status updates\n\nCommands: 'capture', 'help', 'status', 'exit'"
        };

        public async Task<string> ProcessCommandAsync(string command)
        {
            var lowerCommand = command.ToLower().Trim();
            
            return lowerCommand switch
            {
                "capture" or "take screenshots" or "screenshot" => await CaptureAllTabsAsync(),
                "help" or "?" => _prompts["help"],
                "status" => await GetStatusAsync(),
                "greeting" => _prompts["greeting"],
                _ => "🤖 I didn't understand that command. Type 'help' for available options."
            };
        }

        public async Task<string> CaptureAllTabsAsync()
        {
            Console.WriteLine(_prompts["processing"]);
            
            try
            {
                // Create organized folder structure
                var screenshotsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                var companyPath = Path.Combine(screenshotsBasePath, _companyNumber);
                
                // Create main directories
                Directory.CreateDirectory(screenshotsBasePath);
                Directory.CreateDirectory(companyPath);
                
                // Create individual tab folders
                var tabFolders = new Dictionary<string, string>();
                foreach (var tabName in _tabNames)
                {
                    var tabFolderPath = Path.Combine(companyPath, tabName);
                    Directory.CreateDirectory(tabFolderPath);
                    tabFolders[tabName] = tabFolderPath;
                }
                
                Console.WriteLine($"📁 Created organized folder structure:");
                Console.WriteLine($"   📂 Screenshots/");
                Console.WriteLine($"   📂 └── {_companyNumber}/");
                foreach (var tabName in _tabNames)
                {
                    Console.WriteLine($"   📂     └── {tabName}/");
                }
                
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });
                
                var page = await browser.NewPageAsync();
                
                Console.WriteLine("🤖 Agent: Navigating to the company information page...");
                await page.GotoAsync(_baseUrl+_companyNumber);
                
                // Wait for the page to load
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                Console.WriteLine("✅ Page loaded successfully");
                
                var results = new List<string>();
                
                foreach (var tabName in _tabNames)
                {
                    try
                    {
                        Console.WriteLine($"🤖 Agent: Capturing {tabName} tab...");
                        var screenshotPath = await CaptureTabScreenshotAsync(page, tabName, tabFolders[tabName]);
                        results.Add($"✅ {tabName}: {screenshotPath}");
                        Console.WriteLine($"✅ {tabName} captured: {screenshotPath}");
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"❌ {tabName}: Failed - {ex.Message}";
                        results.Add(errorMsg);
                        Console.WriteLine(errorMsg);
                    }
                }
                
                await browser.CloseAsync();
                
                // Create summary file in company folder
                await CreateSummaryFile(companyPath, results);
                
                var summary = $"\n🎉 {_prompts["success"]}\n" +
                             $"📁 Organized folder structure created:\n" +
                             $"   Screenshots/{_companyNumber}/\n" +
                             $"   ├── Overview/\n" +
                             $"   ├── Filing History/\n" +
                             $"   ├── People/\n" +
                             $"   └── Charges/\n" +
                             $"📄 Summary file created in company folder\n\n" +
                             $"Files saved:\n{string.Join("\n", results.Select(r => $"  {r}"))}";
                Console.WriteLine(summary);
                return summary;
            }
            catch (Exception ex)
            {
                var errorMessage = $"{_prompts["error"]} Error: {ex.Message}";
                Console.WriteLine(errorMessage);
                return errorMessage;
            }
        }

        private async Task CreateSummaryFile(string companyPath, List<string> results)
        {
            var summaryPath = Path.Combine(companyPath, "capture_summary.txt");
            var summaryContent = $"Screenshot Capture Summary\n" +
                               $"========================\n" +
                               $"Company: {_companyName}\n" +
                               $"Company Number: {_companyNumber}\n" +
                               $"Capture Date: {DateTime.Now:yyyy-MM-dd}\n" +
                               $"Capture Time: {DateTime.Now:HH:mm:ss}\n" +
                               $"Target URL: {_baseUrl}\n\n" +
                               $"Folder Structure:\n" +
                               $"Screenshots/{_companyNumber}/\n" +
                               $"├── Overview/\n" +
                               $"├── Filing History/\n" +
                               $"├── People/\n" +
                               $"└── Charges/\n\n" +
                               $"Results:\n{string.Join("\n", results)}\n\n" +
                               $"Files saved in: {companyPath}";
            
            await File.WriteAllTextAsync(summaryPath, summaryContent);
            Console.WriteLine($"📄 Created summary file: capture_summary.txt");
        }

        private async Task<string> CaptureTabScreenshotAsync(IPage page, string tabName, string tabFolderPath)
        {
            // Click on the specific tab using the working selector
            var tabSelector = $"a:has-text('{tabName}')";
            await page.WaitForSelectorAsync(tabSelector, new PageWaitForSelectorOptions { Timeout = 10000 });
            await page.ClickAsync(tabSelector);
            Console.WriteLine($"✅ Clicked tab '{tabName}'");
            
            // Wait for the tab content to load
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Task.Delay(2000); // Additional wait for animations
            
            // Generate filename with timestamp
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"{tabName.Replace(" ", "_")}_{timestamp}.png";
            var fullPath = Path.Combine(tabFolderPath, filename);
            
            // Capture screenshot
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = fullPath,
                FullPage = true
            });
            
            return filename; // Return relative path for display
        }
        
        public Task<string> GetStatusAsync()
        {
            var status = $"🤖 Screenshot Agent Status:\n" +
                        $"• Ready: ✅\n" +
                        $"• Company: {_companyName}\n" +
                        $"• Company Number: {_companyNumber}\n" +
                        $"• Target URL: {_baseUrl}\n" +
                        $"• Tabs to capture: {string.Join(", ", _tabNames)}\n" +
                        $"• Last check: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            return Task.FromResult(status);
        }

        public string GetGreeting()
        {
            return _prompts["greeting"];
        }
    }
} 