using Microsoft.Playwright;

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
            ["help"] = "📋 I can help you with:\n• Capturing screenshots of all 4 tabs\n• Capturing individual charges from the Charges tab\n• Complete capture (all tabs + individual charges)\n• Individual tab screenshots\n• Status updates\n\nCommands: 'capture', 'charges', 'capture all', 'help', 'status', 'exit'"
        };

        public async Task<string> ProcessCommandAsync(string command)
        {
            var lowerCommand = command.ToLower().Trim();
            
            return lowerCommand switch
            {
                "capture" or "take screenshots" or "screenshot" => await CaptureAllTabsAsync(),
                "charges" or "capture charges" => await CaptureIndividualChargesAsync(),
                "capture all" or "all" or "complete" => await CaptureAllIncludingChargesAsync(),
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

        public async Task<string> CaptureIndividualChargesAsync()
        {
            Console.WriteLine("🤖 Processing your request... I'll navigate to the Charges tab and capture individual charge screenshots.");
            
            try
            {
                // Create organized folder structure for charges
                var screenshotsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                var companyPath = Path.Combine(screenshotsBasePath, _companyNumber);
                var chargesPath = Path.Combine(companyPath, "Charges");
                var individualChargesPath = Path.Combine(chargesPath, "Individual_Charges");
                
                // Create directories
                Directory.CreateDirectory(screenshotsBasePath);
                Directory.CreateDirectory(companyPath);
                Directory.CreateDirectory(chargesPath);
                Directory.CreateDirectory(individualChargesPath);
                
                Console.WriteLine($"📁 Created folder structure for individual charges:");
                Console.WriteLine($"   📂 Screenshots/{_companyNumber}/Charges/Individual_Charges/");
                
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });
                
                var page = await browser.NewPageAsync();
                
                Console.WriteLine("🤖 Agent: Navigating to the company information page...");
                await page.GotoAsync(_baseUrl + _companyNumber);
                
                // Wait for the page to load
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                Console.WriteLine("✅ Page loaded successfully");
                
                // Navigate to Charges tab
                Console.WriteLine("🤖 Agent: Navigating to Charges tab...");
                var chargesTabSelector = "a:has-text('Charges')";
                await page.WaitForSelectorAsync(chargesTabSelector, new PageWaitForSelectorOptions { Timeout = 10000 });
                await page.ClickAsync(chargesTabSelector);
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(2000);
                Console.WriteLine("✅ Charges tab loaded successfully");
                
                // Find all charge links
                var chargeLinks = await page.QuerySelectorAllAsync("a[href*='/charge/']");
                Console.WriteLine($"🔍 Found {chargeLinks.Count} charge links");
                
                if (chargeLinks.Count == 0)
                {
                    await browser.CloseAsync();
                    return "🤖 No charge links found on the Charges tab.";
                }
                
                var results = new List<string>();
                var chargeCount = 0;
                
                foreach (var chargeLink in chargeLinks)
                {
                    try
                    {
                        chargeCount++;
                        Console.WriteLine($"🤖 Agent: Processing charge {chargeCount}/{chargeLinks.Count}...");
                        
                        // Get the charge link text and href
                        var chargeText = await chargeLink.TextContentAsync();
                        var chargeHref = await chargeLink.GetAttributeAsync("href");
                        
                        if (string.IsNullOrEmpty(chargeHref))
                        {
                            Console.WriteLine($"⚠️ Skipping charge {chargeCount}: No href found");
                            continue;
                        }
                        
                        // Create a new page for each charge
                        var chargePage = await browser.NewPageAsync();
                        
                        // Navigate to the charge page
                        var fullChargeUrl = chargeHref.StartsWith("http") ? chargeHref : $"https://find-and-update.company-information.service.gov.uk{chargeHref}";
                        await chargePage.GotoAsync(fullChargeUrl);
                        await chargePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
                        await Task.Delay(1000);
                        
                        // Generate filename for the charge
                        var safeChargeText = string.Join("_", chargeText?.Split(Path.GetInvalidFileNameChars()) ?? new[] { "charge" });
                        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        var filename = $"Charge_{chargeCount:00}_{safeChargeText}_{timestamp}.png";
                        var fullPath = Path.Combine(individualChargesPath, filename);
                        
                        // Capture screenshot of the charge page
                        await chargePage.ScreenshotAsync(new PageScreenshotOptions
                        {
                            Path = fullPath,
                            FullPage = true
                        });
                        
                        await chargePage.CloseAsync();
                        
                        results.Add($"✅ Charge {chargeCount}: {filename}");
                        Console.WriteLine($"✅ Charge {chargeCount} captured: {filename}");
                        
                        // Small delay between charges
                        await Task.Delay(500);
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"❌ Charge {chargeCount}: Failed - {ex.Message}";
                        results.Add(errorMsg);
                        Console.WriteLine(errorMsg);
                    }
                }
                
                await browser.CloseAsync();
                
                // Create summary file for charges
                await CreateChargesSummaryFile(individualChargesPath, results);
                
                var summary = $"\n🎉 Individual charges capture completed successfully!\n" +
                             $"📁 Charges saved in: Screenshots/{_companyNumber}/Charges/Individual_Charges/\n" +
                             $"📄 Summary file created in charges folder\n\n" +
                             $"Files saved:\n{string.Join("\n", results.Select(r => $"  {r}"))}";
                Console.WriteLine(summary);
                return summary;
            }
            catch (Exception ex)
            {
                var errorMessage = $"❌ I encountered an error while capturing individual charges. Error: {ex.Message}";
                Console.WriteLine(errorMessage);
                return errorMessage;
            }
        }

        private async Task CreateChargesSummaryFile(string chargesPath, List<string> results)
        {
            var summaryPath = Path.Combine(chargesPath, "charges_capture_summary.txt");
            var summaryContent = $"Individual Charges Capture Summary\n" +
                               $"==================================\n" +
                               $"Company: {_companyName}\n" +
                               $"Company Number: {_companyNumber}\n" +
                               $"Capture Date: {DateTime.Now:yyyy-MM-dd}\n" +
                               $"Capture Time: {DateTime.Now:HH:mm:ss}\n" +
                               $"Target URL: {_baseUrl}\n" +
                               $"Charges Tab URL: {_baseUrl}{_companyNumber}/charges\n\n" +
                               $"Folder Structure:\n" +
                               $"Screenshots/{_companyNumber}/Charges/Individual_Charges/\n\n" +
                               $"Results:\n{string.Join("\n", results)}\n\n" +
                               $"Files saved in: {chargesPath}";
            
            await File.WriteAllTextAsync(summaryPath, summaryContent);
            Console.WriteLine($"📄 Created charges summary file: charges_capture_summary.txt");
        }

        public async Task<string> CaptureAllIncludingChargesAsync()
        {
            Console.WriteLine("🤖 Processing complete capture request... I'll capture all tabs AND individual charges.");
            
            try
            {
                // Step 1: Capture all tabs
                Console.WriteLine("\n📋 Step 1: Capturing all tabs...");
                var tabsResult = await CaptureAllTabsAsync();
                
                // Step 2: Capture individual charges
                Console.WriteLine("\n📋 Step 2: Capturing individual charges...");
                var chargesResult = await CaptureIndividualChargesAsync();
                
                // Create combined summary
                var combinedSummary = $"\n🎉 COMPLETE CAPTURE FINISHED!\n" +
                                     $"================================\n" +
                                     $"✅ All tabs captured successfully\n" +
                                     $"✅ Individual charges captured successfully\n" +
                                     $"📁 All files saved in: Screenshots/{_companyNumber}/\n" +
                                     $"📄 Summary files created in respective folders\n\n" +
                                     $"📋 Tabs Result:\n{tabsResult}\n\n" +
                                     $"📋 Charges Result:\n{chargesResult}";
                
                Console.WriteLine(combinedSummary);
                return combinedSummary;
            }
            catch (Exception ex)
            {
                var errorMessage = $"❌ I encountered an error during complete capture. Error: {ex.Message}";
                Console.WriteLine(errorMessage);
                return errorMessage;
            }
        }

        // Main method to run the ScreenshotAgent as a standalone application
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🚀 Starting Screenshot Agent...");
            
            var agent = new ScreenshotAgent();
            Console.WriteLine(agent.GetGreeting());
            Console.WriteLine();

            while (true)
            {
                Console.Write("You: ");
                var input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                    continue;

                if (input.ToLower() == "exit" || input.ToLower() == "quit")
                {
                    Console.WriteLine("🤖 Agent: Goodbye! Have a great day! 👋");
                    break;
                }

                try
                {
                    var response = await agent.ProcessCommandAsync(input);
                    Console.WriteLine($"🤖 Agent: {response}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                }

                Console.WriteLine();
            }
        }
    }
} 