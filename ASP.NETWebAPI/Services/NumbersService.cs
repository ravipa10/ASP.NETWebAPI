﻿using ASP.NETWebAPI.Models;
using ASP.NETWebAPI.Services.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace ASP.NETWebAPI.Services
{
    public class NumbersService : INumbersService
    {
        public Matrices GenerateDatasets(int size)
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

        public string CreateJsonFile(Matrices data)
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
        public Matrices? GetDataFromJsonFile()
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

        public string ConcatenateAndHashCrossProduct()
        {
            var resultMatrix = MatrixMultiplication();

            //Concatenate result from left to right and then top to bottom
            StringBuilder concat = new StringBuilder();
            for (int i = 0; i < resultMatrix.GetLength(0); ++i)
            {
                StringBuilder concatRow = new StringBuilder();
                for (int j = 0; j < resultMatrix.GetLength(1); ++j)
                    concatRow.Append(resultMatrix[i, j]);
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

                if (matrixA == null || matrixB == null)
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
                                result[matrixA_row, matrixB_col] +=
                                  matrixA[matrixA_row][matrixA_col] *
                                  matrixB[matrixA_col][matrixB_col];
                            }
                        }
                    }

                    return result;

                }
            }
            else
                return null;
        }
    }
}
