using Serenity;
using Serenity.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SerenityProjem.Default;

public partial class MovieCastEditorAttribute : CustomEditorAttribute
{
    public const string Key = "SerenityProjem.Default.MovieCastEditor";

    public MovieCastEditorAttribute()
        : base(Key)
    {
    }
}