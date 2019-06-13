﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PES.Toolkit;
using StackExchange.Redis;

namespace DaegunHub.Controllers
{
    [Route("api/pms")]
    [Produces("application/json")]
    [ApiController]
    public class PMSController : ControllerBase
    {
        static ILogger logger;
        static IDatabase _db;
        public PMSController(ILoggerFactory loggerFactory, IRedisConnectionFactory redisConnectionFactory)
        {
            logger = loggerFactory.CreateLogger<PMSController>();
            _db = redisConnectionFactory.Connection().GetDatabase();

        }

        // GET: api/PMS
        [HttpGet("pcs")]
        public async Task<IActionResult> getpcsdata(int siteid)
        {
            JObject getModel = await GetJObject(siteid, "PCS");
            if (getModel == null)
            {
                return Ok(StatusCodes.Status400BadRequest);
            }
            else
                return Ok(getModel);
        }

        // GET: api/PMS
        [HttpGet("bsc")]
        public async Task<IActionResult> getbscdata(int siteid)
        {
            JObject getModel = await GetJObject(siteid, "BSC");
            if (getModel == null)
            {
                return Ok(StatusCodes.Status400BadRequest);
            }
            else
                return Ok(getModel);
        }

        // GET: api/PMS
        [HttpGet("ess")]
        public async Task<IActionResult> getessdata(int siteid)
        {
            JObject getModel = await GetJObject(siteid, "ESSMETER");
            if (getModel == null)
            {
                return Ok(StatusCodes.Status400BadRequest);
            }
            else
                return Ok(getModel);
        }

        // GET: api/PMS
        [HttpGet("pv")]
        public async Task<IActionResult> getpvdata(int siteid)
        {
            JObject getModel = await GetJObject(siteid, "PVMETER");
            if (getModel == null)
            {
                return Ok(StatusCodes.Status400BadRequest);
            }
            else
                return Ok(getModel);
        }

        // GET: api/Contract
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/PMS/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/PMS
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/PMS/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private Task<JObject> GetJObject(int siteid, string deviceid)
        {
            return Task<JObject>.Run<JObject>(async () =>
            {
                JObject obj = new JObject();
                try
                {
                    string key = $"site_{siteid}_{deviceid}";
                    bool isExist = await _db.KeyExistsAsync(key);
                    if(isExist == false)
                    {
                        return null;
                    }
                    HashEntry[] entries = await _db.HashGetAllAsync(key);

                    obj.Add("request_id", key);
                    foreach (HashEntry entry in entries)
                    {
                        double dValue;
                        long lValue;
                        int iValue;

                        if (entry.Value.TryParse(out iValue))
                            obj.Add(entry.Name, iValue);
                        else if (entry.Value.TryParse(out lValue))
                            obj.Add(entry.Name, lValue);
                        else if (entry.Value.TryParse(out dValue))
                            obj.Add(entry.Name, dValue);
                        else
                            obj.Add(entry.Name, entry.Value.ToString());
                    }

                }
                catch (Exception ex)
                {
                    obj.Add("Error type", ex.GetType().ToString());
                    obj.Add("Error Message", ex.Message);
                }
                return obj;
            });

        }
    }
}
