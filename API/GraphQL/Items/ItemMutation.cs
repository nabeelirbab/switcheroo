using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.GraphQL.Items.Models;
using Domain.Items;
using Domain.Users;
using HotChocolate;
using Microsoft.AspNetCore.Http;

namespace API.GraphQL
{
    public partial class Mutation
    {
        public async Task<Items.Models.Item> CreateItem(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemRepository itemRepository,
            ItemInput item
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            var newDomainItem = await itemRepository.CreateItemAsync(Domain.Items.Item.CreateNewItem(
                item.Title,
                item.Description,
                item.AskingPrice,
                item.IsSwapOnly,
                item.Categories,
                item.ImageUrls,
                user.Id.Value,
                item.Latitude,
                item.Longitude
            ));

            return Items.Models.Item.FromDomain(newDomainItem);
        }

        public async Task<Items.Models.Item> UpdateItem(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemRepository itemRepository,
            Guid id,
            ItemInput item
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            var updatedDomainItem = await itemRepository.UpdateItemAsync(Domain.Items.Item.CreateUpdateItem(
                id,
                item.Title,
                item.Description,
                item.AskingPrice,
                item.IsSwapOnly,
                item.Categories,
                item.ImageUrls,
                user.Id.Value,
                item.Latitude,
                item.Longitude,
                item.MainImageUrl
            ));

            return Items.Models.Item.FromDomain(updatedDomainItem);
        }

        public async Task<string> UpdateItemLocation(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemRepository itemRepository,
            Guid itemId,
            decimal? latitude,
            decimal? longitude
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");

            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");
            var updateMessage = await itemRepository.UpdateItemLocation(itemId, latitude, longitude);
            
            if (updateMessage.Contains("Item locations updated successfully."))
            {
                return "Items' locations updated successfully.";
            }
            else
            {
                return "Item locations not updated";
            }
        }

        public async Task<string> UpdateAllItemsLocation(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemRepository itemRepository,
            Guid userId,
            decimal? latitude,
            decimal? longitude
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");

            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            var updateMessage = await itemRepository.UpdateAllItemsLocation(userId, latitude, longitude);
            if (updateMessage.Contains("Item locations updated successfully."))
            {
                return "Items' locations updated successfully.";
            }
            else
            {
                return "Item locations not updated";
            }
        }

        public async Task<bool> ArchiveItem(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemRepository itemRepository,
            Guid itemId
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            return await itemRepository.ArchiveItemAsync(itemId, user.Id.Value);
        }

        public async Task<bool> DeleteItem(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemRepository itemRepository,
            Guid itemId
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            return await itemRepository.DeleteItemAsync(itemId);
        }


        public async Task<bool> DismissItem(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
            [Service] IItemRepository itemRepository,
            Guid? sourceItemId,
            Guid targetItemId
        )
        {
            var userCp = httpContextAccessor?.HttpContext?.User;

            if (userCp == null) throw new ApiException("Not authenticated");
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(userCp);
            if (!user.Id.HasValue) throw new ApiException("Database failure");

            var dismissItem = sourceItemId.HasValue
                ? DismissedItem.CreateDismissItemForItem(sourceItemId.Value, targetItemId, user.Id.Value)
                : DismissedItem.CreateDismissItem(targetItemId, user.Id.Value);

            return await itemRepository.DismissItemAsync(dismissItem);
        }
    }
}
