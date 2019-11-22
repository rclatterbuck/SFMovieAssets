using System.Threading.Tasks;
using Refit;
using SFMovieAssets.Models;

namespace SFMovieAssets.Proxies
{
    public interface IStarWarsAPI
    {
        /// <summary>
        /// A search for films by episode title
        /// </summary>
        /// <param name="title">The text we are searching on</param>
        /// <returns>A FilmSearchResult with the list of films matching the episode title search term.</returns>
        [Get("/films/?search={title}")]
        Task<FilmSearchResult> FilmSearchByTitle(string title);

        /// <summary>
        /// Gets a People object by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The specified People object</returns>
        [Get("/people/{id}")]
        Task<People> GetPeople(string id);

        /// <summary>
        /// Gets a Planet object by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The specified Planet object</returns>
        [Get("/planets/{id}")]
        Task<Planet> GetPlanets(string id);

        /// <summary>
        /// Gets a Starship object by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The specified Starship object</returns>
        [Get("/starships/{id}")]
        Task<Starship> GetStarship(string id);
    }
}
