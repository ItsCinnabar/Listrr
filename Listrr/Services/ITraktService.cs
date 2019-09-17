﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Listrr.Data.Trakt;
using Microsoft.AspNetCore.Identity;
using TraktNet.Objects.Get.Movies;
using TraktNet.Objects.Get.Shows;

namespace Listrr.Services
{
    public interface ITraktService
    {

        Task<TraktList> Get(uint id, bool forceAPI = false);
        Task<List<TraktList>> Get(IdentityUser user);
        Task Delete(TraktList model);

        Task<IList<ITraktMovie>> GetMovies(TraktList model);
        Task<IList<ITraktShow>> GetShows(TraktList model);

        Task<IList<TraktList>> GetProcessable();


        Task AddMovies(IList<ITraktMovie> movies, TraktList list);
        Task RemoveMovies(IEnumerable<ITraktMovie> movies, TraktList list);


        Task AddShows(IList<ITraktShow> movies, TraktList list);
        Task RemoveShows(IEnumerable<ITraktShow> movies, TraktList list);


        Task<TraktList> Create(TraktList model);
        Task<TraktList> Update(TraktList model);


        Task<IList<ITraktMovie>> MovieSearch(TraktList model);
        Task<IList<ITraktShow>> ShowSearch(TraktList model);

    }
}