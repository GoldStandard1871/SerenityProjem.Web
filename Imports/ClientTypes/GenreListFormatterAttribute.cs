using Serenity;
using Serenity.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SerenityProjem;

public partial class GenreListFormatterAttribute : CustomFormatterAttribute
{
    public const string Key = "SerenityProjem.GenreListFormatter";

    public GenreListFormatterAttribute()
        : base(Key)
    {
    }
}