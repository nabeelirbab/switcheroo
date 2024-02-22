using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        [Authorize]
        public async Task<Items.Models.Item> GetItem(
            [Service] IItemRepository itemRepository,
            Guid itemId
        )
        {
            var item = await itemRepository.GetItemByItemId(itemId);

            return Items.Models.Item.FromDomain(item);
        }

        [Authorize]
        public async Task<Paginated<Items.Models.Item>> GetItems(
            [Service] IHttpContextAccessor httpContextAccessor,
            [Service] IUserAuthenticationService userAuthenticationService,
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
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);

            if (user == null) throw new ApiException("Not logged in");
            if (!user.Id.HasValue) throw new ApiException("Fatal. Db entity doesn't have a primary key...or you fucked up");

            if (amount.HasValue)
            {
                var paginatedItemsResult = await itemRepository.GetItems(user.Id.Value, itemId, amount, categories, limit, cursor, latitude, longitude, distance, inMiles);
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
                var paginatedItems = await itemRepository.GetAllItems(user.Id.Value, limit, cursor);
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

        [Authorize]
        public async Task<Paginated<Items.Models.Item>> GetAllItems(
                [Service] IHttpContextAccessor httpContextAccessor,
                [Service] IUserAuthenticationService userAuthenticationService,
                [Service] IItemRepository itemRepository,
                Guid userId,
                int limit,
                string? cursor
            )
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);

            if (user == null) throw new ApiException("Not logged in");
            if (!user.Id.HasValue) throw new ApiException("Fatal. Db entity doesn't have a primary key...or you fucked up");

            var paginatedItems = await itemRepository.GetAllItems(userId, limit, cursor);

            return new Paginated<Items.Models.Item>(
                paginatedItems.Data
                    .Select(Items.Models.Item.FromDomain)
                    .ToList(),
                paginatedItems.Cursor,
                paginatedItems.TotalCount,
                paginatedItems.HasNextPage);

        }

        [Authorize]
        public async Task<List<KeyValue>> CategoriesItemCount(
                [Service] IHttpContextAccessor httpContextAccessor,
                [Service] IUserAuthenticationService userAuthenticationService,
                [Service] IItemRepository itemRepository
            )
        {
            var claimsPrinciple = httpContextAccessor.HttpContext.User;
            var user = await userAuthenticationService.GetCurrentlySignedInUserAsync(claimsPrinciple);

            if (user == null) throw new ApiException("Not logged in");
            if (!user.Id.HasValue) throw new ApiException("Fatal. Db entity doesn't have a primary key...or you fucked up");

            var categoriesItemCount = await itemRepository.GetCategoriesItemCount();

            return categoriesItemCount;

        }
    }
}
