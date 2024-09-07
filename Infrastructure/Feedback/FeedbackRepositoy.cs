using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Feedback;
using System.IO;
using Amazon.S3.Model;
using Amazon.S3;

namespace Infrastructure.Notifications
{
    public class FeedbackRepositoy : IFeedbackRepository
    {
        private readonly SwitcherooContext db;
        private readonly IUserRepository userRepository;
        public FeedbackRepositoy(SwitcherooContext db, IUserRepository userRepository)
        {
            this.db = db;
            this.userRepository = userRepository;
        }

        public async Task<Domain.Feedback.Feedback> CreateFeedbackAsync(Domain.Feedback.Feedback feedback, Guid userId)
        {
            try
            {
                var now = DateTime.UtcNow;
                List<string>? attachments = await UploadAttachments(feedback.Attachments);
                var newDbFeedback = new Database.Schema.Feedback(
                    feedback.Title,
                    feedback.Description,
                    feedback.Status,
                    attachments
                )
                {
                    CreatedByUserId = userId,
                    UpdatedByUserId = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                await db.Feedback.AddAsync(newDbFeedback);
                await db.SaveChangesAsync();
                return await GetFeedbackById(newDbFeedback.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.InnerException}");
                throw new InfrastructureException($"Exception:{ex.Message}");
            }
        }
        private async Task<List<string>?> UploadAttachments(List<string>? attachments)
        {
            if (!(attachments?.Count() > 0))
            {
                return new List<string>();
            }

            var uploadTasks = new List<Task<string>>();
            // Additional images upload tasks
            if (!String.IsNullOrWhiteSpace(attachments[0]) && !String.IsNullOrEmpty(attachments[0]))
            {
                uploadTasks.AddRange(attachments.Select(ConvertAndUploadImageAsync));
            }
            var uploadedImageUrls = await Task.WhenAll(uploadTasks);

            List<string> s3Urls = new List<string>();
            s3Urls = uploadedImageUrls.ToList();
            return s3Urls;
        }
        private async Task<string> ConvertAndUploadImageAsync(string base64Image)
        {
            string base64 = base64Image.Split(',').LastOrDefault()?.Trim();
            byte[] imageBytes = Convert.FromBase64String(base64);

            using var image = SixLabors.ImageSharp.Image.Load(imageBytes);
            using var ms = new MemoryStream();
            var encoder = new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { Quality = 100 };
            image.Save(ms, encoder);
            byte[] webPImageBytes = ms.ToArray();

            return await UploadImageToS3Async(webPImageBytes, "image/webp");
        }
        private async Task<string> UploadImageToS3Async(byte[] imageBytes, string contentType)
        {
            string fileName = Guid.NewGuid().ToString();


            using (var s3Client = new AmazonS3Client("AKIA6EM2LZWU3ULXZ32E", "skgJAOA7bXo6aWe74nuP1UZuCbyO4UVB7t4zMei9", Amazon.RegionEndpoint.EUNorth1))
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = "switcheroofiles",
                    Key = $"{fileName}.jpg",
                    InputStream = new MemoryStream(imageBytes),
                    ContentType = contentType,
                };

                PutObjectResponse response = await s3Client.PutObjectAsync(putRequest);

                // Get the URL of the uploaded image
                return $"https://switcheroofiles.s3.eu-north-1.amazonaws.com/{fileName}.jpg"; ;
            }
        }
        public async Task<string> UpdateFeedbackStatusAsync(Guid id, Guid userId, FeedbackStatus status)
        {
            try
            {
                var now = DateTime.UtcNow;

                var dbFeedback = await db.Feedback.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (dbFeedback == null)
                    return "No feedback found for the specified id";
                dbFeedback.Status = status;
                dbFeedback.UpdatedAt = now;
                dbFeedback.UpdatedByUserId = userId;
                await db.SaveChangesAsync();
                return "Feedback status updated successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception {ex.InnerException}");
                throw new InfrastructureException($"Exception:{ex.Message}");
            }
        }

        public async Task<List<Domain.Feedback.Feedback>> GetFeedbacks()
        {
            return await db.Feedback
                .IgnoreQueryFilters()
                .Select(Database.Schema.Feedback.ToDomain)
                .ToListAsync();
        }
        public async Task<List<Domain.Feedback.Feedback>> GetFeedbacksByStatus(FeedbackStatus status)
        {
            return await db.Feedback
                .Where(x => x.Status == status)
                .Select(Database.Schema.Feedback.ToDomain)
                .ToListAsync();
        }

        public async Task<Domain.Feedback.Feedback> GetFeedbackById(Guid feedbackId)
        {
            var item = await db.Feedback
                .Where(z => z.Id == feedbackId)
                .Select(Database.Schema.Feedback.ToDomain)
                .FirstOrDefaultAsync();

            if (item == null) throw new InfrastructureException($"Unable to locate notificaitonId");

            return item;
        }
    }
}
