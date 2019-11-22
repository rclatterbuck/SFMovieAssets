using SFMovieAssets.Proxies;
using System;
using System.Collections.Generic;
using Refit;
using System.Threading.Tasks;
using SFMovieAssets.Models;

namespace SFMovieAssets.Manager
{
    public class AssetPropertyRetriever
    {
        #region Properties

        /// <summary>
        /// The proxy interface for SWAPI
        /// </summary>
        private readonly IStarWarsAPI _swapi;

        /// <summary>
        /// The list of values already returned to the console.
        /// </summary>
        private readonly IList<string> _returnedValues;

        /// <summary>
        /// A lock object for parallel processing.
        /// </summary>
        private readonly object _addValuesLock;

        /// <summary>
        /// A dictionary containing the assetType keys and the method to retrieve them.
        /// </summary>
        private readonly Dictionary<string, Action<Film, string>> _typeDictionary;

        #endregion

        #region Constructor

        /// <summary>
        /// Base Constructor for the manager
        /// </summary>
        public AssetPropertyRetriever()
        {
            _swapi = RestService.For<IStarWarsAPI>("https://swapi.co/api");
            _returnedValues = new List<string>();
            _addValuesLock = new object();

            _typeDictionary = new Dictionary<string, Action<Film, string>>
            {
                {"characters", ProcessPeople},
                {"planets", ProcessPlanets},
                {"starships", ProcessStarships}
            };
        }

        #endregion

        /// <summary>
        /// Runs the program with the arguments.
        /// </summary>
        /// <param name="args"></param>
        public void Run(string[] args)
        {
            #region Validate Film Arguments

            if (args.Length != 3)
            {
                Console.WriteLine("Please try again with a film title, assetType, and asset property.");
                return;
            }

            var filmTitle = args[0];
            var assetType = args[1];
            var propertyName = args[2];

            if (!_typeDictionary.ContainsKey(assetType))
            {
                Console.WriteLine($"The AssetType of {assetType} is not a valid AssetType on the film.");
                return;
            }

            var filmSearchResults = _swapi.FilmSearchByTitle(filmTitle).Result.results;

            if (filmSearchResults.Count != 1)
            {
                Console.WriteLine(
                    $"Either no films or multiple films matched the search string {filmTitle}. Please try again with an exact Episode Title.");
                return;
            }

            #endregion

            _typeDictionary[assetType](filmSearchResults[0], propertyName);
        }

        #region Process Assets

        /// <summary>
        /// Process the People objects on a Film
        /// </summary>
        /// <param name="film"></param>
        /// <param name="propertyName"></param>
        private void ProcessPeople(Film film, string propertyName)
        {
            // Use Reflection to cache the property type.
            var prop = typeof(People).GetProperty(propertyName);
            if (prop == null || prop.PropertyType != typeof(string))
            {
                Console.WriteLine($"The property value of {propertyName} does not exist as a string on the People Type.");
                return;
            }

            Parallel.ForEach(film.characters, assetUrl =>
            {
                // Get the People from the SWAPI
                var character = _swapi.GetPeople(DeriveIdFromUrl(assetUrl)).Result;

                if (character != null)
                {
                    ReturnValue((string)prop.GetValue(character));
                }
            });
        }

        /// <summary>
        /// Process the Planet objects on a Film
        /// </summary>
        /// <param name="film"></param>
        /// <param name="propertyName"></param>
        private void ProcessPlanets(Film film, string propertyName)
        {
            // Use Reflection to cache the property type.
            var prop = typeof(Planet).GetProperty(propertyName);
            if (prop == null || prop.PropertyType != typeof(string))
            {
                Console.WriteLine($"The property value of {propertyName} does not exist as a string on the Planet Type.");
                return;
            }

            Parallel.ForEach(film.planets, (assetUrl) =>
            {
                // Get the Planet from the SWAPI
                var id = DeriveIdFromUrl(assetUrl);
                var planet = _swapi.GetPlanets(id).Result;

                if (planet != null)
                {
                    ReturnValue((string)prop.GetValue(planet));
                }
            });
        }

        /// <summary>
        /// Process the Starship objects on a Film
        /// </summary>
        /// <param name="film"></param>
        /// <param name="propertyName"></param>
        private void ProcessStarships(Film film, string propertyName)
        {
            // Use Reflection to cache the property type.
            var prop = typeof(Starship).GetProperty(propertyName);
            if (prop == null || prop.PropertyType != typeof(string))
            {
                Console.WriteLine($"The property value of {propertyName} does not exist as a string on the Starship Type.");
                return;
            }

            Parallel.ForEach(film.starships, (assetUrl) =>
            {
                // Get the Starship from the SWAPI
                var starship = _swapi.GetStarship(DeriveIdFromUrl(assetUrl)).Result;

                if (starship != null)
                {
                    ReturnValue((string)prop.GetValue(starship));
                }
            });
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Checks if the property value has already been returned to the user.
        /// If it has not, it prints to the console, and adds to the _returnedValues list.
        /// </summary>
        /// <param name="propertyValue"></param>
        private void ReturnValue(string propertyValue)
        {
            lock (_addValuesLock)
            {
                if (!_returnedValues.Contains(propertyValue))
                {
                    _returnedValues.Add(propertyValue);
                    Console.WriteLine(propertyValue);
                }
            }
        }
        
        /// <summary>
        /// The proxy already has the correct URL for asset return.
        /// This method returns the value of the id parameter needed for a particular object.
        /// </summary>
        /// <param name="url">The URL returned by the SWAPI JSON object.</param>
        /// <returns>The string id of the object from the URL.</returns>
        private string DeriveIdFromUrl(string url)
        {
            // It appears that the URLs returned within asset JSON by SWAPI are of the form "https://baseurl/api/assettype/{id}/"
            // As such, we need to first strip off the last "/"
            // Then we can strip off everything before the penultimate "/"
            var cleanedUrl = url.Remove(url.Length - 1);
            
            return cleanedUrl.Substring(cleanedUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);
        }

        #endregion
    }
}
