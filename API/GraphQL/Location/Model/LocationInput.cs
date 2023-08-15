using System;
using System.Collections.Generic;

namespace API.GraphQL.Location.Model
{
    public class LocationInput
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public Guid ItemId { get; set; }
        public bool IsActive { get; set; }
    }
}
