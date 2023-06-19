using System.Runtime.Serialization;

namespace ASP.NETWebAPI.DataContracts
{
    [DataContract]
    public class ApiResponse<T>
    {
        [DataMember]
        public T? Value { get; set; }

        [DataMember]
        public string? Cause { get; set; }

        [DataMember]
        public bool Success { get; set; }
    }
}
