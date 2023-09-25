using System.Text.Json;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Linq;
using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using System.Text;
using Adletec.Sonic;

namespace S3JsonCalculator;

public class Calculator
{
    public void Run(CalculatorRequest Request)
    {
        // Read source content from S3 file
        string jsonString = GetFileContent(Request.Region,
            Request.Bucketname, Request.Path);

        JContainer sourceDocument = (JContainer)JToken.Parse(jsonString);

        var nodes = sourceDocument.Descendants()
                         .OfType<JObject>();

        foreach (var n in nodes)
        {
            var variables = new Dictionary<string, double>();

            foreach (var p in n.Children<JProperty>())
            {
                if (double.TryParse(p.Value.ToString(), out double d))
                {
                    variables.Add(p.Name, d);
                }
            }

            var engine = Evaluator.CreateWithDefaults();
            double result = engine.Evaluate(Request.Calculation, variables);

            // Add computed field
            if (n[Request.Field] != null)
            {
                if (Request.Overwrite)
                {
                    n[Request.Field] = result;
                }
            }
            else
            {
                n.Add(Request.Field, result);
            }
        }

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(sourceDocument.ToString()));

        SaveFile(Request.Region, Request.Bucketname, Request.Path, stream, "application/json");
    }

    public void SaveFile(string Region, string Bucketname,
    string Key, Stream Stream, string ContentType)
    {
        var region = RegionEndpoint.GetBySystemName(Region);

        var _client = new AmazonS3Client(region);

        var putRequest = new PutObjectRequest
        {
            BucketName = Bucketname,
            Key = Key,
            ContentType = ContentType,
            InputStream = Stream
        };

        _client.PutObjectAsync(putRequest).Wait();
    }

    public string GetFileContent(string Region, string Bucketname, string Key)
    {
        var region = RegionEndpoint.GetBySystemName(Region);

        var _client = new AmazonS3Client(region);

        var request = new GetObjectRequest();
        request.BucketName = Bucketname;
        request.Key = Key;

        GetObjectResponse response = _client.GetObjectAsync(request).Result;
        StreamReader reader = new StreamReader(response.ResponseStream);
        string content = reader.ReadToEnd();
        return content;
    }

}