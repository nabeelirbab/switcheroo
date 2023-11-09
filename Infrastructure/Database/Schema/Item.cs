using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.Database.Schema
{
    public class Item : Audit
    {
        public Item(string title, string description, decimal askingPrice, bool isHidden, bool isSwapOnly, decimal? latitude, decimal? longitude)
        {
            Title = title;
            Description = description;
            AskingPrice = askingPrice;
            IsHidden = isHidden;
            IsSwapOnly = isSwapOnly;
            Latitude = latitude != null ? (decimal)latitude : null;
            Longitude = longitude != null ? (decimal)longitude : null; ;
        }

        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal AskingPrice { get; set; }
        [Required]
        public decimal? Latitude { get; set; }
        [Required]
        public decimal? Longitude { get; set; }

        public int? FlexibilityRange { get; set; }
        public bool IsFlexible => FlexibilityRange.HasValue;

        [Required]
        public bool IsHidden { get; set; }

        [Required]
        public bool IsSwapOnly { get; set; }

        public List<ItemCategory> ItemCategories { get; set; } = new List<ItemCategory>();

        public List<ItemImage> ItemImages { get; set; } = new List<ItemImage>();
        public List<Location> Locations { get; set; } = new List<Location>();

        public string MainImageUrl { get; set; }

        public void FromDomain(Domain.Items.Item domainItem)
        {
            Title = domainItem.Title;
            Description = domainItem.Description;
            AskingPrice = domainItem.AskingPrice;
            FlexibilityRange = domainItem.FlexibilityRange;
            IsHidden = domainItem.IsHidden;
            IsSwapOnly = domainItem.IsSwapOnly;
            Latitude = domainItem.Latitude;
            Longitude = domainItem.Longitude;
        }

        public static Expression<Func<Item, Domain.Items.Item>> ToDomain =>
            item => new Domain.Items.Item(
                item.Id,
                item.Title,
                item.Description,
                item.AskingPrice,
                item.FlexibilityRange,
                item.IsHidden,
                item.IsSwapOnly,
                item.ItemCategories.Select(ic => ic.Category.Name).ToList(),
                item.ItemImages.Select(ii => ii.Url).ToList(),
                item.CreatedByUserId,
                item.UpdatedByUserId,
                item.Latitude != null ? item.Latitude : null,
                item.Longitude != null ? item.Longitude : null,
                item.MainImageUrl
            );

    }
}
