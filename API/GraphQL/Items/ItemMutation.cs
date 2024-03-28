using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.GraphQL.CommonServices;
using API.GraphQL.Items.Models;
using Domain.Items;
using Domain.Users;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;

namespace API.GraphQL
{
    public partial class Mutation
    {
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Items.Models.Item> CreateItem(
            [Service] UserContextService userContextService,
            [Service] IItemRepository itemRepository,
            ItemInput item
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            if (item.Categories == null || !item.Categories.Any())
                throw new Exception("Item Category is Required");
            var newDomainItem = await itemRepository.CreateItemAsync(Domain.Items.Item.CreateNewItem(
                item.Title,
                item.Description,
                item.AskingPrice,
                item.IsSwapOnly,
                item.Categories,
                item.ImageUrls,
                item.MainImageUrl,
                requestUserId,
                item.Latitude,
                item.Longitude
            ));
            return Items.Models.Item.FromDomain(newDomainItem);
        }


        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Items.Models.Item> UpdateItem(
            [Service] UserContextService userContextService,
            [Service] IItemRepository itemRepository,
            Guid id,
            ItemInput item
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();

            var updatedDomainItem = await itemRepository.UpdateItemAsync(Domain.Items.Item.CreateUpdateItem(
                id,
                item.Title,
                item.Description,
                item.AskingPrice,
                item.IsSwapOnly,
                item.Categories,
                item.ImageUrls,
                requestUserId,
                item.Latitude,
                item.Longitude,
                item.MainImageUrl
            ));
            return Items.Models.Item.FromDomain(updatedDomainItem);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<string> UpdateItemLocation(
            [Service] IItemRepository itemRepository,
            Guid itemId,
            decimal? latitude,
            decimal? longitude
        )
        {
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
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<string> UpdateAllItemsLocation(
            [Service] IItemRepository itemRepository,
            Guid userId,
            decimal? latitude,
            decimal? longitude
        )
        {
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
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> ArchiveItem(
            [Service] UserContextService userContextService,
            [Service] IItemRepository itemRepository,
            Guid itemId
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            return await itemRepository.ArchiveItemAsync(itemId, requestUserId);
        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> DeleteItem(
            [Service] IItemRepository itemRepository,
            Guid itemId
        )
        {
            return await itemRepository.DeleteItemAsync(itemId);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<bool> DismissItem(
            [Service] UserContextService userContextService,
            [Service] IItemRepository itemRepository,
            Guid? sourceItemId,
            Guid targetItemId
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            var dismissItem = sourceItemId.HasValue
                ? DismissedItem.CreateDismissItemForItem(sourceItemId.Value, targetItemId, requestUserId)
                : DismissedItem.CreateDismissItemForItem(targetItemId, targetItemId, requestUserId);
            return await itemRepository.DismissItemAsync(dismissItem);
        }
    }
}
