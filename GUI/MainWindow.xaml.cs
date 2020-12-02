using CoinbasePro.Network.Authentication;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
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
        public static Authenticator authenticator;
        public static CoinbasePro.CoinbaseProClient coinbaseProClient;
        public static Tuple<string, string> BTCUSDAccIds;

        public MainWindow()
        {
            InitializeComponent();
            pauseTradingBtt.IsEnabled = false;
            
            tradingTimer = new System.Timers.Timer();
            tradingTimer.Elapsed += new ElapsedEventHandler(TradeCycleOnCoinBase);
            tradingTimer.Interval = 100;

            analyzingTimer = new System.Timers.Timer(); ;
            analyzingTimer.Elapsed += new ElapsedEventHandler(AnalyzeCycleOnCoinBaseAsync);
            analyzingTimer.Interval = 100;

            //create an authenticator with your apiKey, apiSecret and passphrase
            authenticator = new Authenticator("secret1", "secret2", "secret3");

            //create the CoinbasePro client
            coinbaseProClient = new CoinbasePro.CoinbaseProClient(authenticator);

            Task<Tuple<string, string>> task = Task.Run<Tuple<string, string>>(async () => await GetBTCAndUSDAccountIDs());
            BTCUSDAccIds = task.Result;
        }

        private async Task<Tuple<string, string>> GetBTCAndUSDAccountIDs()
        {
            //get account ids
            var getAllAccountsResponse = await coinbaseProClient.AccountsService.GetAllAccountsAsync();
            string BTCid = "";
            string USDid = "";
            var enumerator1 = getAllAccountsResponse.GetEnumerator();
            while (enumerator1.MoveNext())
            {
                if (enumerator1.Current.Currency.ToString().Equals("BTC"))
                {
                    BTCid = enumerator1.Current.Id.ToString();
                }
                else if (enumerator1.Current.Currency.ToString().Equals("USD"))
                {
                    USDid = enumerator1.Current.Id.ToString();
                }
            }

            return new Tuple<string, string>(BTCid, USDid);
        }

        private async void AnalyzeCycleOnCoinBaseAsync(object sender, ElapsedEventArgs e)
        {
            // Disable timer to avoid creating many analysis tasks at the same time
            analyzingTimer.Enabled = false;

            var productsOrderBookResponse = await coinbaseProClient.ProductsService.GetProductOrderBookAsync(CoinbasePro.Shared.Types.ProductType.BtcUsd, CoinbasePro.Services.Products.Types.ProductLevel.One);
            var enumerator = productsOrderBookResponse.Asks.GetEnumerator();
            enumerator.MoveNext();
            var currentBTCPrice = enumerator.Current.Price;
            // prints last trade price of BTC from CoinBase
            lastTracePriceLb.Dispatcher.Invoke(() => lastTracePriceLb.Content = currentBTCPrice);
            CoinBaseOperations.CheckStableStatusOfBTCPrice(
                ref CoinBaseOperations.isBTCPriceUnstable, currentBTCPrice);

            // set a fix price to track when BTC price approaching this price
            string priceToAlarmText = "";
            priceToAlarmTb.Dispatcher.Invoke(() => priceToAlarmText = priceToAlarmTb.Text);
            if (decimal.TryParse(priceToAlarmText, out decimal priceToAlarm) == false)
            {
                priceToAlarm = 0;
            }
            if(Math.Abs(priceToAlarm - currentBTCPrice) <= HyperParameters.AlarmTolerance)
            {
                CoinBaseOperations.SendUrgentMessage("Tin khan cap!", "Gia BTC dang gan muc bao dong " + priceToAlarmText + " USD");
            }
            //
            analyzingTimer.Enabled = true;
        }

        private async void TradeCycleOnCoinBase(object source, ElapsedEventArgs e)
        {
            // Disable timer to avoid creating many analysis tasks at the same time
            tradingTimer.Enabled = false;

            var BTCAcc = await coinbaseProClient.AccountsService.GetAccountByIdAsync(BTCUSDAccIds.Item1);
            var USDAcc = await coinbaseProClient.AccountsService.GetAccountByIdAsync(BTCUSDAccIds.Item2);
            // gets number of BTCs current user has
            currentBTCLb.Dispatcher.Invoke(() => currentBTCLb.Content = BTCAcc.Balance);
            // gets USD current user has
            currentUSDLb.Dispatcher.Invoke(() => currentUSDLb.Content = USDAcc.Balance);

            // gets minimum asked price of BTC (current BTC price)
            var productsOrderBookResponse = await coinbaseProClient.ProductsService.GetProductOrderBookAsync(CoinbasePro.Shared.Types.ProductType.BtcUsd, CoinbasePro.Services.Products.Types.ProductLevel.One);
            var enumerator = productsOrderBookResponse.Asks.GetEnumerator();
            enumerator.MoveNext();
            var currentBTCPrice = enumerator.Current.Price;

            // gets all filled orders user made since the app started running
            // considers open orders to keep or cancel them
            var filledOrdersDetails = CoinBaseOperations.DealWithExistingOrders(currentBTCPrice, coinbaseProClient);
            transactionDetailTb.Dispatcher.Invoke(() => transactionDetailTb.Text = filledOrdersDetails);

            // given current status of trading market, decides whether or not to buy or sell BTCc
            var numBTCToBuyStr = "";
            numBTCToBuyTb.Dispatcher.Invoke(() => numBTCToBuyStr = numBTCToBuyTb.Text);
            var numBTCToSellStr = "";
            numBTCToSellTb.Dispatcher.Invoke(() => numBTCToSellStr = numBTCToSellTb.Text);
            // consider to make transactions only when BTC is unstable
            if (Decimal.TryParse(numBTCToBuyStr, out decimal numBTCToBuy) && 
                Decimal.TryParse(numBTCToSellStr, out decimal numBTCToSell))
            {
                if (CoinBaseOperations.isBTCPriceUnstable)
                {
                    CoinBaseOperations.ConsiderToBuyOrSell(currentBTCPrice, numBTCToBuy, numBTCToSell);
                }
            }

            // enable timer for next cycle
            if (tradingTimerAllowedToRun)
            {
                tradingTimer.Enabled = true;
            }
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
