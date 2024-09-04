using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Items
{
    public class Item
    {
        public Item(Guid? id, string title, string description,
            decimal askingPrice, int? flexibilityRange, bool isHidden,
            bool isSwapOnly, List<string> categories, List<string> imageUrls,
            Guid? createdByUserId, Guid? updatedByUserId, decimal? latitude, decimal? longitude, string? mainImageUrl)
        {
            Id = id;
            Title = title;
            Description = description;
            AskingPrice = askingPrice;
            FlexibilityRange = flexibilityRange;
            IsHidden = isHidden;
            IsSwapOnly = isSwapOnly;
            Categories = new List<string>(categories);
            ImageUrls = new List<string>(imageUrls);
            CreatedByUserId = createdByUserId;
            UpdatedByUserId = updatedByUserId;
            Latitude = latitude;
            Longitude = longitude;
            MainImageUrl = mainImageUrl;
        }

        [Required]
        public Guid? Id { get; private set; }

        [Required]
        public string Title { get; private set; }

        [Required]
        public string Description { get; private set; }

        [Required]
        public decimal AskingPrice { get; private set; }
        public int? FlexibilityRange { get; private set; }
        public bool IsFlexible => FlexibilityRange.HasValue;

        [Required]
        public bool IsHidden { get; private set; }

        [Required]
        public bool IsSwapOnly { get; private set; }
        [Required]
        public decimal? Latitude { get; private set; }
        [Required]
        public decimal? Longitude { get; private set; }

        public List<string> Categories { get; private set; }

        public List<string> ImageUrls { get; set; }

        public string? MainImageUrl { get; set; }

        public Guid? CreatedByUserId { get; private set; }

        public Guid? UpdatedByUserId { get; private set; }

        [NotMapped]
        public bool? HasMatchingOffer { get; set; }

        [NotMapped]
        public bool? HasCashOffer { get; set; }

        [NotMapped]
        public int? CashOfferValue { get; set; }

        //[NotMapped]
        public bool IsDeleted { get; set; }
        //[NotMapped]
        public DateTimeOffset? DeletedAt { get; set; }
        //[NotMapped]
        public Guid? DeletedByUserId { get; set; }
        public static Item CreateNewItem(
            string title,
            string description,
            decimal askingPrice,
            bool isSwapOnly,
            List<string> categories,
            List<string> imageUrls,
            string? mainImageUrl,
            Guid createdByUserId,
            decimal? latitude,
            decimal? longitude
        )
        {
            return new Item(
                null,
                title,
                description,
                askingPrice,
                null,
                false,
                isSwapOnly,
                categories,
                imageUrls,
                createdByUserId,
                createdByUserId,
                latitude,
                longitude,
                mainImageUrl
            );
        }

        public static Item CreateUpdateItem(
            Guid id,
            string title,
            string description,
            decimal askingPrice,
            bool isSwapOnly,
            List<string> categories,
            List<string> imageUrls,
            Guid updatedByUserId,
            decimal? latitude,
            decimal? longitude,
            string mainImageUrl
        )
        {
            return new Item(
                id,
                title,
                description,
                askingPrice,
                null,
                false,
                isSwapOnly,
                categories,
                imageUrls,
                null,
                updatedByUserId,
                latitude,
                longitude,
                mainImageUrl
            );
        }
    }
}
