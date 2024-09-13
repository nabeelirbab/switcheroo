using Domain.Complaints;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Version
{
    public interface IAppVersionRepository
    {
        Task<AppVersion> CreateAsync(AppVersion version, Guid userId);
        Task<AppVersion> GetById(Guid notificationId);
        Task<List<AppVersion>> GetAll();
        Task<AppVersion> GetLatest();
    }
}
