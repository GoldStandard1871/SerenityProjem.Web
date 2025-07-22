using Serenity;
using Serenity.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MovieTutorial.MovieDB;

public partial class MovieCastEditorAttribute : CustomEditorAttribute
{
    public const string Key = "MovieTutorial.MovieDB.MovieCastEditor";

    public MovieCastEditorAttribute()
        : base(Key)
    {
    }
}