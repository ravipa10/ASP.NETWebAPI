using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Data;
using ASP.NETWebAPI.DataContracts;
using Newtonsoft.Json;
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
        public ActionResult<ApiResponse<MatrixData[]>> InitializeDatasets([Required] int size)
        {
            try
            {
                var data = _numbersService.GenerateDatasets(size);

                var datasets = _numbersService.CreateJsonFile(data);

                var response =  new ApiResponse<MatrixData[]>
                {
                    Value = data?.MatrixData,
                    Cause = null,
                    Success = true,
                };

                return Ok(response);
            }
            catch(Exception ex)
            {
                var response =  new ApiResponse<MatrixData[]>
                {
                    Value = null,
                    Cause = "An unexpected error occurred during initializing datasets" + ex.ToString(),
                    Success = false,
                };
                return BadRequest(response);
            }
        }

        // GET api/numbers/{dataset}/{type}/{idx}
        // Retrieves row / col of data
        [HttpGet("/{dataset}/{type}/{idx}")]
        public ActionResult<ApiResponse<IEnumerable<int>>> GetDatasetByIndex([Required] string dataset, [Required] string type, [Required] int idx)
        {
            try
            {
                Matrices? matrices = _numbersService.GetDataFromJsonFile();
                if (matrices != null)
                {
                    var data = matrices?.MatrixData?
                                .FirstOrDefault(x => x.MatrixName != null && x.MatrixName.Equals(dataset, StringComparison.OrdinalIgnoreCase))?.Value;

                    if (data != null)
                    {
                        IEnumerable<int> collection;
                        if (type == "row")
                            collection =  data.Select((s, i) => new { i, s }).Where(x => x.i == idx).SelectMany(x => x.s);
                        else if (type == "col")
                        {
                            List<int> col = new List<int>(data.Count);
                            for (int i = 0; i < data.Count; ++i)
                                col.Add(data[i][idx]);

                            collection = col;
                        }
                        else
                            return BadRequest("Please use only valid type values: row, col");

                        var response = new ApiResponse<IEnumerable<int>>
                        {
                            Value = collection,
                            Cause = null,
                            Success = true,
                        };
                        return Ok(response);
                    }
                    else
                        return NotFound("Please initialize datasets. If done, please use only valid dataset names: A, B.");
                }
                else
                    return NotFound("Data not found. Please initialize datasets.");
            }
            catch(Exception ex)
            {
                var response =  new ApiResponse<List<int>>
                {
                    Value = null,
                    Cause = $"An unexpected error occurred while fetching dataset {dataset} by {type} index {idx}" + ex.ToString(),
                    Success = false,
                };
                return BadRequest(response);
            }
        }

        // POST api/numbers/validate
        // Validates calculation based on MD5 hash of all values in the new dataset.
        [HttpPost("validate")]
        public ActionResult<ApiResponse<string>> ValidateCalculation([FromBody][Required] string md5HashValue)
        {
            try
            {
                var hash = _numbersService.ConcatenateAndHashCrossProduct();
                var resultString = hash.Aggregate(new StringBuilder(), (s, i) => s.Append(i.ToString())).ToString();

                var response = new ApiResponse<string>
                {
                    Value = resultString == md5HashValue ? "Calculation valid. Computed md5 hash matches request body." : "Calculation invalid. Computed md5 hash does not match request body.",
                    Cause = null,
                    Success = true,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ApiResponse<string>
                {
                    Value = null,
                    Cause = $"An unexpected error occurred while validating the cross product" + ex.ToString(),
                    Success = false,
                };
                return BadRequest(response);
            }
        }

       
    }
}
