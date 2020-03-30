# Omnibasis Google Wallet Pass Demo to showcase # C# and .NET Core and .Net Framework Drivers for Google Wallet Pass

C# and .NET Core and .Net Framework Demo for the library for all your Google Wallet Pass needs: create passes, updated passes, get JWT links to distribute, 
receive [de]install notifications.

**Attention:** Google Pay API Account required!
[Google Pay API account](https://developers.google.com/pay/passes/guides/get-started/basic-setup/get-access-to-rest-api)

1. You need to replace omnipass.json file with credentials file obtained from Google.
2. You need to update the following content in config.json obtained from Google.
    a. "Key": "YOUR_KEY",
    b. "AccountFile": "C:\\Path To Project File\\Omnibasis.GoogleWallet.Demo\\omnipass.json",
    c. "EmailAddress": "YOUR_SERVICE_ACCOUNT",
    d. "IssuerId": "YOUR_ISSUER_ID"

Note: if you did not replace with proper credentials, you will not be able to generate pass tokens. You can still run the demo.

## Features

1. Create, validate and update pass classes and objects:
    * Boarding passes
    * Event tickets
    * Gift cards
    * Loyalty cards
    * Offers
    * Transit passes
2. Receive notifications from Google about pass [de]installations and send updates:
    * Add `UseGooleWalletMiddleware` into your `Startup.Configure()`
    * Implement `IGoogleWalletService` for callback processing. See SampleGoogleWalletService for an example
3. If you are interested in source code access, [contact Omnibasis support](https://help.omnibasis.com)


## Demo

### 1. Configure to create passes

#### For any app we recommend to use settings files, such as appsettings.json

[Learn more about configuration files ](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
[Learn more about .NET Core configuration files](https://docs.microsoft.com/en-us/archive/msdn-magazine/2016/february/essential-net-configuration-in-net-core)

Included is a sample config.json file with this demo, note, you will need to obtain your own credentials to run this demo. Please update AccountFile path with a path to your key file provided by Google.

```csharp
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
```

### 2. Create class and object, generate JWT for a link or embeding on the page. 
[Learn how to implement Google Pay Pass distribution](https://developers.google.com/pay/passes/guides/get-started/implementing-the-api/save-to-google-pay)

#### 2.1. Create a class with builder and verify it has all the fields it needs
```csharp
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
```

#### 2.2. Create an object with builder and verify it has all the fields it needs
```csharp
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

```

#### 2.3. Request JWT token for distribution

You have 3 options for distribution.

```csharp
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


```

Code above will create this beautiful pass:

![](sample_pass.jpg)

### 3. Implementing WebService for interaction

#### 3.1. Implement IGoogleWalletService. Sample implementation in Service directory.

```csharp
 public class SampleGoogleWalletService : IGoogleWalletService
    {

        public SampleGoogleWalletService()
        {
        }


        public async Task<int> ProcessAsync(string queryParams, string classId, string objectId, string expTimeMillis, string eventType, string nonce)
        {

            // check and verify token for example
            var query = HttpUtility.ParseQueryString(queryParams);

            if (query["token"] == null)
            {
                return (int)HttpStatusCode.Unauthorized;
            }


            if (eventType == "save")
            {
                // put your save logic here
                object ret = null;
                if (ret == null)
                    return (int)HttpStatusCode.NotFound;

                return (int)HttpStatusCode.OK;
            }
            else if (eventType == "del")
            {
                // put your delete logic here
                object ret = null;
                if (ret != null)
                {
                    return (int)HttpStatusCode.OK;
                }
                else
                {
                    return (int)HttpStatusCode.NotFound;
                }
            }
            else
            {
                return (int)HttpStatusCode.BadRequest;
            }
        }



    }
```

#### 3.2. Register in `Startup`

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddSingleton<IGoogleWalletService, SampleGoogleWalletService>();
}

public void Configure(IApplicationBuilder app)
{
    ...
    app.UseGooleWalletMiddleware("/callbacks/googlewallet");
    ...
}
```

#### 3.3. Update classes and object
```csharp
// check for class and create if neede, check object and create if needed
...
service.CreateOrUpdate(passBuilder, builderObj, origings);


## Installation

Use NuGet package [Omnibasis.GoogleWallet](https://www.nuget.org/packages/GoogleWallet/)


## Running

You need `netcore2.2` or above to run build and tests;

