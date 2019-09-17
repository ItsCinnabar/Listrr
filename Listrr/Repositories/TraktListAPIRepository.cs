﻿using Listrr.Comparer;
using Listrr.Data.Trakt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TraktNet;
using TraktNet.Enums;
using TraktNet.Objects.Authentication;
using TraktNet.Objects.Get.Movies;
using TraktNet.Objects.Get.Shows;
using TraktNet.Objects.Post.Users.CustomListItems;
using TraktNet.Requests.Parameters;
using TraktNet.Utils;

namespace Listrr.Repositories
{
    public class TraktListAPIRepository : ITraktListAPIRepository
    {

        private readonly TraktClient _traktClient;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        private readonly uint? fetchLimit = 100;

        public TraktListAPIRepository(UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            _traktClient = new TraktClient(configuration["Trakt:ClientID"], configuration["Trakt:ClientSecret"]);
        }


        public async Task<TraktList> Create(TraktList model)
        {
            await PrepareForApiRequest(model.Owner);

            var response = await _traktClient.Users.CreateCustomListAsync(
                "me",
                model.Name,
                Constants.LIST_Description,
                TraktAccessScope.Public,
                false,
                false
            );

            if (!response.IsSuccess) throw response.Exception;

            model.Id = response.Value.Ids.Trakt;
            model.Slug = response.Value.Ids.Slug;
            model.LastProcessed = DateTime.Now;
            model.Process = true;

            return model;
        }

        public async Task Delete(TraktList model)
        {
            await PrepareForApiRequest(model.Owner);

            await _traktClient.Users.DeleteCustomListAsync(model.Owner.UserName, model.Slug);
        }

        public async Task<TraktList> Get(uint id)
        {
            await PrepareForApiRequest();

            var response = await _traktClient.Users.GetCustomListAsync("me", id.ToString());

            if (!response.IsSuccess) throw response.Exception;

            return new TraktList()
            {
                Id = response.Value.Ids.Trakt,
                Slug = response.Value.Ids.Slug,
                Name = response.Value.Name
            };
        }

        public async Task<IList<ITraktMovie>> MovieSearch(TraktList model)
        {
            var list = new List<ITraktMovie>();

            await MovieSearch(model, list);

            return list;
        }

        private async Task MovieSearch(TraktList model, IList<ITraktMovie> list)
        {
            uint? page = 0;

            while (true)
            {
                var result = await _traktClient.Search.GetTextQueryResultsAsync(
                    TraktSearchResultType.Movie,
                    model.Query,
                    model.Filter_SearchField,
                    new TraktSearchFilter(
                        model.Filter_Years.From,
                        model.Filter_Years.To,
                        model.Filter_Genres.Genres,
                        model.Filter_Languages.Languages,
                        model.Filter_Countries.Languages,
                        new Range<int>(
                            model.Filter_Runtimes.From,
                            model.Filter_Runtimes.To
                        ),
                        new Range<int>(
                            model.Filter_Ratings.From,
                            model.Filter_Ratings.To
                        )
                    ), new TraktExtendedInfo().SetMetadata(),
                    new TraktPagedParameters(page, fetchLimit)
                );

                if (!result.IsSuccess) throw result.Exception;

                foreach (var traktSearchResult in result.Value)
                {
                    if (!list.Contains(traktSearchResult.Movie, new TraktMovieComparer()))
                        list.Add(traktSearchResult.Movie);
                }

                if (result.PageCount == page) break;

                page++;
            }
        }

        public async Task<TraktList> Update(TraktList model)
        {
            await PrepareForApiRequest();

            await _traktClient.Users.UpdateCustomListAsync(
                "me",
                model.Id.ToString(),
                model.Name,
                Constants.LIST_Description,
                TraktAccessScope.Public,
                false,
                false
            );

            return model;
        }

        
        public async Task<IList<ITraktMovie>> GetMovies(TraktList model)
        {
            await PrepareForApiRequest(model.Owner);

            var result = new List<ITraktMovie>();

            await GetMovies(model, null, null, result);

            return result;
        }

        private async Task GetMovies(TraktList model, uint? page, uint? limit, IList<ITraktMovie> list)
        {
            var result = await _traktClient.Users.GetCustomListItemsAsync(
                model.Owner.UserName,
                model.Slug,
                TraktListItemType.Movie,
                new TraktExtendedInfo().SetMetadata(),
                new TraktPagedParameters(
                    page,
                    limit
                )
            );

            if (!result.IsSuccess) throw result.Exception;

            foreach (var traktSearchResult in result.Value)
            {
                list.Add(traktSearchResult.Movie);
            }

            if (result.PageCount > page)
            {
                await Task.Delay(500);
                await GetMovies(model, page + 1, limit, list);
            }

        }


