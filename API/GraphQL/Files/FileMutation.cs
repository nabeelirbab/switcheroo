using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace API.GraphQL;
public class ResponseDto
{
    public string Message { get; set; }
    public string Data { get; set; }
    public bool Success { get; set; }
}

public class UploadFileRequestDto
{
    public string Data { get; set; }
    public string FileName { get; set; }
}
public partial class Mutation
{
    public async Task<ResponseDto> UploadFile(UploadFileRequestDto request)
    {
        try
        {
            var name = Guid.NewGuid().ToString("N") + "-" + request.FileName;
            var bytes = Convert.FromBase64String(request.Data.Split(',').Last());
            var stream = new MemoryStream(bytes);
            var credentials = new Amazon.Runtime.BasicAWSCredentials("AKIA6EM2LZWU3ULXZ32E", "skgJAOA7bXo6aWe74nuP1UZuCbyO4UVB7t4zMei9");
            var config = new AmazonS3Config { RegionEndpoint = Amazon.RegionEndpoint.EUNorth1 };
            var s3Client = new AmazonS3Client(credentials, config);
            var transferUtility = new TransferUtility(s3Client);
            string bucketName = "switcheroofiles";  
            string keyName = "https://switcheroofiles.s3.eu-north-1.amazonaws.com/" + name; 
            await transferUtility.UploadAsync(stream, bucketName, name);
            return new ResponseDto()
            {
                Data = keyName,
                Success = true
            };
        }
        catch (Exception e)
        {
            return new ResponseDto()
            {
                Message = e.Message,
                Success = false
            };
        }
    }
}