using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using rpa_functions.rpa_pc35;
using rpa_functions.rpa_pc269;
using System.Linq;
using System.Threading;

namespace rpa_functions
{

    public class PC269_Webservice
    {
        private readonly PC269Context _context;
        public PC269_Webservice(PC269Context context)
        {
            _context = context;
        }

        [FunctionName("PC269_GetAssets")]
        public IActionResult GetAssets(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "PC269_GetAssets")] HttpRequest req,
            ILogger log)
        { 
            log.LogInformation("PC269 GetAssets called");

            var assetArray = _context.Assets.OrderBy(p => p.asset_name).ToArray();
            
            return new OkObjectResult(assetArray);

        }


        [FunctionName("PC269_PostAsset")]
        public async Task<IActionResult> PostAssetAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PC269_PostAsset")] HttpRequest req, 
            CancellationToken cts,
            ILogger log)
        {
            log.LogInformation("PC269 post assets  request received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Asset newAsset = JsonConvert.DeserializeObject<Asset>(requestBody);

            var entity = await _context.Assets.AddAsync(newAsset, cts);

            await _context.SaveChangesAsync(cts);

            return new OkObjectResult(JsonConvert.SerializeObject(entity.Entity));

        }


        [FunctionName("PC269_PostDailyReport")]
        public async Task<IActionResult> PostDailyReportAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PC269_PostDailyReport/{asset_id}")] HttpRequest req,
            int asset_id,
            CancellationToken cts,
            ILogger log)
        {
            log.LogInformation("PC269 post daily report  request received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            DailyReport newDailyReport = JsonConvert.DeserializeObject<DailyReport>(requestBody);

            newDailyReport.asset_Id = asset_id;

            var entity = await _context.DailyReports.AddAsync(newDailyReport, cts);

            await _context.SaveChangesAsync(cts);

            return new OkObjectResult(JsonConvert.SerializeObject(entity.Entity));

        }

        [FunctionName("PC269_PostWelltest")]
        public async Task<IActionResult> PostWellsTestAsync(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PC269_PostWellstest/{dailyreport_id}")] HttpRequest req,
          int dailyreport_id,
          CancellationToken cts,
          ILogger log)
        {
            log.LogInformation("PC269 post wellstest  request received");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            WellsTest newWellstest = JsonConvert.DeserializeObject<WellsTest>(requestBody);

            newWellstest.dailyreport_Id = dailyreport_id;

            var entity = await _context.WellsTests.AddAsync(newWellstest, cts);

            await _context.SaveChangesAsync(cts);

            return new OkObjectResult(JsonConvert.SerializeObject(entity.Entity));

        }

        // Comment

        // Waterinjection

        // Gasinjection


    }
    public static class PC35_Auth
    {
        [FunctionName("PC35_Auth")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("PC35 Auth request handled");

            // Dummy service to enable token generation.
            return (ActionResult)new OkObjectResult("RPA Authentication successful");

        }
    }

    public static class PC35_Webservice
    {
        [FunctionName("PC35_Webservice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", "patch", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("PC35 Webservice received a request");

            // Get HTTP data
            dynamic bodyData = JsonConvert.DeserializeObject(await new StreamReader(req.Body).ReadToEndAsync());
            string method = req.Method;

            // Setup table operations object
            PackCheckTableOperations packTable = new PackCheckTableOperations();

            switch(method)
            {
                case "GET":
                    // Get pending packages (robot initiated)
                    log.LogInformation("GET Request");

                    string result_get = packTable.QueryPendingPackages().Result;

                    return result_get != null
                        ? (ActionResult)new OkObjectResult(result_get)
                        : new BadRequestObjectResult("No Matches");

                case "POST":
                    // Insert new packages (user initiatied)
                    log.LogInformation("POST Request");

                    string result_post = null;

                    if (bodyData != null) result_post = await packTable.InsertBatch(bodyData);

                    return result_post != null
                        ? (ActionResult)new OkObjectResult(result_post)
                        : new BadRequestObjectResult("Not valid input");

                case "PATCH":
                    log.LogInformation("PATCH Request");

                    bool result_patch = false;

                    if(bodyData.Id != "")
                    {
                        result_patch = await packTable.UpdatePackage(bodyData);

                    }
                    

                    return result_patch != false
                        ? (ActionResult)new OkObjectResult("Success")
                        : new BadRequestObjectResult("Not valid input");


                default:
                    log.LogError("Invalid HTTP method");
                    return new BadRequestObjectResult("Not implemented, go away");
            }
            

        }
    }

    public static class PC185_Webservice
    {
        [FunctionName("PC185_Webservice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", "patch", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("PC185 received a webservice call");

            string name = req.Query["name"];

            dynamic bodyData = JsonConvert.DeserializeObject(await new StreamReader(req.Body).ReadToEndAsync());
            string method = req.Method;

            // Setup table operations object
            PackCheckTableOperations packTable = new PackCheckTableOperations();

            switch (method)
            {
                case "GET":
                    // Get pending serials 
                    log.LogInformation("GET Request");

                    string result_get = null;
                    string wakeup = req.Query["wakeup"];


                    if (wakeup == "true") result_get = "Waking up webservice...";
                    else result_get = packTable.QueryPendingPackages().Result;


                    return result_get != null
                        ? (ActionResult)new OkObjectResult(result_get)
                        : new BadRequestObjectResult("No Matches");

                case "POST":
                    // Insert new packages (user initiatied)
                    log.LogInformation("POST Request");

                    string result_post = null;

                    if (bodyData != null) result_post = await packTable.InsertBatch(bodyData);

                    return result_post != null
                        ? (ActionResult)new OkObjectResult(result_post)
                        : new BadRequestObjectResult("Not valid input");

                case "PATCH":
                    log.LogInformation("PATCH Request");

                    bool result_patch = false;

                    if (bodyData.Id != "")
                    {
                        result_patch = await packTable.UpdatePackage(bodyData);

                    }


                    return result_patch != false
                        ? (ActionResult)new OkObjectResult("Success")
                        : new BadRequestObjectResult("Not valid input");


                default:
                    log.LogError("Invalid HTTP method");
                    return new BadRequestObjectResult("Not implemented, go away");
            }

        }
    }
}
