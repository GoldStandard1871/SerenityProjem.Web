﻿using Serenity.ComponentModel;
using System.ComponentModel;

namespace SerenityProjem.Default.Columns;

[ColumnsScript("Default.Genre")]
[BasedOnRow(typeof(GenreRow), CheckNames = true)]
public class GenreColumns
{
    [EditLink, DisplayName("Db.Shared.RecordId"), AlignRight]
    public int GenreId { get; set; }
    [EditLink]
    public string Name { get; set; }
}