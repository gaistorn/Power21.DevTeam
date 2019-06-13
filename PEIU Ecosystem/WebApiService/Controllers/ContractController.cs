using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using PES.Models;
using PES.Service.WebApiService.Localization;
using Power21.PEIUEcosystem.Models;

namespace PES.Service.WebApiService.Controllers
{
    [Route("api/contract")]
    [Produces("application/json")]
    //[Authorize]
    //[EnableCors(origins: "http://www.peiu.co.kr:3011", headers: "*", methods: "*")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly UserManager<AccountModel> userManager;
        ILogger<ContractController> logger;
        MongoDB.Driver.MongoClient mongoClient;
        MongoDB.Driver.IMongoDatabase monogodb;
        AccountRecordContext accountContext;
        private readonly LocalizedIdentityErrorDescriber describer;
        public ContractController(IConfiguration configuration, ILoggerFactory loggerFactory, AccountRecordContext _accountContext, UserManager<AccountModel> _userManager)
        {
            logger = loggerFactory.CreateLogger<ContractController>();
            string conn_str = configuration.GetConnectionString("mongodb");
            mongoClient = new MongoDB.Driver.MongoClient(conn_str);
            monogodb = mongoClient.GetDatabase("PEIU");
            accountContext = _accountContext;
            userManager = _userManager;
            describer = userManager.ErrorDescriber as LocalizedIdentityErrorDescriber;
        }

        //[HttpPost("register")]
        //public async Task<IActionResult> RegisterContract([FromBody] ContractModel param)
        //{
        //    var cols = monogodb.GetCollection<ContractModel>("ContractInfo");
        //    await cols.InsertOneAsync(param);

        //    return Ok();
        //}

        

        [HttpPost("inputcontract")]
        public async void GetContractorList([FromBody] ContractModel model)
        {

            var cols = monogodb.GetCollection<ContractModel>("ContractInfo");
            await cols.InsertOneAsync(model);
        }

        [HttpPost("AppendAsset")]
        public async Task<IActionResult> AppendAsset(string email, [FromBody] AssetModel addAssets)
        {
            var user = userManager.FindByEmailAsync(email);
            if(user == null)
            {
                return BadRequest(LocalizedIdentityErrorDescriber.UserNotFound(describer));
            }

            AssetDBModel asset = new AssetDBModel();
            asset.AssetName = addAssets.AssetName;
            asset.DLNo = addAssets.DLNo;
            asset.InstallDate = addAssets.InstallDate;
            asset.ServiceCode = addAssets.ServiceCode;
            asset.SiteId = addAssets.SiteId;
            asset.TotalAvaliableESSMountKW = addAssets.TotalAvaliableESSMountKW;
            asset.TotalAvaliablePCSMountKW = addAssets.TotalAvaliablePCSMountKW;
            asset.TotalAvaliablePVMountKW = addAssets.TotalAvaliablePVMountKW;

            AddressModel address = new AddressModel();
            LocationModel add_address = addAssets.Address;
            address.Address1 = add_address.Address1;
            address.Address2 = add_address.Address2;
            address.Latitude = add_address.Latitude;
            address.Longtidue = add_address.Longtidue;
            address.LawFirstCode = add_address.LawFirstCode;
            address.LawMiddleCode = add_address.LawMiddleCode;
            address.LawLasttCode = add_address.LawLasttCode;
            address.RCC = add_address.RCC;
            asset.Address = address;

            accountContext.Address.Add(address);
            accountContext.Assets.Add(asset);
            await accountContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("getsitedbyrcc")]
        public async Task<IActionResult> getsitedbyrcc()
        {
            JArray result = new JArray();
            var rcc_keys = PMSController.rcc_list.Keys.OrderBy(x => x);
            foreach(int rcc in rcc_keys)
            {
                JObject obj = new JObject();
                obj.Add("rcc", rcc);
                obj.Add("name", PMSController.rcc_list[rcc]);

                if (PMSController.RccBySiteMap.ContainsKey(rcc))
                {
                    obj.Add("sites", JArray.FromObject( PMSController.RccBySiteMap[rcc]));
                }
                else
                {
                    obj.Add("sites", JArray.FromObject(new int[0]));
                }
                result.Add(obj);
            }
            return Ok(result);
        }

        [HttpGet("getcontractorlist")]
        public async Task<IActionResult> GetContractorList(string modeltypecode)
        {
            ContractModel testModel = new ContractModel();
            testModel.Assets.Add(new AssetModel());

            string model =  Newtonsoft.Json.JsonConvert.SerializeObject(testModel);

            var cols = monogodb.GetCollection<ContractModel>("ContractInfo");
            var filter = "{ ModelTypeCode: '" + modeltypecode + "'}";
            IAsyncCursor<ContractModel> cursor = await cols.FindAsync(filter);
            JArray jArray = new JArray();

            //List<ServiceModel> result_models = new List<ServiceModel>();
            await cursor.ForEachAsync(db =>
            {
                JObject obj = JObject.FromObject(db);
                jArray.Add(obj);
            });
            return Ok(jArray);
        }

        [HttpGet("getservicelist")]
        public async Task<IActionResult> GetServiceList()
        {
            var cols = monogodb.GetCollection<ServiceModel>("ContractInfo");

            //ServiceModel[] defaultModels = new ServiceModel[]
            //{
            //    new ServiceModel() { ServiceCode = 100, ServiceName = "스케쥴링", Describe = "스케쥴링 알고리즘"},
            //    new ServiceModel() { ServiceCode = 101, ServiceName = "Peak-cut", Describe = "피크컷 알고리즘"},
            //    new ServiceModel() { ServiceCode = 102, ServiceName = "주파수 조정(Frequency Regulation)", Describe = "주파수 조정 알고리즘"},
            //    new ServiceModel() { ServiceCode = 100, ServiceName = "DR", Describe = "수요반응"}
            //};

            //await cols.InsertManyAsync(defaultModels);

            var filter = "{ ModelTypeCode: '3'}";
            IAsyncCursor<ServiceModel> cursor = await cols.FindAsync(filter);
            JArray jArray = new JArray();

            //List<ServiceModel> result_models = new List<ServiceModel>();
            await cursor.ForEachAsync(db =>
            {
                JObject obj = JObject.FromObject(db);
                jArray.Add(obj);
            });
            return Ok(jArray);

        }

        //// GET: api/Contract
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET: api/Contract/5
        //[HttpGet("{id}", Name = "Get")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST: api/Contract
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Contract/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
