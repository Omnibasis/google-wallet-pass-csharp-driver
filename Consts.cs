namespace Omnibasis.GoogleWallet.Demo
{
    public enum DigitalPassType
    {
        Generic = 1,
        StoreCard = 2,
        EventTicket = 3,
        Coupon = 4,
        BoardingPass = 5,
        Transit = 6
    }

    public enum AppInfoTypeId
    {
        Apple,
        Google,
        Web
    }

    public enum DigitalPassStatus
    {
        Active = 1,
        Expired = 2,
        Void = 3,
        Completed
    }

    public class GoogleJwtBundle
    {
        public const string SAVE_TO_GOOGLE = "https://pay.google.com/gp/v/save/";
        public string Jwt { get; set; }
        public string ClassId { get; set; }
        public string ObjectId { get; set; }


    }
}
