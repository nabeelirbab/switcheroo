using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Version
{
    public class AppVersion
    {
        public AppVersion(Guid? id, string androidVersion, string iosVersion, Guid? createdByUserId, Guid? updatedByUserId)
        {
            Id = id;
            AndroidVersion = androidVersion;
            IOSVersion = iosVersion;
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
        }

        [Required]
        public Guid? Id { get; set; }
        [Required]
        public string AndroidVersion { get; set; }
        [Required]
        public string IOSVersion { get; set; }
        public DateTime? CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }


        public static AppVersion CreateNewAppVersion(
            string androidVersion,
            string iosVersion,
            Guid createdByUserId
        )
        {
            return new AppVersion(
                null,
                androidVersion,
                iosVersion,
                createdByUserId,
                createdByUserId
            )
            {
                CreatedAt = DateTime.Now
            };
        }
    }
}
