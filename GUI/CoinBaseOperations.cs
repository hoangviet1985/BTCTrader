using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

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
        private static string currentOpenOderId = "";
        private static decimal buyPriceOfBTC = 0;
        private static decimal sellPriceOfBTC = 0;

        public static decimal stableBTCPrice = 0;
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
        public static string DealWithExistingOrders(decimal currentBTCPrice, CoinbasePro.CoinbaseProClient coinbaseProClient)
        {
            var saleAndBuyBTCsVolumeAtOptimal = GetSellAndBuyBTCsVolumeAtOptimalAsync();
            string filledOrderDetails = "";
            Task.Run(async () =>
            {
                var fillResponse = await MainWindow.coinbaseProClient.FillsService.GetFillsByProductIdAsync(CoinbasePro.Shared.Types.ProductType.BtcUsd, 10000, 0);
                var pageIterator = fillResponse.GetEnumerator();
                while (pageIterator.MoveNext())
                {
                    var transIterator = pageIterator.Current.GetEnumerator();
                    while (transIterator.MoveNext())
                    {
                        filledOrderDetails += "ID: " + transIterator.Current.OrderId + "\t" +
                        "BTCprice: " + transIterator.Current.Price + "\t" +
                        "Volume: " + transIterator.Current.Size + "\t" +
                        "Fee: " + transIterator.Current.Fee + "\t" +
                        "TimeCreated:" + transIterator.Current.CreatedAt + "\n";
                    }
                }
            });
            if (!currentOpenOderId.Equals("") &&
                (Math.Abs(saleAndBuyBTCsVolumeAtOptimal.Result.Item1 - saleAndBuyBTCsVolumeAtOptimal.Result.Item2) < HyperParameters.DiffBTCsBetweenCurrSellAndBuyToCancelAll) &&
                ((turnOfBuying && (sellPriceOfBTC < currentBTCPrice + HyperParameters.BuySellVolumeIsBalanceAndOrderTooCloseCurrentBTCPrice)) ||
                (!turnOfBuying && (buyPriceOfBTC > currentBTCPrice - HyperParameters.BuySellVolumeIsBalanceAndOrderTooCloseCurrentBTCPrice))))
            {
                CancelAllOrders();
            }
            else
            {
                ConsiderToCancelOpenOrders(
                    saleAndBuyBTCsVolumeAtOptimal.Result,
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
        public static void ConsiderToBuyOrSell(decimal currentBTCPrice, decimal numOfBTCsForBuy, decimal numOfBTCsForSell)
        {
            var saleAndBuyBTCsVolumeAtOptimal = GetSellAndBuyBTCsVolumeAtOptimalAsync();
            if (turnOfBuying && currentOpenOderId.Equals(""))
            {
                if ((saleAndBuyBTCsVolumeAtOptimal.Result.Item2 - saleAndBuyBTCsVolumeAtOptimal.Result.Item1 >= HyperParameters.DiffBTCsBetweenCurrSellAndBuyToCancelAll))
                {
                    buyPriceOfBTC = currentBTCPrice - 
                        HyperParameters.BuySellOffsetBasedOnCurrentBTCPrice -
                        currentBTCPrice * numOfBTCsForBuy * HyperParameters.FeePriceRatio;
                    // set buy order
                    Task.Run(async () =>
                    {
                        var order = await MainWindow.coinbaseProClient.OrdersService.PlaceLimitOrderAsync(
                            CoinbasePro.Services.Orders.Types.OrderSide.Buy,
                            CoinbasePro.Shared.Types.ProductType.BtcUsd,
                            numOfBTCsForBuy,
                            currentBTCPrice);
                        currentOpenOderId = order.Id.ToString();
                        turnOfBuying = false;
                    });
                }
            }
            else if (currentOpenOderId.Equals(""))
            {
                if (currentBTCPrice >= buyPriceOfBTC + HyperParameters.ExpectedBenefitOfOneCircle)
                {
                    sellPriceOfBTC = currentBTCPrice + 
                        HyperParameters.BuySellOffsetBasedOnCurrentBTCPrice +
                        currentBTCPrice * numOfBTCsForSell * HyperParameters.FeePriceRatio;
                    // set sell order
                    Task.Run(async () =>
                    {
                        var order = await MainWindow.coinbaseProClient.OrdersService.PlaceLimitOrderAsync(
                            CoinbasePro.Services.Orders.Types.OrderSide.Buy,
                            CoinbasePro.Shared.Types.ProductType.BtcUsd,
                            numOfBTCsForSell,
                            currentBTCPrice);
                        currentOpenOderId = order.Id.ToString();
                        turnOfBuying = true;
                    });
                }
            }
        }

        /// <summary>
        /// records the number of BTCs are being put on the web to sell at lowest price at the moment
        /// records the number of BTCs that can be bought at highest price at the moment
        /// </summary>
        /// <returns>the pair of the two values mentioned above</returns>
        private static async System.Threading.Tasks.Task<Tuple<decimal, decimal>> GetSellAndBuyBTCsVolumeAtOptimalAsync()
        {
            var productsOrderBookResponse = await MainWindow.coinbaseProClient.ProductsService.GetProductOrderBookAsync(CoinbasePro.Shared.Types.ProductType.BtcUsd, CoinbasePro.Services.Products.Types.ProductLevel.One);
            var enumerator0 = productsOrderBookResponse.Asks.GetEnumerator();
            var enumerator1 = productsOrderBookResponse.Bids.GetEnumerator();
            enumerator0.MoveNext();
            enumerator1.MoveNext();

            return new Tuple<decimal, decimal>(enumerator0.Current.Size, enumerator1.Current.Size);
        }

        /// <summary>
        /// cancels all open orders
        /// saves details of filled order
        /// </summary>
        /// <param name="filledOrderDetails">where to save details of filled orders</param>
        private static void CancelAllOrders()
        {
            Task.Run(async () =>
            {
                await MainWindow.coinbaseProClient.OrdersService.CancelAllOrdersAsync();
                currentOpenOderId = "";
                turnOfBuying = !turnOfBuying;
            });
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
            Tuple<decimal, decimal> saleAndBuyBTCsVolumeAtOptimal,
            decimal currentBTCPrice)
        {
            if (!currentOpenOderId.Equals(""))
            {
                Task.Run(async () =>
                {
                    var order = await MainWindow.coinbaseProClient.OrdersService.GetOrderByIdAsync(currentOpenOderId);
                    if (order.Status.Equals(CoinbasePro.Services.Orders.Types.OrderStatus.Open))
                    {
                        if (turnOfBuying)
                        {
                            if (saleAndBuyBTCsVolumeAtOptimal.Item1 - saleAndBuyBTCsVolumeAtOptimal.Item2 > HyperParameters.BTCBuyOrSellDemandMuchGreaterThanOther)
                            {
                                await MainWindow.coinbaseProClient.OrdersService.CancelOrderByIdAsync(currentOpenOderId);
                                currentOpenOderId = "";
                                turnOfBuying = false;
                            }
                        }
                        else
                        {
                            if ((currentBTCPrice - buyPriceOfBTC > HyperParameters.CancelWhenOrderTooFarFromCurrentBTCPrice) ||
                                (saleAndBuyBTCsVolumeAtOptimal.Item2 - saleAndBuyBTCsVolumeAtOptimal.Item1 < HyperParameters.DiffBTCsBetweenCurrSellAndBuyToCancelAll))
                            {
                                await MainWindow.coinbaseProClient.OrdersService.CancelOrderByIdAsync(currentOpenOderId);
                                currentOpenOderId = "";
                                turnOfBuying = true;
                            }
                        }
                    }
                    else
                    {
                        currentOpenOderId = "";
                    }
                });    
            }
        }

        public static void CheckStableStatusOfBTCPrice(ref bool isBTCPriceUnstable, decimal currentBTCPrice)
        {
            if ((decimal)(DateTimeOffset.Now - timeAtCurrentStablePrice).TotalSeconds <= HyperParameters.MaxNumOfSecondBetween2StablePricesShowsUnstable)
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
