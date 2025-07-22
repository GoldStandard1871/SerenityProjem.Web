using Serenity.ComponentModel;
using SerenityProjem.Default.Pages;
using System;
using System.ComponentModel;

namespace SerenityProjem.Default.Columns;

[ColumnsScript("Default.Movie")]
[BasedOnRow(typeof(MovieRow), CheckNames = true)]
public class MovieColumns
{
    [EditLink, DisplayName("Db.Shared.RecordId"), AlignRight]
    public int MovieId { get; set; }
    [EditLink]
    public string Title { get; set; }
    public string Description { get; set; }
    public string Storyline { get; set; }
    public int Year { get; set; }
    public DateTime ReleaseDate { get; set; }
    [DisplayName("Runtime in Minutes"), Width(150), AlignRight]
    public int Runtime { get; set; }
   
    [QuickFilter]
    public MovieKind Kind { get; set; }
    [Width(200),GenreListFormatter, QuickFilter ]
    public List<int> GenreList { get; set; }
}