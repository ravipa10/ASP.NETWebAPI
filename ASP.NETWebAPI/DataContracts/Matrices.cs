using System.Runtime.Serialization;

namespace ASP.NETWebAPI.DataContracts
{
    [DataContract]
    public class Matrices
    {
        [DataMember]
        public MatrixData[]? MatrixData { get; set; }
    }

    [DataContract]
    public class MatrixData
    {
        [DataMember]
        public string? MatrixName { get; set; }

        [DataMember]
        public List<List<int>>? Value { get; set; }

    }
}
