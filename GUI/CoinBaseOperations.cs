using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.IO;
using System.Net.Mail;
using System.Threading;

namespace GUI
{
    /// <summary>
    /// This class contains all operations on CoinBase website
    /// </summary>
    public class CoinBaseOperations
    {
        public static IWebDriver analyzeWebDriver;
        public static IWebElement switchToDepthChartElem;
        public static IWebElement depthChartCanvasElem;
        public static IWebElement BTCsOfLowestPriceSellDemandElem;
        public static IWebElement BTCsOfHighestPriceBuyDemandElem;

        public static IWebDriver tradingWebDriver;
        public static IWebElement lastTradePriceElem;
        public static IWebElement currentUSDElem;
        public static IWebElement currentBTCElem;
        public static IWebElement numBTCToBuySellElem;
        public static IWebElement setSellBuyPriceElem;
        public static IWebElement placeBuySaleOrderButtonElem;
        public static IWebElement tradingBTCsOfLowestPriceSellDemandElem;
        public static IWebElement tradingBTCsOfHighestPriceBuyDemandElem;
        public static IWebElement tradingSwitchToDepthChartElem;
        public static IWebElement tradingDepthChartCanvasElem;

        public static IWebElement switchToBuyTabElem;
        public static IWebElement switchToSellTabElem;
        public static IWebElement switchToLimitTabElem;
       

        private static bool turnOfBuying = true;
        private static bool orderExists = false;
        private static double buyPriceOfBTC = 0;
        private static double sellPriceOfBTC = 0;

        public static double stableBTCPrice = 0;
        public static DateTimeOffset timeAtCurrentStablePrice = DateTimeOffset.MinValue;
        public static int timesOfGoToNewMuchDifferentStablePrice = 0;
        public static bool isBTCPriceUnstable = false;
        public static DateTimeOffset whenCurrentUnstableStateStarted;
        public static double currentUnstableStateDurationInSeconds = 0;

        public static SmtpClient smtp;

        /// <summary>
        /// checks current status of orders
        /// removes orders if necessary
        /// </summary>
        /// <param name="currentBTCPrice">current market BTC price</param>
        /// <returns>string of details of current existing orders</returns>
        public static string DealWithExistingOrders(double currentBTCPrice)
        {
            var saleAndBuyBTCsVolumeAtOptimal = GetSellAndBuyBTCsVolumeAtOptimal();
            string filledOrderDetails = "";
            if (orderExists &&
                (Math.Abs(saleAndBuyBTCsVolumeAtOptimal.Item1 - saleAndBuyBTCsVolumeAtOptimal.Item2) < HyperParameters.DiffBTCsBetweenCurrSellAndBuyToCancelAll) &&
                ((turnOfBuying && (sellPriceOfBTC < currentBTCPrice + HyperParameters.BuySellVolumeIsBalanceAndOrderTooCloseCurrentBTCPrice)) ||
                (!turnOfBuying && (buyPriceOfBTC > currentBTCPrice - HyperParameters.BuySellVolumeIsBalanceAndOrderTooCloseCurrentBTCPrice))))
            {
                CancelAllOrders(ref filledOrderDetails);
            }
            else
            {
                ConsiderToCancelOpenOrders(
                    ref filledOrderDetails,
                    saleAndBuyBTCsVolumeAtOptimal,
                    currentBTCPrice);
            }
            return filledOrderDetails;
        }

