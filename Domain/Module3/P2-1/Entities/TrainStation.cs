namespace ProRental.Domain.Entities;

public partial class TrainStation
{
    // --- Public getters for private scaffolded fields ---
    public string GetTrainstationCode() => _trainstationCode;
    public string GetTrainstationName() => _trainstationName;
    public int? GetPlatform() => _platform;
    public int? GetTrainSize() => _trainSize;

    // --- Public setters for private scaffolded fields ---
    public void SetTrainstationCode(string trainstationCode) => _trainstationCode = trainstationCode;
    public void SetTrainstationName(string trainstationName) => _trainstationName = trainstationName;
    public void SetPlatform(int? platform) => _platform = platform;
    public void SetTrainSize(int? trainSize) => _trainSize = trainSize;

    // --- RDM business methods ---
    public bool CanAccommodateTrain(int requiredSize)
    {
        return (_trainSize ?? 0) >= requiredSize;
    }
}
