namespace ASP.NETWebAPI.Models
{
    public class Matrices
    {
        public MatrixData[]? MatrixData { get; set; }
    }
    public class MatrixData
    {
        public string? MatrixName { get; set; }
        public List<List<int>>? Value { get; set; }

    }
}
