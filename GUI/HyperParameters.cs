using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    /// <summary>
    /// this class contains some hyperparameters of the trading algorithm running for coinbase
    /// </summary>
    public class HyperParameters
    {
        /// <summary>
        /// coinbase homepage
        /// </summary>
        public const string HomeUrl = "https://pro.coinbase.com/trade/BTC-USD";
        /// <summary>
        /// when buy demand and sell demand are close to each other
        /// in BTC
        /// </summary>
        public const double DiffBTCsBetweenCurrSellAndBuyToCancelAll = 10;
        /// <summary>
        /// when buy demand and sell demand are way far from each other
        /// in BTC
        /// </summary>
        public const double BTCBuyOrSellDemandMuchGreaterThanOther = 40;
        /// <summary>
        /// USD offset from current price of BTC
        /// used when set price to sell or buy
        /// </summary>
        public const double BuySellOffsetBasedOnCurrentBTCPrice = 0.05;
        /// <summary>
        /// used when deciding to cancel an open order falling too deep in order book
        /// depth measured by distance in USD from top order of the order book
        /// </summary>
        public const double CancelWhenOrderTooFarFromCurrentBTCPrice = 10;
        /// <summary>
        /// used when deciding to cancel an open order which is too close in term of USD from top order of the book,
        /// and BTC sell and buy demands are in balance.
        /// </summary>
        public const double BuySellVolumeIsBalanceAndOrderTooCloseCurrentBTCPrice = 3;
        /// <summary>
        /// minimum expected benefit in USD for each buy-sell cycle
        /// </summary>
        public const double ExpectedBenefitOfOneCircle = 10;
        /// <summary>
        /// path of log file of filled orders
        /// </summary>
        public const string LogFilePath = "log.txt";

        public const double BTCPriceSignificantChangeValue = 3;

        public const int NumOfSigChangesInPriceShowsUnstable = 3;

        public const double MaxNumOfSecondBetween2StablePricesShowsUnstable = 4;

        public const double LongEnoughUnstableBTCPriceDurationToNotice = 10;

        public const double AlarmTolerance = 10;

        public const string fromAddress = "hoangviet26985@gmail.com";
        public const string toAddresses = "6176103632@tmomail.net";
        public const string fromPassword = "vietbo1985";
    }
}
