using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Infrastructure.Database.Schema
{
    public class AppVersion : Audit
    {
        public AppVersion(string androidVersion, string iosVersion)
        {
            AndroidVersion = androidVersion;
            IOSVersion = iosVersion;
        }
        public AppVersion() { }
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string AndroidVersion { get; set; }
        [Required]
        public string IOSVersion { get; set; }

        public void FromDomain(Domain.Version.AppVersion domainEntity)
        {
            AndroidVersion = domainEntity.AndroidVersion;
            IOSVersion = domainEntity.IOSVersion;
        }

        public static Expression<Func<AppVersion, Domain.Version.AppVersion>> ToDomain =>
            entity => new Domain.Version.AppVersion(
                entity.Id,
                entity.AndroidVersion,
                entity.AndroidVersion,
                entity.CreatedByUserId,
                entity.UpdatedByUserId
            )
            {
                CreatedAt = entity.CreatedAt.DateTime
            };

    }
}
