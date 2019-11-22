﻿using System.Collections.Generic;

namespace SFMovieAssets.Models
{
    public class Film
    {
        public string title { get; set; }
        public List<string> characters { get; set; }
        public List<string> starships { get; set; }
        public List<string> planets { get; set; }
    }
}
