using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public class CoinBaseXPaths
    {
        public const string SwitchToLoginPageButton = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[1]/div[1]";
        public const string LoginEmailTextBox = "//*[@id=\"email\"]";
        public const string LoginPasswordTextBox = "//*[@id=\"password\"]";
        public const string ClickToLogInButton = "//*[@id=\"signin_button\"]";
        public const string LastTradPriceLabel = "//*[@id=\"page_content\"]/div/div/div[2]/div[1]/div[2]/div[1]/span[1]";
        public const string CurrentUSDOfUser = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[1]/div/div[2]/div[2]/div[1]/div[2]/span[1]";
        public const string CurrentBTCOfUser = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[1]/div/div[2]/div[2]/div[1]/div[2]/span[2]";
        public const string BuyTab = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[1]/div/div[1]";
        public const string SellTab = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[1]/div/div[2]";
        public const string LimitTab = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[2]/div/div/div[1]/span[2]";
        public const string MarketTab = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[2]/div/div/div[1]/span[1]";
        public const string MaxButton = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[3]/div[1]/div[1]/div/a";
        public const string LimitModeInputBTCsToBuySellTextBox = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[3]/div[1]/div[1]/div/input";
        public const string LimitModeInputBTCPriceTextBox = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[3]/div[1]/div[2]/div/input";
        public const string PostOnlyButton = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[3]/div[1]/div[3]/div[1]/div[1]";
        public const string PlaceOrderButton = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[3]/div[4]";
        public const string CommonPartOfXPathOfReportedOrderDetail = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[4]/div/div[2]/div/div/div[2]/div/div";
        public const string MarketModeInputBTCsToSellTextBox = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[3]/div[1]/div/input";
        public const string LabelNextToMarketModeInputBTCsToSellTextBox = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[3]/div[1]/div/div";
        public const string NumOfBTCsWhenClickOnLastLineSellPartOrderBook = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[3]/div[3]/div[2]/span[2]/span";
        public const string OrderRejectedMessageCloseSymbol = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[1]/div/div/div/div/div[3]/div[2]/form/div[3]/div[5]/span[2]";
        public const string DepthChartButton = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[3]/div/div[1]/div/div[2]/span[2]";
        public const string DepthChartCanvas = "//*[@id=\"page_content\"]/div/div/div[2]/div[2]/div/div[3]/div/div[2]/div/div[4]/canvas";
    }
}
