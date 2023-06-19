using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.IO;
using System.Drawing;
using System.Net.Mail;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Data;
using ASP.NETWebAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Security.Principal;
using System;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Net.NetworkInformation;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Numerics;
using ASP.NETWebAPI.Services.Interfaces;

namespace ASP.NETWebAPI.Controllers
{
    [Route("api/numbers")]
    [ApiController]
    public class NumbersController : ControllerBase
    {
        private readonly INumbersService _numbersService;
        public NumbersController(INumbersService numbersService) 
        {
            _numbersService = numbersService;
        }


        // GET: api/numbers/init/{size}
        // Initialized two data sets A and B with a given size. Each dataset will contain size x size elements.
        [HttpGet("init/{size}")]
        public string InitializeDatasets([Required] int size)
        {
            var data = _numbersService.GenerateDatasets(size);

            return _numbersService.CreateJsonFile(data);
        }

        // GET api/numbers/{dataset}/{type}/{idx}
        // Retrieves row / col of data
        [HttpGet("/{dataset}/{type}/{idx}")]
        public ActionResult<List<int>> GetDatasetByIndex([Required] string dataset, [Required] string type, [Required] int idx)
        {
            Matrices? matrices = _numbersService.GetDataFromJsonFile();
            if (matrices != null)
            {
                var data = matrices?.MatrixData?
                            .FirstOrDefault(x => x.MatrixName != null && x.MatrixName.Equals(dataset, StringComparison.OrdinalIgnoreCase))?.Value;

                if (data != null)
                {
                    if (type == "row")
                        return Ok(data.Select((s, i) => new { i, s }).Where(x => x.i == idx).SelectMany(x => x.s));
                    else if (type == "col")
                    {
                        List<int> col = new List<int>(data.Count);
                        for (int i = 0; i < data.Count; ++i)
                            col.Add(data[i][idx]);

                        return Ok(col);
                    }
                    else
                        return BadRequest("Please use only valid type values: row, col");
                }
                else 
                    return NotFound("Please initialize datasets. If done, please use only valid dataset names: A, B.");
            }
            else
                return NotFound("Data not found. Please initialize datasets.");
        }

        // POST api/numbers/validate
        // Validates calculation based on MD5 hash of all values in the new dataset.
        [HttpPost("validate")]
        public bool ValidateCalculation([FromBody][Required] string md5HashValue)
        {

            var hash = _numbersService.ConcatenateAndHashCrossProduct();
            var resultString = hash.Aggregate(new StringBuilder(), (s, i) => s.Append(i.ToString())).ToString();
            if (resultString == md5HashValue)
                return true;
            else
                return false;
        }

       
    }
}