        /// <summary>
        /// Analyzes the market and decides to sell or buy BTCs
        /// </summary>
        /// <param name="currentBTCPrice">current market price of BTC</param>
        /// <param name="numOfBTCsForBuy">number of BTCs for buying</param>
        /// <param name="numOfBTCsForSell">number of BTCc for selling</param>
        public static void ConsiderToBuyOrSell(double currentBTCPrice, string numOfBTCsForBuy, string numOfBTCsForSell)
        {
            try
            {
                var saleAndBuyBTCsVolumeAtOptimal = GetSellAndBuyBTCsVolumeAtOptimal();
                if (turnOfBuying && !orderExists)
                {
                    if ((saleAndBuyBTCsVolumeAtOptimal.Item2 - saleAndBuyBTCsVolumeAtOptimal.Item1 >= HyperParameters.DiffBTCsBetweenCurrSellAndBuyToCancelAll))
                    {
                        InitializeElement(tradingWebDriver, out numBTCToBuySellElem, CoinBaseXPaths.LimitModeInputBTCsToBuySellTextBox);
                        numBTCToBuySellElem.SendKeys(numOfBTCsForBuy);
                        buyPriceOfBTC = currentBTCPrice - HyperParameters.BuySellOffsetBasedOnCurrentBTCPrice;
                        InitializeElement(tradingWebDriver, out setSellBuyPriceElem, CoinBaseXPaths.LimitModeInputBTCPriceTextBox);
                        setSellBuyPriceElem.SendKeys(buyPriceOfBTC.ToString());
                        InitializeElement(tradingWebDriver, out switchToBuyTabElem, CoinBaseXPaths.BuyTab);
                        switchToBuyTabElem.Click();
                        InitializeElement(tradingWebDriver, out CoinBaseOperations.switchToLimitTabElem, CoinBaseXPaths.LimitTab);
                        switchToLimitTabElem.Click();
                        placeBuySaleOrderButtonElem.Click();

                        var orderRejected = TryFindElement(tradingWebDriver, CoinBaseXPaths.OrderRejectedMessageCloseSymbol, out IWebElement xSymbol);
                        if (orderRejected)
                        {
                            xSymbol.Click();
                        }
                        else
                        {
                            turnOfBuying = false;
                            orderExists = true;
                        }
                    }
                }
                else if (!orderExists)
                {
                    if (currentBTCPrice >= buyPriceOfBTC + HyperParameters.ExpectedBenefitOfOneCircle)
                    {
                        InitializeElement(tradingWebDriver, out numBTCToBuySellElem, CoinBaseXPaths.LimitModeInputBTCsToBuySellTextBox);
                        numBTCToBuySellElem.SendKeys(numOfBTCsForSell);
                        sellPriceOfBTC = currentBTCPrice + HyperParameters.BuySellOffsetBasedOnCurrentBTCPrice;
                        InitializeElement(tradingWebDriver, out setSellBuyPriceElem, CoinBaseXPaths.LimitModeInputBTCPriceTextBox);
                        setSellBuyPriceElem.SendKeys(sellPriceOfBTC.ToString());
                        InitializeElement(tradingWebDriver, out switchToSellTabElem, CoinBaseXPaths.SellTab);
                        switchToSellTabElem.Click();
                        InitializeElement(tradingWebDriver, out CoinBaseOperations.switchToLimitTabElem, CoinBaseXPaths.LimitTab);
                        switchToLimitTabElem.Click();
                        placeBuySaleOrderButtonElem.Click();

                        var orderRejected = TryFindElement(tradingWebDriver, CoinBaseXPaths.OrderRejectedMessageCloseSymbol, out IWebElement xSymbol);
                        if (orderRejected)
                        {
                            xSymbol.Click();
                        }
                        else
                        {
                            turnOfBuying = true;
                            orderExists = true;
                        }
                    }
                }
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// records the number of BTCs are being put on the web to sell at lowest price at the moment
        /// records the number of BTCs that can be bought at highest price at the moment
        /// </summary>
        /// <returns>the pair of the two values mentioned above</returns>
        private static Tuple<double, double> GetSellAndBuyBTCsVolumeAtOptimal()
        {
            double numOfBTCsForSaleAtLowestPrice = 0;
            double numOfBTCsOrderedAtHighestPrice = 0;
            while (true)
            {
                try
                {
                    var action = new Actions(tradingWebDriver);
                    InitializeElement(tradingWebDriver, out tradingSwitchToDepthChartElem, CoinBaseXPaths.DepthChartButton);
                    tradingSwitchToDepthChartElem.Click();
                    InitializeElement(tradingWebDriver, out IWebElement switchToMarketTabElem, CoinBaseXPaths.MarketTab);
                    switchToMarketTabElem.Click();

                    InitializeElement(tradingWebDriver, out tradingDepthChartCanvasElem, CoinBaseXPaths.DepthChartCanvas);
                    InitializeElement(tradingWebDriver, out IWebElement saleElem, CoinBaseXPaths.NumOfBTCsWhenClickOnLastLineSellPartOrderBook);
                    InitializeElement(tradingWebDriver, out IWebElement buyElem, CoinBaseXPaths.MarketModeInputBTCsToSellTextBox);
                    InitializeElement(tradingWebDriver, out IWebElement labelUSDBTCElem, CoinBaseXPaths.LabelNextToMarketModeInputBTCsToSellTextBox);

                    action.MoveToElement(tradingDepthChartCanvasElem, tradingDepthChartCanvasElem.Size.Width / 2 + 1, 1).Click().Perform();
                    string saleElemText = saleElem.Text;
                    if(saleElemText[0] != '$')
                    {
                        numOfBTCsForSaleAtLowestPrice = double.Parse(saleElemText.Substring(1));
                    }
                    
                    action.MoveToElement(tradingDepthChartCanvasElem, tradingDepthChartCanvasElem.Size.Width / 2, 1).Click().Perform();
                    if(labelUSDBTCElem.Text == "BTC")
                    {
                        numOfBTCsOrderedAtHighestPrice = double.Parse(buyElem.GetAttribute("value"));
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if ((ex is WebDriverException) || IsBrowserClosed(tradingWebDriver))
                    {
                        break;
                    }
                }
            }

            return new Tuple<double, double>(numOfBTCsForSaleAtLowestPrice, numOfBTCsOrderedAtHighestPrice);
        }

        /// <summary>
        /// initializes a variable to hold a web page's element
        /// </summary>
        /// <param name="element">the web element's variable</param>
        /// <param name="elementXPath">the string represents XPath of the element in the page</param>
        /// <param name="timeout">time out in ms for the searching operation</param>
        public static void InitializeElement(IWebDriver webDriver, out IWebElement element, string elementXPath, int timeout = 150)
        {
            bool success = false;
            int elapsed = 0;
            element = null;
            while ((!success) && (elapsed < timeout))
            {
                try
                {
                    Thread.Sleep(10);
                    elapsed += 10;
                    element = webDriver.FindElement(By.XPath(elementXPath));
                    success = true;
                }
                catch (Exception ex)
                {
                    if (!(ex is NoSuchElementException))
                    {
                        throw;
                    }
                }
            }
            if (!success)
            {
                throw new NoSuchElementException("XPath not found: " + elementXPath);
            }
        }

        /// <summary>
        /// checks if the chrome browser instance associated with the webdriver is closed
        /// </summary>
        /// <param name="driver">the web driver instance that the current browser instance associated with</param>
        /// <returns>true if the browser instance is closed, false otherwise</returns>
        public static bool IsBrowserClosed(IWebDriver driver)
        {
            bool isClosed = false;
            try
            {
                var title = driver.Title;
            }
            catch (InvalidOperationException)
            {
                isClosed = true;
            }
            return isClosed;
        }

        /// <summary>
        /// write a filled order to a log file
        /// </summary>
        /// <param name="orderIndex">index of the order in list of on-page orders</param>
        /// <returns>a string represents the order's details</returns>
        private static string WriteFilledTransactionToLogFile(int orderIndex)
        {
            var buySellSide = tradingWebDriver.FindElement(By.XPath(CoinBaseXPaths.CommonPartOfXPathOfReportedOrderDetail + "[" + orderIndex.ToString() + "]/div[1]/div/span")).Text;
            var amountBTCs = tradingWebDriver.FindElement(By.XPath(CoinBaseXPaths.CommonPartOfXPathOfReportedOrderDetail + "[" + orderIndex.ToString() + "]/div[2]/span")).Text;
            var atPrice = tradingWebDriver.FindElement(By.XPath(CoinBaseXPaths.CommonPartOfXPathOfReportedOrderDetail + "[" + orderIndex.ToString() + "]/div[4]/span")).Text;
            var orderDetails = buySellSide + " " + amountBTCs + " at " + atPrice + " USD/BTC" + "--------" + DateTimeOffset.Now.ToString();
            File.AppendAllLines(HyperParameters.LogFilePath,
            new[] { orderDetails });
            return orderDetails;
        }

        /// <summary>
        /// tries to find an web element from a web page
        /// </summary>
        /// <param name="elemXPath">the string represent XPath of the element in the web page</param>
        /// <param name="element">the variable that hold the element if it found</param>
        /// <returns>true if the element found, and false otherwise</returns>
        private static bool TryFindElement(IWebDriver webDriver, string elemXPath, out IWebElement element)
        {
            try
            {
                InitializeElement(webDriver, out element, elemXPath);
            }
            catch (NoSuchElementException)
            {
                element = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// cancels all open orders
        /// saves details of filled order
        /// </summary>
        /// <param name="filledOrderDetails">where to save details of filled orders</param>
        private static void CancelAllOrders(ref string filledOrderDetails)
        {
            var orderIndex = 1;
            while (true)
            {
                try
                {
                    var orderStatus = tradingWebDriver.FindElement(By.XPath(CoinBaseXPaths.CommonPartOfXPathOfReportedOrderDetail + "[" + orderIndex.ToString() + "]/div[6]/span")).Text;

                    if (orderStatus == "Filled")
                    {
                        var orderDetails = WriteFilledTransactionToLogFile(orderIndex);
                        filledOrderDetails += orderDetails + "\n";
                    }

                    var buySellSide = tradingWebDriver.FindElement(By.XPath(CoinBaseXPaths.CommonPartOfXPathOfReportedOrderDetail + "[" + orderIndex.ToString() + "]/div[1]/div/span")).Text;
                    tradingWebDriver.FindElement(By.XPath(CoinBaseXPaths.CommonPartOfXPathOfReportedOrderDetail + "[" + orderIndex.ToString() + "]/div[7]/div")).Click();
                    if (orderStatus == "Open" || orderStatus == "Filled")
                    {
                        orderExists = false;
                        if (orderStatus == "Open")
                        {
                            if (buySellSide == "Buy")
                            {
                                turnOfBuying = true;
                            }
                            else
                            {
                                turnOfBuying = false;
                            }
                        }
                    }

                    orderIndex += 1;
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Looks at every open order
        /// Cancels open orders if necessary
        /// Saves details of filled orders
        /// </summary>
        /// <param name="filledOrderDetails">where to save details of filled orders</param>
        /// <param name="saleAndBuyBTCsVolumeAtOptimal">BTC volume of lowest price sell demand and highest price buy demand</param>
        /// <param name="currentBTCPrice">current market price of BTC</param>
        private static void ConsiderToCancelOpenOrders(
            ref string filledOrderDetails, 
            Tuple<double, double> saleAndBuyBTCsVolumeAtOptimal,
            double currentBTCPrice)
        {
            int orderIndex = 1;
            while (true)
            {
                try
                {
                    var orderStatus = tradingWebDriver.FindElement(By.XPath(CoinBaseXPaths.CommonPartOfXPathOfReportedOrderDetail + "[" + orderIndex.ToString() + "]/div[6]/span")).Text;
                    if (orderStatus == "Open")
                    {
                        if (turnOfBuying)
                        {
                            if (saleAndBuyBTCsVolumeAtOptimal.Item1 - saleAndBuyBTCsVolumeAtOptimal.Item2 > HyperParameters.BTCBuyOrSellDemandMuchGreaterThanOther)
                            {
                                InitializeElement(tradingWebDriver, out IWebElement cancelOrderButton, CoinBaseXPaths.CommonPartOfXPathOfReportedOrderDetail + "[" + orderIndex.ToString() + "]/div[7]/div");
                                cancelOrderButton.Click();
                                orderExists = false;
                                turnOfBuying = false;
                            }
                        }
                        else
                        {
                            if ((currentBTCPrice - buyPriceOfBTC > HyperParameters.CancelWhenOrderTooFarFromCurrentBTCPrice) ||
                                (saleAndBuyBTCsVolumeAtOptimal.Item2 - saleAndBuyBTCsVolumeAtOptimal.Item1 < HyperParameters.DiffBTCsBetweenCurrSellAndBuyToCancelAll))
                            {
                                InitializeElement(tradingWebDriver, out IWebElement cancelOrderButton, CoinBaseXPaths.CommonPartOfXPathOfReportedOrderDetail + "[" + orderIndex.ToString() + "]/div[7]/div");
                                cancelOrderButton.Click();
                                orderExists = false;
                                turnOfBuying = true;
                            }
                        }
                    }
                    else
                    {
                        if (orderStatus == "Filled")
                        {
                            var orderDetails = WriteFilledTransactionToLogFile(orderIndex);
                            filledOrderDetails += orderDetails + "\n";
                        }

                        tradingWebDriver.FindElement(By.XPath(CoinBaseXPaths.CommonPartOfXPathOfReportedOrderDetail + "[" + orderIndex.ToString() + "]/div[7]/div")).Click();
                        orderExists = false;
                    }

                    orderIndex += 1;
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        public static double GetCurrentBTCPrice()
        {
            InitializeElement(analyzeWebDriver, out IWebElement priceLb, CoinBaseXPaths.LastTradPriceLabel);
            var currentBTCPrice = double.Parse(priceLb.Text.Split(' ')[0],
                    System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowDecimalPoint);
            return currentBTCPrice;
        }

        public static void CheckStableStatusOfBTCPrice(ref bool isBTCPriceUnstable, double currentBTCPrice)
        {
            if ((DateTimeOffset.Now - timeAtCurrentStablePrice).TotalSeconds <= HyperParameters.MaxNumOfSecondBetween2StablePricesShowsUnstable)
            {
                if (Math.Abs(currentBTCPrice - stableBTCPrice) >= HyperParameters.BTCPriceSignificantChangeValue)
                {
                    stableBTCPrice = currentBTCPrice;
                    timeAtCurrentStablePrice = DateTimeOffset.Now;
                    timesOfGoToNewMuchDifferentStablePrice += 1;
                    if (timesOfGoToNewMuchDifferentStablePrice == HyperParameters.NumOfSigChangesInPriceShowsUnstable)
                    {
                        timesOfGoToNewMuchDifferentStablePrice = 0;
                        isBTCPriceUnstable = true;
                    }
                }
            }
            else
            {
                timeAtCurrentStablePrice = DateTimeOffset.Now;
                if(whenCurrentUnstableStateStarted != null && isBTCPriceUnstable)
                {
                    currentUnstableStateDurationInSeconds = (timeAtCurrentStablePrice - whenCurrentUnstableStateStarted).TotalSeconds;
                    if(currentUnstableStateDurationInSeconds >= HyperParameters.LongEnoughUnstableBTCPriceDurationToNotice)
                    {
                        SendUrgentMessage(
                            "Tin BTC khan cap!",
                            "Gia BTC dang bien dong manh trong lien tuc hon 10 giay. Gia hien tai: " + currentBTCPrice.ToString() + " USD");
                    }
                }
                whenCurrentUnstableStateStarted = timeAtCurrentStablePrice;
                stableBTCPrice = currentBTCPrice;
                timesOfGoToNewMuchDifferentStablePrice = 0;
                isBTCPriceUnstable = false;
            }
        }

        public static void SendUrgentMessage(string subject, string body)
        {
            using (var message = new MailMessage(HyperParameters.fromAddress, HyperParameters.toAddresses)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
