using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Timers;
using System.Windows;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Timers.Timer tradingTimer;
        private bool tradingTimerAllowedToRun;
        private System.Timers.Timer analyzingTimer;

        public MainWindow()
        {
            InitializeComponent();
            pauseTradingBtt.IsEnabled = false;
            
            tradingTimer = new System.Timers.Timer();
            tradingTimer.Elapsed += new ElapsedEventHandler(TradeCycleOnCoinBase);
            tradingTimer.Interval = 100;

            analyzingTimer = new System.Timers.Timer(); ;
            analyzingTimer.Elapsed += new ElapsedEventHandler(AnalyzeCycleOnCoinBase);
            analyzingTimer.Interval = 100;
        }

        private void AnalyzeCycleOnCoinBase(object sender, ElapsedEventArgs e)
        {
            // Disable timer to avoid creating many analysis tasks at the same time
            analyzingTimer.Enabled = false;
            try
            {
                var currentBTCPrice = CoinBaseOperations.GetCurrentBTCPrice();
                CoinBaseOperations.CheckStableStatusOfBTCPrice(
                    ref CoinBaseOperations.isBTCPriceUnstable, currentBTCPrice);
                string priceToAlarmText = "";
                priceToAlarmTb.Dispatcher.Invoke(() => priceToAlarmText = priceToAlarmTb.Text);
                if (double.TryParse(priceToAlarmText, out double priceToAlarm) == false)
                {
                    priceToAlarm = 0;
                }
                if(Math.Abs(priceToAlarm - currentBTCPrice) <= HyperParameters.AlarmTolerance)
                {
                    CoinBaseOperations.SendUrgentMessage("Tin khan cap!", "Gia BTC dang gan muc bao dong " + priceToAlarmText + " USD");
                }
            }
            catch (Exception ex)
            {
                if ((ex is NullReferenceException) ||
                    (ex is WebDriverException) ||
                    CoinBaseOperations.IsBrowserClosed(CoinBaseOperations.analyzeWebDriver))
                {
                    InitializeCoinBaseEnvironmentForAnalyzing();
                }
            }
            analyzingTimer.Enabled = true;
        }

        private void TradeCycleOnCoinBase(object source, ElapsedEventArgs e)
        {
            // Disable timer to avoid creating many analysis tasks at the same time
            tradingTimer.Enabled = false;
            try
            {
                // gets last trade price of BTC from CoinBase
                CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.lastTradePriceElem, CoinBaseXPaths.LastTradPriceLabel);
                var tempStr = CoinBaseOperations.lastTradePriceElem.Text;
                lastTracePriceLb.Dispatcher.Invoke(() => lastTracePriceLb.Content = tempStr);

                // gets number of BTCs current user has
                CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.currentBTCElem, CoinBaseXPaths.CurrentBTCOfUser);
                currentBTCLb.Dispatcher.Invoke(() => currentBTCLb.Content = CoinBaseOperations.currentBTCElem.Text);

                // gets USD current user has
                CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.currentUSDElem, CoinBaseXPaths.CurrentUSDOfUser);
                currentUSDLb.Dispatcher.Invoke(() => currentUSDLb.Content = CoinBaseOperations.currentUSDElem.Text);
                var currentBTCPrice = double.Parse(tempStr.Split(' ')[0], 
                    System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowDecimalPoint);

                // gets all filled orders user made since the app started running
                // considers open orders to keep or cancel them
                var filledOrdersDetails = CoinBaseOperations.DealWithExistingOrders(currentBTCPrice);
                transactionDetailTb.Dispatcher.Invoke(() => transactionDetailTb.Text += filledOrdersDetails);

                // given current status of trading market, decides whether or not to buy or sell BTCc
                var numBTCToBuy = "";
                numBTCToBuyTb.Dispatcher.Invoke(() => numBTCToBuy = numBTCToBuyTb.Text);
                var numBTCToSell = "";
                numBTCToSellTb.Dispatcher.Invoke(() => numBTCToSell = numBTCToSellTb.Text);
                // consider to make transactions only when BTC is unstable
                if (CoinBaseOperations.isBTCPriceUnstable)
                {
                    CoinBaseOperations.ConsiderToBuyOrSell(currentBTCPrice, numBTCToBuy, numBTCToSell);
                }
            }
            catch (Exception ex)
            {
                if ((ex is NullReferenceException) || 
                    (ex is WebDriverException) ||
                    CoinBaseOperations.IsBrowserClosed(CoinBaseOperations.tradingWebDriver))
                {
                    InitializeCoinBaseEnvironmentForTrading();
                }
            }
            // enable timer for next cycle
            if (tradingTimerAllowedToRun)
            {
                tradingTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Initializes all necessary CoinBase web elements for trading
        /// Brings user to the page that trading operation can be made
        /// </summary>
        private void InitializeCoinBaseEnvironmentForTrading()
        {
            if (CoinBaseOperations.tradingWebDriver != null)
            {
                CoinBaseOperations.tradingWebDriver.Quit();
            }
            CoinBaseOperations.tradingWebDriver = new ChromeDriver();
            // goes to CoinBase home page
            CoinBaseOperations.tradingWebDriver.Navigate().GoToUrl(HyperParameters.HomeUrl);
            // goes to login page
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out IWebElement switchToLoginPageBtt, CoinBaseXPaths.SwitchToLoginPageButton);
            switchToLoginPageBtt.Click();
            // fills out user's credentials and logs user in
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out IWebElement email, CoinBaseXPaths.LoginEmailTextBox);
            emailTb.Dispatcher.Invoke(() => email.SendKeys(emailTb.Text));
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out IWebElement password, CoinBaseXPaths.LoginPasswordTextBox);
            passwordTb.Dispatcher.Invoke(() => password.SendKeys(passwordTb.Password));
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out IWebElement loginBtt, CoinBaseXPaths.ClickToLogInButton);
            loginBtt.Click();
            // stops for 20 seconds for user to receive passcode from their phone and
            // enter passcode to the page before continue to load other web elements
            Thread.Sleep(20000);
            // initializes some variables to hold some web elements
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.lastTradePriceElem, CoinBaseXPaths.LastTradPriceLabel);
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.currentBTCElem, CoinBaseXPaths.CurrentBTCOfUser);
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.currentUSDElem, CoinBaseXPaths.CurrentUSDOfUser);
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.switchToLimitTabElem, CoinBaseXPaths.LimitTab);
            // click on limit tab of trading page
            CoinBaseOperations.switchToLimitTabElem.Click();
            // initializes variables of web elements that only appear when limit tab clicked
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.numBTCToBuySellElem, CoinBaseXPaths.LimitModeInputBTCsToBuySellTextBox);
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.setSellBuyPriceElem, CoinBaseXPaths.LimitModeInputBTCPriceTextBox);
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out IWebElement setPostOnlyButtonElem, CoinBaseXPaths.PostOnlyButton);
            // makes sure all future order is in post mode
            setPostOnlyButtonElem.Click();
            // initializes some variables holding some web elements
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.switchToBuyTabElem, CoinBaseXPaths.BuyTab);
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.switchToSellTabElem, CoinBaseXPaths.SellTab);
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.placeBuySaleOrderButtonElem, CoinBaseXPaths.PlaceOrderButton);
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.tradingSwitchToDepthChartElem, CoinBaseXPaths.DepthChartButton);
            CoinBaseOperations.tradingSwitchToDepthChartElem.Click();
            CoinBaseOperations.InitializeElement(CoinBaseOperations.tradingWebDriver, out CoinBaseOperations.tradingDepthChartCanvasElem, CoinBaseXPaths.DepthChartCanvas);
        }

        /// <summary>
        /// Initializes all necessary CoinBase web elements for trading
        /// Brings user to the page that trading operation can be made
        /// </summary>
        private void InitializeCoinBaseEnvironmentForAnalyzing()
        {
            if (CoinBaseOperations.analyzeWebDriver != null)
            {
                CoinBaseOperations.analyzeWebDriver.Quit();
            }
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            CoinBaseOperations.analyzeWebDriver = new ChromeDriver(options);
            // goes to CoinBase home page
            CoinBaseOperations.analyzeWebDriver.Navigate().GoToUrl(HyperParameters.HomeUrl);
            // initializes some variables holding some web elements
            CoinBaseOperations.InitializeElement(CoinBaseOperations.analyzeWebDriver, out CoinBaseOperations.switchToDepthChartElem, CoinBaseXPaths.DepthChartButton);
            CoinBaseOperations.switchToDepthChartElem.Click();
            CoinBaseOperations.InitializeElement(CoinBaseOperations.analyzeWebDriver, out CoinBaseOperations.BTCsOfHighestPriceBuyDemandElem, CoinBaseXPaths.MarketModeInputBTCsToSellTextBox);
            CoinBaseOperations.InitializeElement(CoinBaseOperations.analyzeWebDriver, out CoinBaseOperations.BTCsOfLowestPriceSellDemandElem, CoinBaseXPaths.NumOfBTCsWhenClickOnLastLineSellPartOrderBook);
            CoinBaseOperations.InitializeElement(CoinBaseOperations.analyzeWebDriver, out CoinBaseOperations.depthChartCanvasElem, CoinBaseXPaths.DepthChartCanvas);
        }

        private void StartButtonClick(object sender, RoutedEventArgs e)
        {
            //InitializeEnvironment();
            tradingTimerAllowedToRun = true;
            tradingTimer.Enabled = true;
            startBtt.IsEnabled = false;
            pauseTradingBtt.IsEnabled = true;
        }

        private void PauseTrading(object sender, RoutedEventArgs e)
        {
            tradingTimerAllowedToRun = false;
            startBtt.IsEnabled = true;
            pauseTradingBtt.IsEnabled = false;
        }

        private void StartAnalyzingClick(object sender, RoutedEventArgs e)
        {
            CoinBaseOperations.smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(HyperParameters.fromAddress, HyperParameters.fromPassword)
            };

            analyzingTimer.Enabled = true;
            analyzeBtt.IsEnabled = false;
        }
    }
}
