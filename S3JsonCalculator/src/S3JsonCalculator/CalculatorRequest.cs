namespace S3JsonCalculator;

public class CalculatorRequest
{
    public string Region { get; set; }
    public string Bucketname { get; set; }
    public string Path { get; set; }
    public string Field {get; set;}
    public string Calculation {get; set;}
    public bool Overwrite{get; set;}
}