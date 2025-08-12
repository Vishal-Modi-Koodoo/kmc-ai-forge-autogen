using Microsoft.Playwright;

namespace KMC_Forge_BTL_Core_Agent.Utils
{
    public class CompanyHouseDetailsCapturer
    {
        private readonly string _baseUrl = "https://find-and-update.company-information.service.gov.uk/company/03489004";
        private readonly string[] _tabNames = ["Overview", "Filing History", "People", "Charges"];
        private readonly string _companyNumber;
        private readonly string _companyName = "PARATUS AMC LIMITED";

        public CompanyHouseDetailsCapturer(string companyNumber)
        {
            _companyNumber = companyNumber;
        }
        public async Task<string> CaptureAllTabsAsync()
        {            
            try
            {
                // Create organized folder structure with safe folder names
                var screenshotsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                var companyPath = Path.Combine(screenshotsBasePath, _companyNumber);
                
                Console.WriteLine($"📁 Creating directories...");
                Console.WriteLine($"   Base path: {screenshotsBasePath}");
                Console.WriteLine($"   Company path: {companyPath}");
                
                // Create main directories
                Directory.CreateDirectory(screenshotsBasePath);
                Directory.CreateDirectory(companyPath);
                
                // Create individual tab folders with safe names
                var tabFolders = new Dictionary<string, string>();
                foreach (var tabName in _tabNames)
                {
                    // Create safe folder name by replacing spaces with underscores
                    var safeFolderName = tabName.Replace(" ", "_");
                    var tabFolderPath = Path.Combine(companyPath, safeFolderName);
                    
                    try
                    {
                        Directory.CreateDirectory(tabFolderPath);
                        tabFolders[tabName] = tabFolderPath;
                        Console.WriteLine($"📁 Created folder: {safeFolderName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to create folder {safeFolderName}: {ex.Message}");
                        throw;
                    }
                }
                
                Console.WriteLine($"📁 Created organized folder structure:");
                Console.WriteLine($"   📂 Screenshots/");
                Console.WriteLine($"   📂 └── {_companyNumber}/");
                foreach (var tabName in _tabNames)
                {
                    var safeFolderName = tabName.Replace(" ", "_");
                    Console.WriteLine($"   📂     └── {safeFolderName}/");
                }
                
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });
                
                var page = await browser.NewPageAsync();
                
                Console.WriteLine("🤖 Agent: Navigating to the company information page...");
                await page.GotoAsync(_baseUrl);
                
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
                
