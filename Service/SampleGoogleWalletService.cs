using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Omnibasis.GoogleWallet.Demo.Service
{
    //https://developers.google.com/pay/passes/guides/overview/how-to/use-callbacks
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
}
