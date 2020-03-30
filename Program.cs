using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Omnibasis.GoogleWallet.Services;
using System;
using System.Collections.Generic;

namespace Omnibasis.GoogleWallet.Demo
{
    class Program
    {
        public static string PathPrefix = "GooglePass";
        public static string DefaultLogo = "https://static.omnibasis.com/common/omnibasis-logo.png";

        static void Main(string[] args)
        {
            Console.WriteLine("Google Pay Pass Demo on Omnibasis!");

            // lets build configuration in memory
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder = configurationBuilder.AddJsonFile("C:\\Dev\\Omnibasis\\Base\\shared\\Omnibasis.GoogleWallet.Demo\\config.json", optional: false, reloadOnChange: true)
                             .AddEnvironmentVariables();
            IConfigurationRoot _appConfiguration = configurationBuilder.Build();

            // get hosting environment
            var _hostingEnvironment = new HostingEnvironment
            {
                EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                ApplicationName = AppDomain.CurrentDomain.FriendlyName,
                ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
                ContentRootFileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
            };

            // get logger 
            var loggerFactory = LoggerFactory.Create(logBuilder =>
            {
                logBuilder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Omnibasis.GoogleWallet.Demo.Program", LogLevel.Debug)
                    .AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Program>();

            // for asp.net core app, register your service
            var issuerId = _appConfiguration["DigitalPass:Google:IssuerId"];
            //app.UseGooleWalletMiddleware($"/{PathPrefix}", issuerId, _hostingEnvironment, _appConfiguration);

            // we are going build a coupon
            DigitalPassType typeId = DigitalPassType.Coupon;

            // since its our first time, we will build one from scratch. If you are upading the pass, save class Google Id in application database
            string definitionGoogleId = "";

            PassClassResourceBuilder passBuilder = getClassBuilder(typeId, definitionGoogleId, logger, _appConfiguration, _hostingEnvironment);

            // here is a specific way to cast genetic pass builder
            EventTicketClassBuilder evt = null;
            GiftCardClassBuilder gift = null;
            OfferClassBuilder offer = null;
            TransitClassBuilder transit = null;
            LoyaltyClassBuilder loyalty = null;
            FlightClassBuilder flight = null;
            if (passBuilder is EventTicketClassBuilder)
                evt = (EventTicketClassBuilder)passBuilder;
            else if (passBuilder is GiftCardClassBuilder)
                gift = (GiftCardClassBuilder)passBuilder;
            else if (passBuilder is OfferClassBuilder)
                offer = (OfferClassBuilder)passBuilder;
            else if (passBuilder is TransitClassBuilder)
                transit = (TransitClassBuilder)passBuilder;
            else if (passBuilder is LoyaltyClassBuilder)
                loyalty = (LoyaltyClassBuilder)passBuilder;
            else if (passBuilder is FlightClassBuilder)
                flight = (FlightClassBuilder)passBuilder;

            // add default logo
            passBuilder.HeroImage = new GoogleWalletImage() { Uri = DefaultLogo };
            // point to home page
            passBuilder.HomepageUri = new GoogleWalletUri() { Uri = "https://omnibasis.com", Description = "Welcome to Omnibasis" };
            // common fields
            passBuilder.IssuerName = "Omnibasis";

            // set status to active
            passBuilder.ReviewStatus = ReviewStatus.UNDER_REVIEW;
            // set optional country code
            passBuilder.CountryCode = "us";
            // set optional rule for holders
            passBuilder.MultipleDevicesAndHoldersAllowedStatus = MultipleDevicesAndHoldersAllowedStatus.MULTIPLE_HOLDERS;
            // add sample message, you can more just assign a new one and it will be stored in array
            passBuilder.Message = new GoogleWalletMessage() { Header = "Please use your coupon today!", Body = "Great discounts for valued customers" };
            // add sample text, you can more just assign a new one and it will be stored in array
            passBuilder.TextModuleData = new GoogleTextModuleData() { Header = "When you shop you win", Body = "More shopping, more offers you get" };
            // add sample link, you can more just assign a new one and it will be stored in array
            passBuilder.LinksModuleData = new GoogleWalletUri() { Description = "Learn more about our shop", Uri = "https://omnibasis.com" };
            // put additional image, you can more just assign a new one and it will be stored in array
            passBuilder.ImageModuleData = new GoogleWalletImage() { Uri = DefaultLogo };
            // add location of the store, you can more just assign a new one and it will be stored in array
            passBuilder.Location = new GoogleWalletLocation()
            {
                Latitude = 40.75212445,
                Longitude = -73.97758039
            };

            // now offer specific 
            if (offer != null)
            {
                offer.TitleImage = new GoogleWalletImage() { Uri = DefaultLogo };
                offer.RedemptionChannel = RedemptionChannel.BOTH;
                offer.Title = "Best offer ever";
                offer.FinePrint = "Use it or loose it";
                offer.Provider = "Fine shop!";
            }

            // add callback, you can test free with link = "https://free.beeceptor.com/"
            var link = "https://free.beeceptor.com/" + PathPrefix + "/?secretkey=" + DateTime.Now.Ticks.ToString();
            passBuilder.CallbackOptionsUrl = link;

            // validate pass class
            if (passBuilder != null)
            {
                try
                {
                    passBuilder.IsValid(true);
                }
                catch (ResourceBuilderException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            // time to build an object
            PassObjectResourceBuilder builderObj = null;
            // if you have build an object before, and saved it, you can use googleid to update. Here we set it to empty.
            var infoGoogleId = "";
            builderObj = getObjectBuilder(typeId, passBuilder.ClassId, infoGoogleId, logger, _appConfiguration, _hostingEnvironment);

            //set pass barcode
            builderObj.Barcode = new GoogleWalletBarcode()
            {
                AlternateText = "1234567890",
                Value = "1234567890",
                Type = BarcodeType.QR_CODE
            };

            // add app info
            // NOTE!!!: it requires special rights from google. See https://developers.google.com/pay/passes/guides/pass-verticals/loyalty/link-out-from-a-saved-google-pay-pass?hl=es
            GoogleAppInfo googleAppInfo = new GoogleAppInfo();
            googleAppInfo.Title = "My app";
            googleAppInfo.Description = "My description";
            googleAppInfo.AppTarget = "https://omnibasis.com";
            googleAppInfo.AppLogoImage = DefaultLogo;
            var appInfoTypeId = (short)AppInfoTypeId.Web;
            // for google apps, please make sure you have a google app first
            if (appInfoTypeId == (short)AppInfoTypeId.Google)
            {
                //builderObj.AndroidAppLinkInfo = googleAppInfo;
            }
            else if (appInfoTypeId == (short)AppInfoTypeId.Apple)
            {
                //builderObj.IosAppLinkInfo = googleAppInfo;
            }
            else if (appInfoTypeId == (short)AppInfoTypeId.Web)
            {
                //builderObj.WebAppLinkInfo = googleAppInfo;
            }

            // optional
            builderObj.DisableExpirationNotification = true;

            // set status
            var infoStatus = (short)DigitalPassStatus.Active;
            if (infoStatus == (short)DigitalPassStatus.Active)
                builderObj.State = ObjectState.ACTIVE;
            else if (infoStatus == (short)DigitalPassStatus.Expired)
                builderObj.State = ObjectState.EXPIRED;
            else if (infoStatus == (short)DigitalPassStatus.Void)
                builderObj.State = ObjectState.INACTIVE;
            else if (infoStatus == (short)DigitalPassStatus.Completed)
                builderObj.State = ObjectState.COMPLETED;

            // optional start date
            builderObj.StartDate = DateTime.Now;

            // optional end date
            builderObj.EndDate = DateTime.Now.AddYears(1);

            // add sample message, you can more just assign a new one and it will be stored in array
            builderObj.Message = new GoogleWalletMessage() { Header = "You, Please use your coupon today!", Body = "Great discounts for valued customers" };
            // add sample text, you can more just assign a new one and it will be stored in array
            builderObj.TextModuleData = new GoogleTextModuleData() { Header = "You, When you shop you win", Body = "More shopping, more offers you get" };
            // add sample link, you can more just assign a new one and it will be stored in array
            builderObj.LinksModuleData = new GoogleWalletUri() { Description = "You, Learn more about our shop", Uri = "https://omnibasis.com" };
            // put additional image, you can more just assign a new one and it will be stored in array
            builderObj.ImageModuleData = new GoogleWalletImage() { Uri = DefaultLogo };

            // validate object
            if (builderObj != null)
            {
                try
                {
                    builderObj.IsValid(true);
                }
                catch (ResourceBuilderException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            // get service to generate JWT
            GoogleWalletService service = getService(typeId, logger, _appConfiguration, _hostingEnvironment);

            // optional origins
            List<string> origings = new List<string>();
            origings.Add("https://omnibasis.com");

            // for full JWT that does not save class and object till the pass is saved
            //var jwt = service.GetClassGetObjectGetJwt(passBuilder, builderObj, origings);

            // to insert class and see if object exist, use
            //var jwt = service.InsertClassGetObjectGetJwt(passBuilder, builderObj, origings);

            // for very short JWT, check for class and create if neede, check object and create if needed
            var jwt = service.CreateOrUpdate(passBuilder, builderObj, origings);
            if (jwt == "")
            {
                Console.WriteLine("Failed to create JWT");
            }
            else
            {
                // you can use https://py-jwt-decoder.appspot.com/ to test JWT
                Console.WriteLine("JWT: " + jwt);

                // you can add JWT to this link to see if you can install the pass
                Console.WriteLine("URL to test: " + GoogleJwtBundle.SAVE_TO_GOOGLE + jwt);

                Console.WriteLine("Class Id: " + passBuilder.ClassId);
                Console.WriteLine("Object Id: " + builderObj.ObjectId);
            }




        }

        private static GoogleWalletService getService(DigitalPassType typeId,
            ILogger _logger,
            IConfigurationRoot _appConfiguration,
            IHostingEnvironment _env)
        {
            GoogleWalletConfig googleWalletConfig = new GoogleWalletConfig(_logger, _appConfiguration, _env);
            GoogleWalletService service = null;
            if (typeId == DigitalPassType.Generic)
                service = new GoogleWalletGiftCardService(googleWalletConfig, _logger);

            else if (typeId == DigitalPassType.Coupon)
                service = new GoogleWalletOfferService(googleWalletConfig, _logger);

            else if (typeId == DigitalPassType.BoardingPass)
                service = new GoogleWalletFlightService(googleWalletConfig, _logger);

            else if (typeId == DigitalPassType.Transit)
                service = new GoogleWalletTransitService(googleWalletConfig, _logger);

            else if (typeId == DigitalPassType.EventTicket)
                service = new GoogleWalletEventTicketService(googleWalletConfig, _logger);

            else if (typeId == DigitalPassType.StoreCard)
                service = new GoogleWalletLoyaltyService(googleWalletConfig, _logger);

            else
                service = new GoogleWalletGiftCardService(googleWalletConfig, _logger);

            return service;
        }
        private static PassClassResourceBuilder getClassBuilder(DigitalPassType typeId, string googleId,
            ILogger _logger,
            IConfigurationRoot _appConfiguration,
            IHostingEnvironment _env)
        {

            GoogleWalletService service = getService(typeId, _logger, _appConfiguration, _env);

            return service.GetClassBuilder(googleId);
        }

        private static PassObjectResourceBuilder getObjectBuilder(DigitalPassType typeId, string classId, string googleId,
            ILogger _logger,
            IConfigurationRoot _appConfiguration,
            IHostingEnvironment _env)
        {
            GoogleWalletService service = getService(typeId, _logger, _appConfiguration, _env);

            return service.GetObjectBuilder(classId, googleId);
        }

    }
}
