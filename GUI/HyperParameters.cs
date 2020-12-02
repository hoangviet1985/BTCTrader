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
        public const decimal DiffBTCsBetweenCurrSellAndBuyToCancelAll = 10;
        /// <summary>
        /// when buy demand and sell demand are way far from each other
        /// in BTC
        /// </summary>
        public const decimal BTCBuyOrSellDemandMuchGreaterThanOther = 40;
        /// <summary>
        /// USD offset from current price of BTC
        /// used when set price to sell or buy
        /// </summary>
        public const decimal BuySellOffsetBasedOnCurrentBTCPrice = 0.05M;
        /// <summary>
        /// used when deciding to cancel an open order falling too deep in order book
        /// depth measured by distance in USD from top order of the order book
        /// </summary>
        public const decimal CancelWhenOrderTooFarFromCurrentBTCPrice = 10;
        /// <summary>
        /// used when deciding to cancel an open order which is too close in term of USD from top order of the book,
        /// and BTC sell and buy demands are in balance.
        /// </summary>
        public const decimal BuySellVolumeIsBalanceAndOrderTooCloseCurrentBTCPrice = 3;
        /// <summary>
        /// minimum expected benefit in USD for each buy-sell cycle
        /// </summary>
        public const decimal ExpectedBenefitOfOneCircle = 10;
        /// <summary>
        /// path of log file of filled orders
        /// </summary>
        public const string LogFilePath = "log.txt";

        public const decimal BTCPriceSignificantChangeValue = 10;

        public const int NumOfSigChangesInPriceShowsUnstable = 3;

        public const decimal MaxNumOfSecondBetween2StablePricesShowsUnstable = 4;

        public const double LongEnoughUnstableBTCPriceDurationToNotice = 3;

        public const decimal AlarmTolerance = 10;

        public const decimal FeePriceRatio = 0.0015M;

        public const string fromAddress = "user_email@domain.com";
        public const string toAddresses = "phone#@tmomail.net";
        public const string fromPassword = "email_password";
    }
}
