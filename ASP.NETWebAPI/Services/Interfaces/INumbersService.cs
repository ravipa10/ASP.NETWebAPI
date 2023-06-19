using ASP.NETWebAPI.DataContracts;

namespace ASP.NETWebAPI.Services.Interfaces
{
    public interface INumbersService
    {
        Matrices GenerateDatasets(int size);
        string CreateJsonFile(Matrices data);
        Matrices? GetDataFromJsonFile();
        string ConcatenateAndHashCrossProduct();
    }
}
