using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.GraphQL.Categories.Model;
using API.GraphQL.CommonServices;
using Domain;
using Domain.Items;
using Domain.Services;
using Domain.Users;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Infrastructure.Database;
using Microsoft.AspNetCore.Http;

namespace API.GraphQL
{

    public partial class Query
    {
        private readonly ILoggerManager _loggerManager;

        public Query(ILoggerManager loggerManager)
        {
            _loggerManager = loggerManager;
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Items.Models.Item> GetItem(
            [Service] IItemRepository itemRepository,
            Guid itemId
        )
        {
            var item = await itemRepository.GetItemByItemId(itemId);
            return Items.Models.Item.FromDomain(item);
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Paginated<Items.Models.Item>> GetItems(
            [Service] UserContextService userContextService,
            [Service] IItemRepository itemRepository,
            Guid itemId,
            decimal? amount,
            string[]? categories,
            int limit,
            string? cursor,
            decimal? latitude,
            decimal? longitude,
            decimal? distance,
            bool? inMiles = false
        )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            if (amount.HasValue)
            {
                var paginatedItemsResult = await itemRepository.GetItems(requestUserId, itemId, amount, categories, limit, cursor, latitude, longitude, distance, inMiles);
                _loggerManager.LogWarn($"API return Items Result to frontend: {paginatedItemsResult.Data.Count}");
                return new Paginated<Items.Models.Item>(
                    paginatedItemsResult.Data
                        .Select(Items.Models.Item.FromDomain)
                        .ToList(),
                    paginatedItemsResult.Cursor,
                    paginatedItemsResult.TotalCount,
                    paginatedItemsResult.HasNextPage);
            }
            else
            {
                var paginatedItems = await itemRepository.GetAllItems(requestUserId, limit, cursor);
                _loggerManager.LogError($"paginatedItems: {paginatedItems.Data.Count}");
                return new Paginated<Items.Models.Item>(
                    paginatedItems.Data
                        .Select(Items.Models.Item.FromDomain)
                        .ToList(),
                    paginatedItems.Cursor,
                    paginatedItems.TotalCount,
                    paginatedItems.HasNextPage);
            }
        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Paginated<Items.Models.Item>> GetAllItems(
                [Service] UserContextService userContextService,
                [Service] IItemRepository itemRepository,
                Guid userId,
                int limit,
                string? cursor
            )
        {
            var paginatedItems = await itemRepository.GetAllItems(userId, limit, cursor);
            return new Paginated<Items.Models.Item>(
                paginatedItems.Data
                    .Select(Items.Models.Item.FromDomain)
                    .ToList(),
                paginatedItems.Cursor,
                paginatedItems.TotalCount,
                paginatedItems.HasNextPage);

        }

        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<List<KeyValue>> CategoriesItemCount(
                [Service] IItemRepository itemRepository
            )
        {
            var categoriesItemCount = await itemRepository.GetCategoriesItemCount();
            return categoriesItemCount;

        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin", "User" })]
        public async Task<Paginated<Items.Models.Item>> GetCashItems(
               [Service] UserContextService userContextService,
               [Service] IItemRepository itemRepository,
               int limit,
               string? cursor,
               decimal? latitude,
               decimal? longitude,
               decimal? distance,
               bool? inMiles = false
           )
        {
            var requestUserId = userContextService.GetCurrentUserId();
            var paginatedItemsResult = await itemRepository.GetCashItems(requestUserId, limit, cursor, latitude, longitude, distance, inMiles);
            _loggerManager.LogWarn($"API return cashItems Result to frontend: {paginatedItemsResult.Data.Count}");
            return new Paginated<Items.Models.Item>(
                paginatedItemsResult.Data
                    .Select(Items.Models.Item.FromDomain)
                    .ToList(),
                paginatedItemsResult.Cursor,
                paginatedItemsResult.TotalCount,
                paginatedItemsResult.HasNextPage);

        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]

        public async Task<int> GetItemsCount(
                [Service] IItemRepository itemRepository
            )
        {
            return await itemRepository.GetItemCount();
        }
        [HotChocolate.AspNetCore.Authorization.Authorize(Roles = new string[] { "SuperAdmin", "Admin" })]
        public async Task<Paginated<Items.Models.Item>> GetAllItemsInDatabase(
                [Service] IItemRepository itemRepository,
                int limit,
                string? cursor
            )
        {
            var paginatedItems = await itemRepository.GetAllItems(limit, cursor);

            return new Paginated<Items.Models.Item>(
                paginatedItems.Data
                    .Select(Items.Models.Item.FromDomain)
                    .ToList(),
                paginatedItems.Cursor,
                paginatedItems.TotalCount,
                paginatedItems.HasNextPage);

        }
    }
}
