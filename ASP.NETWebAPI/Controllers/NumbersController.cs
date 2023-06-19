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

namespace ASP.NETWebAPI.Controllers
{
    [Route("api/numbers")]
    [ApiController]
    public class NumbersController : ControllerBase
    {
        // GET: api/numbers/init/{size}
        // Initialized two data sets A and B with a given size. Each dataset will contain size x size elements.
        [HttpGet("init/{size}")]
        public string InitializeDatasets([Required] int size)
        {
            var data = GenerateDatasets(size);

            return CreateJsonFile(data);
        }

        // GET api/numbers/{dataset}/{type}/{idx}
        // Retrieves row / col of data
        [HttpGet("/{dataset}/{type}/{idx}")]
        public ActionResult<List<int>> GetDatasetByIndex([Required] string dataset, [Required] string type, [Required] int idx)
        {
            Matrices? matrices = GetDataFromJsonFile();
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

            var hash = ConcatenatedProductString();
            var resultString = hash.Aggregate(new StringBuilder(), (s, i) => s.Append(i.ToString())).ToString();
            if (resultString == md5HashValue)
                return true;
            else
                return false;
        }

        private Matrices? GetDataFromJsonFile()
        {
            string folder = Directory.GetCurrentDirectory() + @"\";
            string fileName = "Matrices.json";
            string fullPath = folder + fileName;
            if (System.IO.File.Exists(fullPath))
            {
                string content = System.IO.File.ReadAllText(fullPath);
                return JsonConvert.DeserializeObject<Matrices>(content);
            }
            else
                return null;
        }

        private string CalculateMD5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes); 
            }
        }

        private string ConcatenatedProductString()
        {
            var resultMatrix = MatrixMultiplication();

            StringBuilder concat = new StringBuilder();
            for (int i = 0; i < resultMatrix.GetLength(0); ++i)
            {
                StringBuilder concatRow = new StringBuilder();
                for (int j = 0; j < resultMatrix.GetLength(1); ++j)
                    concatRow.Append(resultMatrix[i,j]);
                concat.Append(concatRow);
            }

            var hash = CalculateMD5Hash(concat.ToString());

            return hash;
        }

        private int[,]? MatrixMultiplication()
        {
            Matrices? matrices = GetDataFromJsonFile();
            if (matrices != null)
            {
                var matrixA = matrices?.MatrixData?
                            .FirstOrDefault(x => x.MatrixName != null && x.MatrixName.Equals("A"))?.Value;

                var matrixB = matrices?.MatrixData?
                            .FirstOrDefault(x => x.MatrixName != null && x.MatrixName.Equals("B"))?.Value;

                if(matrixA == null || matrixB == null)
                    throw new Exception("Data missing. Please initialize datasets.");
                else if (matrixA != null && matrixB != null && matrixA[0].Count != matrixB.Count)
                    throw new Exception("Matrix multiplication is not possible. # of columns of first matrix must equal to # of rows of second matrix");
                else
                {
                    var size = matrixA[0].Count;
                    //List<List<int>> result = new List<List<int>>();
                    int[,] result = new int[size, size];

                    // looping through matrix A rows  
                    for (int matrixA_row = 0; matrixA_row < size; ++matrixA_row)
                    {
                        // for each matrix A row, loop through matrix B columns  
                        for (int matrixB_col = 0; matrixB_col < size; ++matrixB_col)
                        {
                            // loop through matrix A columns to calculate the dot product  
                            for (int matrixA_col = 0; matrixA_col < size; ++matrixA_col)
                            {
                                result[matrixA_row,matrixB_col] +=
                                  matrixA[matrixA_row][matrixA_col] *
                                  matrixB[matrixA_col][matrixB_col];
                            }
                        }
                    }

                    return result;

                }
            }
            else
                return null; // NotFound("Data not found. Please initialize datasets.");
        }

        private Matrices GenerateDatasets(int size)
        {
            Random random = new Random();

            //Matrix A
            List<List<int>> matrixA = new List<List<int>>(size);
            for (int i = 0; i < size; ++i)
            {
                List<int> matrix = new List<int>(size);
                for (int j = 0; j < size; ++j)
                    matrix.Add(random.Next(10));
                matrixA.Add(matrix);
            }

            //Matrix B
            List<List<int>> matrixB = new List<List<int>>(size);
            for (int i = 0; i < size; ++i)
            {
                List<int> matrix = new List<int>(size);
                for (int j = 0; j < size; ++j)
                    matrix.Add(random.Next(10));
                matrixB.Add(matrix);
            }

            return new Matrices
            {
                MatrixData = new MatrixData[] {
                        new MatrixData
                        {
                            MatrixName = "A",
                            Value = matrixA
                        },
                        new MatrixData
                        {
                            MatrixName = "B",
                            Value = matrixB
                        }
                }
            };

        }

        private string CreateJsonFile(Matrices data)
        {

            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            string folder = Directory.GetCurrentDirectory() + @"\";
            string fileName = "Matrices.json";
            string fullPath = folder + fileName;

            if (!System.IO.File.Exists(fullPath))
                System.IO.File.WriteAllText(fullPath, json);
            else
            {
                System.IO.File.Delete(fullPath);
                System.IO.File.WriteAllText(fullPath, json);
            }

            return json;
        }
    }
}
