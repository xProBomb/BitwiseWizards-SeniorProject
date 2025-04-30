using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace TestTrustTrade.Reqnroll.Services
{
    /// <summary>
    /// Singleton class to manage a single instance of WebDriver across test runs
    /// </summary>
    public class WebDriverSingleton
    {
        private static IWebDriver _instance;
        private static readonly object _lock = new object();
        private static int _referenceCount = 0;
        private static readonly string _baseUrl = "http://localhost:5102";

        /// <summary>
        /// Gets the base URL for the application
        /// </summary>
        public static string BaseUrl => _baseUrl;

        /// <summary>
        /// Gets the singleton instance of WebDriver
        /// </summary>
        public static IWebDriver Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            InitializeDriver();
                        }
                    }
                }
                _referenceCount++;
                return _instance;
            }
        }

        /// <summary>
        /// Initializes the WebDriver with Chrome options
        /// </summary>
        private static void InitializeDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-extensions");
            options.AddArguments("--incognito");

            _instance = new ChromeDriver(options);
            _instance.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            _instance.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Releases a reference to the WebDriver and disposes it if not in use
        /// </summary>
        public static void ReleaseInstance()
        {
            if (_referenceCount > 0)
            {
                _referenceCount--;
            }

            if (_referenceCount == 0 && _instance != null)
            {
                lock (_lock)
                {
                    if (_referenceCount == 0 && _instance != null)
                    {
                        _instance.Quit();
                        _instance.Dispose();
                        _instance = null;
                    }
                }
            }
        }

        /// <summary>
        /// Forces disposal of the WebDriver instance regardless of reference count
        /// </summary>
        public static void ForceQuit()
        {
            lock (_lock)
            {
                if (_instance != null)
                {
                    _instance.Quit();
                    _instance.Dispose();
                    _instance = null;
                }
                _referenceCount = 0;
            }
        }

        /// <summary>
        /// Creates a new WebDriverWait instance for the singleton driver
        /// </summary>
        /// <param name="timeoutInSeconds">Timeout in seconds</param>
        /// <returns>WebDriverWait instance</returns>
        public static WebDriverWait GetWait(int timeoutInSeconds = 20)
        {
            return new WebDriverWait(Instance, TimeSpan.FromSeconds(timeoutInSeconds));
        }
    }
}