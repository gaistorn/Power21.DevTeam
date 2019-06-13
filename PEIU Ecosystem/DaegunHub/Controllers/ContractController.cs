using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using PES.Models;

namespace DaegunHub.Controllers
{
    [Route("api/contract")]
    [Produces("application/json")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        ILogger<ContractController> logger;
        MongoDB.Driver.MongoClient mongoClient;
        MongoDB.Driver.IMongoDatabase monogodb;
        public ContractController()//MongoDB.Driver.MongoClient _client, ILoggerFactory loggerFactory)
        {
            //logger = loggerFactory.CreateLogger<ContractController>();
            //mongoClient = _client;
            //monogodb = mongoClient.GetDatabase("PEIU");
        }

        [HttpPost("register")]
        public async Task<IActionResult> registercontract([FromBody] ContractModel model)
        {
            return Ok();
        }

        [HttpGet("getservicelist")]
        public async Task<IActionResult> GetServiceList()
        {
            ContractModel model = new ContractModel();
            string strmodel = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            var cols = monogodb.GetCollection<ServiceModel>("ContractInfo");
            
            ServiceModel[] defaultModels = new ServiceModel[]
            {
                new ServiceModel() { ServiceCode = 100, ServiceName = "스케쥴링", Describe = "스케쥴링 알고리즘"},
                new ServiceModel() { ServiceCode = 101, ServiceName = "Peak-cut", Describe = "피크컷 알고리즘"},
                new ServiceModel() { ServiceCode = 102, ServiceName = "주파수 조정(Frequency Regulation)", Describe = "주파수 조정 알고리즘"},
                new ServiceModel() { ServiceCode = 100, ServiceName = "DR", Describe = "수요반응"}
            };

            await cols.InsertManyAsync(defaultModels);

            IAsyncCursor<ServiceModel> cursor = await cols.FindAsync<ServiceModel>(null);
            JArray jArray = new JArray();
            
            //List<ServiceModel> result_models = new List<ServiceModel>();
            await cursor.ForEachAsync(db => jArray.Add(db));
            return Ok(jArray);

        }

        // GET: api/Contract
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Contract/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

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