        public async Task AddMovies(IList<ITraktMovie> movies, TraktList list)
        {
            await PrepareForApiRequest(list.Owner);
            
            var result = await _traktClient.Users.AddCustomListItemsAsync(
                list.Owner.UserName,
                list.Slug,
                TraktUserCustomListItemsPost.Builder().AddMovies(movies).Build()
            );

            if (!result.IsSuccess) throw result.Exception;
        }

        public async Task RemoveMovies(IEnumerable<ITraktMovie> movies, TraktList list)
        {
            await PrepareForApiRequest(list.Owner);

            var result = await _traktClient.Users.RemoveCustomListItemsAsync(
                list.Owner.UserName,
                list.Slug,
                TraktUserCustomListItemsPost.Builder().AddMovies(movies).Build()
            );

            if (!result.IsSuccess) throw result.Exception;
        }


        

        public async Task<IList<ITraktShow>> GetShows(TraktList model)
        {
            await PrepareForApiRequest(model.Owner);

            var result = new List<ITraktShow>();

            await GetShows(model, null, null, result);

            return result;
        }

        private async Task GetShows(TraktList model, uint? page, uint? limit, IList<ITraktShow> list)
        {
            var result = await _traktClient.Users.GetCustomListItemsAsync(
                model.Owner.UserName,
                model.Slug,
                TraktListItemType.Show,
                new TraktExtendedInfo().SetMetadata(),
                new TraktPagedParameters(
                    page,
                    limit
                )
            );

            if (!result.IsSuccess) throw result.Exception;

            foreach (var traktSearchResult in result.Value)
            {
                list.Add(traktSearchResult.Show);
            }

            if (result.PageCount > page)
            {
                await Task.Delay(500);
                await GetShows(model, page + 1, limit, list);
            }

        }


        public async Task AddShows(IList<ITraktShow> shows, TraktList list)
        {
            await PrepareForApiRequest(list.Owner);

            var result = await _traktClient.Users.AddCustomListItemsAsync(
                list.Owner.UserName,
                list.Slug,
                TraktUserCustomListItemsPost.Builder().AddShows(shows).Build()
            );

            if (!result.IsSuccess) throw result.Exception;
        }

        public async Task RemoveShows(IEnumerable<ITraktShow> shows, TraktList list)
        {
            await PrepareForApiRequest(list.Owner);

            var result = await _traktClient.Users.RemoveCustomListItemsAsync(
                list.Owner.UserName,
                list.Slug,
                TraktUserCustomListItemsPost.Builder().AddShows(shows).Build()
            );

            if (!result.IsSuccess) throw result.Exception;
        }


        public async Task<IList<ITraktShow>> ShowSearch(TraktList model)
        {
            var list = new List<ITraktShow>();

            await ShowSearch(model, list);

            return list;
        }

        private async Task ShowSearch(TraktList model, IList<ITraktShow> list)
        {
            uint? page = 0;

            while (true)
            {
                var result = await _traktClient.Search.GetTextQueryResultsAsync(
                    TraktSearchResultType.Show,
                    model.Query,
                    model.Filter_SearchField,
                    new TraktSearchFilter(
                        model.Filter_Years.From,
                        model.Filter_Years.To,
                        model.Filter_Genres.Genres,
                        model.Filter_Languages.Languages,
                        model.Filter_Countries.Languages,
                        new Range<int>(
                            model.Filter_Runtimes.From,
                            model.Filter_Runtimes.To
                        ),
                        new Range<int>(
                            model.Filter_Ratings.From,
                            model.Filter_Ratings.To
                        ),
                        model.Filter_Certifications_Show.Certifications,
                        model.Filter_Networks.Networks
                    ), new TraktExtendedInfo().SetMetadata(),
                    new TraktPagedParameters(page, fetchLimit)
                );


                if (!result.IsSuccess) throw result.Exception;

                foreach (var traktSearchResult in result.Value)
                {
                    if (!list.Contains(traktSearchResult.Show, new TraktShowComparer()))
                        list.Add(traktSearchResult.Show);
                }

                if (result.PageCount == page) break;

                page++;
            }
        }



        private async Task PrepareForApiRequest(IdentityUser user = null)
        {
            if (user == null)
            {
                user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            }

            var expiresAtToken = await _userManager.GetAuthenticationTokenAsync(user, Constants.TOKEN_LoginProvider, Constants.TOKEN_ExpiresAt);
            var access_token = await _userManager.GetAuthenticationTokenAsync(user, Constants.TOKEN_LoginProvider, Constants.TOKEN_AccessToken);
            var refresh_token = await _userManager.GetAuthenticationTokenAsync(user, Constants.TOKEN_LoginProvider, Constants.TOKEN_RefreshToken);

            var expiresAt = DateTime.Parse(expiresAtToken);

            if (expiresAt < DateTime.Now)
            {
                //Refresh the token
            }

            _traktClient.Authorization = TraktAuthorization.CreateWith(access_token);
        }

        
    }
}