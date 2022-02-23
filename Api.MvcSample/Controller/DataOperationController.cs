using Api.MvcSample.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Api.MvcSample.Controller
{
    [EnableCors("CorsPolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class DataOperationController : ControllerBase
    {
        //private readonly string data = Path.GetFullPath("D:\\Projects\\Workspace\\MvcSample\\UI.MvcSample\\wwwroot");

        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly string databaseName = "\\database.json";

        public DataOperationController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/<DataOperationController>
        [HttpGet]
        public async Task<List<UserModel>> GetAsync()
        {
            List<UserModel> userModels = new List<UserModel>();
            var policy = Policy.Handle<Exception>().Retry();
            try
            {
                // DEFINE THE PATH WHERE WE WANT TO SAVE THE FILES.
                string sPath = _hostingEnvironment.WebRootPath + "\\Data";

                var result = policy.ExecuteAndCapture(async () => await System.IO.File.ReadAllTextAsync(sPath + databaseName));

                if (result.Result.IsCompletedSuccessfully)
                {
                    userModels = JsonConvert.DeserializeObject<List<UserModel>>(result.Result.Result);
                }

                if (userModels == null) userModels = new List<UserModel>();

                return userModels;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return userModels;
        }

        // POST api/<DataOperationController>
        [HttpPost]
        public async Task PostAsync([FromBody] UserModel model)
        {
            var policy = Policy.Handle<Exception>().Retry();
            try
            {
                // DEFINE THE PATH WHERE WE WANT TO SAVE THE FILES.
                string sPath = _hostingEnvironment.WebRootPath + "\\Data";

                if (!System.IO.Directory.Exists(sPath))
                    System.IO.Directory.CreateDirectory(sPath);

                if (!System.IO.File.Exists(sPath + databaseName))
                    System.IO.File.Create(sPath + databaseName);

                List<UserModel> userModels = new List<UserModel>();
                string database = await System.IO.File.ReadAllTextAsync(sPath + databaseName);
                userModels = JsonConvert.DeserializeObject<List<UserModel>>(database);

                if (userModels == null) userModels = new List<UserModel>();

                userModels.Add(model);

                var policyResult = policy.ExecuteAndCapture(async () => await System.IO.File.WriteAllTextAsync(sPath + databaseName, JsonConvert.SerializeObject(userModels)));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