                var summary = $"\n🎉 {"success"}\n" +
                             $"📁 Organized folder structure created:\n" +
                             $"   Screenshots/{_companyNumber}/\n" +
                             $"   ├── Overview/\n" +
                             $"   ├── Filing_History/\n" +
                             $"   ├── People/\n" +
                             $"   └── Charges/\n" +
                             $"📄 Summary file created in company folder\n\n" +
                             $"Files saved:\n{string.Join("\n", results.Select(r => $"  {r}"))}";
                Console.WriteLine(summary);
                return summary;
            }
            catch (Exception ex)
            {
                var errorMessage = $"{"error"} Error: {ex.Message}";
                Console.WriteLine(errorMessage);
                return errorMessage;
            }
        }

        public async Task<string> CaptureChargeLinksAsync()
        {
            Console.WriteLine("🤖 Agent: Capturing individual charge links...");
            
            try
            {
                // Create folder structure for charge links with safe names
                var screenshotsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                var companyPath = Path.Combine(screenshotsBasePath, _companyNumber);
                var chargesPath = Path.Combine(companyPath, "Charges");
                var chargeLinksPath = Path.Combine(chargesPath, "Charge_Links");
                
                Console.WriteLine($"📁 Creating charge links directories...");
                Console.WriteLine($"   Base path: {screenshotsBasePath}");
                Console.WriteLine($"   Company path: {companyPath}");
                Console.WriteLine($"   Charges path: {chargesPath}");
                Console.WriteLine($"   Charge links path: {chargeLinksPath}");
                
                try
                {
                    Directory.CreateDirectory(screenshotsBasePath);
                    Directory.CreateDirectory(companyPath);
                    Directory.CreateDirectory(chargesPath);
                    Directory.CreateDirectory(chargeLinksPath);
                    Console.WriteLine("📁 Created charge links folder structure");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to create folders: {ex.Message}");
                    throw;
                }
                
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true
                });
                
                var page = await browser.NewPageAsync();
                
                Console.WriteLine("🤖 Agent: Navigating to the company information page...");
                await page.GotoAsync(_baseUrl);
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                
                // Navigate to Charges tab
                Console.WriteLine("🤖 Agent: Navigating to Charges tab...");
                var chargesTabSelector = "a:has-text('Charges')";
                await page.WaitForSelectorAsync(chargesTabSelector, new PageWaitForSelectorOptions { Timeout = 10000 });
                await page.ClickAsync(chargesTabSelector);
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(2000);
                
                // Find all charge links
                Console.WriteLine("🤖 Agent: Searching for charge links...");
                var chargeLinks = await FindChargeLinks(page);
                
                if (chargeLinks.Count == 0)
                {
                    await browser.CloseAsync();
                    return "❌ No charge links found on the Charges tab.";
                }
                
                Console.WriteLine($"🤖 Agent: Found {chargeLinks.Count} charge links");
                
                var results = new List<string>();
                var linkIndex = 1;
                
                Console.WriteLine($"Charge links: {chargeLinks}");
                foreach (var linkInfo in chargeLinks)
                {
                    try
                    {
                        Console.WriteLine($"🤖 Agent: Capturing charge link {linkIndex}/{chargeLinks.Count}: {linkInfo.Text}");
                        Console.WriteLine($"🔍 Debug: Selector='{linkInfo.Selector}', Href='{linkInfo.Href}'");
                        
                        // Find and click on the charge link using multiple strategies
                        IElementHandle? link = null;
                        
                        // Strategy 1: Try to find by exact text match
                        try
                        {
                            link = await page.QuerySelectorAsync($"a:has-text('{linkInfo.Text}')");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Strategy 1 failed for '{linkInfo.Text}': {ex.Message}");
                        }
                        
                        // Strategy 2: Try to find by partial text match (more flexible)
                        if (link == null)
                        {
                            try
                            {
                                var allLinks = await page.QuerySelectorAllAsync("a");
                                foreach (var potentialLink in allLinks)
                                {
                                    var linkText = await potentialLink.TextContentAsync();
                                    if (!string.IsNullOrEmpty(linkText) && 
                                        linkText.ToLower().Contains(linkInfo.Text.ToLower()))
                                    {
                                        link = potentialLink;
                                        Console.WriteLine($"🔍 Found link using partial text match: '{linkText}' contains '{linkInfo.Text}'");
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Strategy 2 failed for '{linkInfo.Text}': {ex.Message}");
                            }
                        }
                        
                        // Strategy 3: Try to find by href if available
                        if (link == null && !string.IsNullOrEmpty(linkInfo.Href))
                        {
                            try
                            {
                                link = await page.QuerySelectorAsync($"a[href='{linkInfo.Href}']");
                                if (link != null)
                                {
                                    Console.WriteLine($"🔍 Found link using href: {linkInfo.Href}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Strategy 3 failed for '{linkInfo.Text}': {ex.Message}");
                            }
                        }
                        
                        // Strategy 4: Try to find by href contains pattern
                        if (link == null && !string.IsNullOrEmpty(linkInfo.Href))
                        {
                            try
                            {
                                var hrefParts = linkInfo.Href.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
                                if (hrefParts.Length > 0)
                                {
                                    var lastPart = hrefParts[^1]; // Get last part of href
                                    var allLinks = await page.QuerySelectorAllAsync("a");
                                    foreach (var potentialLink in allLinks)
                                    {
                                        var href = await potentialLink.GetAttributeAsync("href");
                                        if (!string.IsNullOrEmpty(href) && href.Contains(lastPart))
                                        {
                                            link = potentialLink;
                                            Console.WriteLine($"🔍 Found link using href pattern: {href} contains {lastPart}");
                                            break;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"⚠️ Strategy 4 failed for '{linkInfo.Text}': {ex.Message}");
                            }
                        }
                        
                        if (link == null)
                        {
                            throw new Exception($"Could not find link for: {linkInfo.Text} after trying all strategies");
                        }
                        
                        // Click on the charge link
                        await link.ClickAsync();
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                        await Task.Delay(2000);
                        
                        // Capture screenshot
                        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        var filename = $"Charge_Link_{linkIndex:00}_{timestamp}.png";
                        var fullPath = Path.Combine(chargeLinksPath, filename);
                        
                        await page.ScreenshotAsync(new PageScreenshotOptions
                        {
                            Path = fullPath,
                            FullPage = true
                        });
                        
                        results.Add($"✅ Charge Link {linkIndex}: {filename}");
                        Console.WriteLine($"✅ Charge Link {linkIndex} captured: {filename}");
                        
                        // Go back to charges tab
                        await page.GoBackAsync();
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                        await Task.Delay(1000);
                        
                        // Verify we're back on the charges tab
                        try
                        {
                            await page.WaitForSelectorAsync("a:has-text('Charges')", new PageWaitForSelectorOptions { Timeout = 5000 });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Warning: May not be back on charges tab: {ex.Message}");
                        }
                        
                        linkIndex++;
                    }
                    catch (Exception ex)
                    {
                        results.Add($"❌ Charge Link {linkIndex}: Failed - {ex.Message}");
                        Console.WriteLine($"❌ Charge Link {linkIndex} failed: {ex.Message}");
                        linkIndex++;
                    }
                }
                
                await browser.CloseAsync();
                
                // Create summary file
                await CreateChargeLinksSummary(chargeLinksPath, results, chargeLinks.Count);
                
                var summary = $"\n🎉 Charge links capture completed!\n" +
                             $"📁 Folder: Screenshots/{_companyNumber}/Charges/Charge_Links/\n" +
                             $"📄 Summary file created\n\n" +
                             $"Results:\n{string.Join("\n", results)}";
                Console.WriteLine(summary);
                return summary;
            }
            catch (Exception ex)
            {
                return $"❌ Error capturing charge links: {ex.Message}";
            }
        }

        private async Task<List<ChargeLinkInfo>> FindChargeLinks(IPage page)
        {
            var chargeLinks = new List<ChargeLinkInfo>();
            
            // Try different selectors to find charge links
            var selectors = new[]
            {
                "a:has-text('Charge code')",
                "a:has-text('charge')",
                "a[href*='charge']",
                "a:has-text('deed')",
                "a:has-text('Deed of charge')",
                "a[href*='deed']"
            };
            
            foreach (var selector in selectors)
            {
                try
                {
                    var links = await page.QuerySelectorAllAsync(selector);
                    foreach (var link in links)
                    {
                        var text = await link.TextContentAsync();
                        var href = await link.GetAttributeAsync("href");
                        
                        // Only add if it's not already in the list and has meaningful content
                        if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(href) && !chargeLinks.Any(existing => existing.Text == text))
                        {
                            // Filter out unwanted links
                            var lowerText = text.ToLower().Trim();
                            var unwantedPatterns = new[]
                            {
                                "follow this company",
                                "file for this company", 
                                "satisfy charge",
                                "tell us what you think of this service",
                                "is there anything wrong with this page?",
                                "feedback",
                                "help us improve",
                                "report a problem",
                                "contact us",
                                "privacy policy",
                                "terms and conditions",
                                "accessibility statement"
                            };
                            
                            var isUnwanted = unwantedPatterns.Any(pattern => lowerText.Contains(pattern));
                            
                            if (!isUnwanted)
                            {
                                chargeLinks.Add(new ChargeLinkInfo { Text = text, Href = href, Selector = selector });
                                Console.WriteLine($"🔗 Found charge link: {text.Trim()} -> {href}");
                            }
                            else
                            {
                                Console.WriteLine($"🚫 Filtered out unwanted link: {text.Trim()}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Selector '{selector}' failed: {ex.Message}");
                }
            }
            
            return chargeLinks;
        }

        private class ChargeLinkInfo
        {
            public string Text { get; set; } = "";
            public string Href { get; set; } = "";
            public string Selector { get; set; } = "";
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
                               $"├── Filing_History/\n" +
                               $"├── People/\n" +
                               $"└── Charges/\n\n" +
                               $"Results:\n{string.Join("\n", results)}\n\n" +
                               $"Files saved in: {companyPath}";
            
            await File.WriteAllTextAsync(summaryPath, summaryContent);
            Console.WriteLine($"📄 Created summary file: capture_summary.txt");
        }

        private async Task CreateChargeLinksSummary(string chargeLinksPath, List<string> results, int totalLinks)
        {
            var summaryPath = Path.Combine(chargeLinksPath, "charge_links_summary.txt");
            var summaryContent = $"Charge Links Capture Summary\n" +
                               $"===========================\n" +
                               $"Company: {_companyName}\n" +
                               $"Company Number: {_companyNumber}\n" +
                               $"Capture Date: {DateTime.Now:yyyy-MM-dd}\n" +
                               $"Capture Time: {DateTime.Now:HH:mm:ss}\n" +
                               $"Target URL: {_baseUrl}\n" +
                               $"Total Links Found: {totalLinks}\n\n" +
                               $"Results:\n{string.Join("\n", results)}\n\n" +
                               $"Files saved in: {chargeLinksPath}";
            
            await File.WriteAllTextAsync(summaryPath, summaryContent);
            Console.WriteLine($"📄 Created charge links summary file: charge_links_summary.txt");
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

        public async Task<string> CaptureAllIncludingChargesAsync()
        {
            Console.WriteLine("🤖 Agent: Processing complete capture request... I'll capture all tabs AND individual charge links.");
            
            try
            {
                // Step 1: Capture all tabs
                Console.WriteLine("\n📋 Step 1: Capturing all tabs...");
                var tabsResult = await CaptureAllTabsAsync();
                
                // Step 2: Capture individual charge links
                Console.WriteLine("\n📋 Step 2: Capturing individual charge links...");
                var chargesResult = await CaptureChargeLinksAsync();
                
                // Get the screenshots folder path
                var screenshotsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
                var companyPath = Path.Combine(screenshotsBasePath, _companyNumber);
                
                Console.WriteLine($"\n🎉 COMPLETE CAPTURE FINISHED!");
                Console.WriteLine($"📁 Screenshots saved in: {companyPath}");
                
                return companyPath;
            }
            catch (Exception ex)
            {
                var errorMessage = $"❌ I encountered an error during complete capture. Error: {ex.Message}";
                Console.WriteLine(errorMessage);
                // Return empty string on error to indicate failure
                return string.Empty;
            }
        }
    }
}