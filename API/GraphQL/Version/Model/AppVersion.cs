using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace API.GraphQL.Model
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
        public DateTime? UpdatedAt { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }


        public static AppVersion FromDomain(Domain.Version.AppVersion domainEntity)
        {
            if (!domainEntity.Id.HasValue) throw new ApiException("Mapping error. Id missing");

            return new AppVersion(
                domainEntity.Id.Value,
                domainEntity.AndroidVersion,
                domainEntity.IOSVersion,
                domainEntity.CreatedByUserId.Value,
                domainEntity.UpdatedByUserId.Value
                )
            {
                CreatedAt = domainEntity.CreatedAt,
            };
        }
        public static List<AppVersion> FromDomains(List<Domain.Version.AppVersion> domainEntities)
        {
            if (domainEntities == null || domainEntities.Count == 0)
            {
                // Handle the case where there are no messages if needed.
                return new List<AppVersion>();
            }
            return domainEntities.Select(newEntity => new AppVersion(
                newEntity.Id,
                newEntity.AndroidVersion,
                newEntity.IOSVersion,
                newEntity.CreatedByUserId,
                newEntity.UpdatedByUserId)
            {
                CreatedAt = newEntity.CreatedAt
            })
                .ToList();
        }


    }
}
