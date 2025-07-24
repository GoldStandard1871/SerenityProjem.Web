using Serenity.ComponentModel;
using SerenityProjem.Default.Pages;
using System;
using System.Collections;
using System.Collections.Generic;


namespace SerenityProjem.Default.Forms;

[FormScript("Default.Movie")]
[BasedOnRow(typeof(MovieRow), CheckNames = true)]
public class MovieForm
{

    public string Title { get; set; }
    [TextAreaEditor(Rows = 3)]
    public string Description { get; set; }
    [DisplayName("Cast"), MovieCastEditor, IgnoreName]
    public List<MovieCastRow> CastList { get; set; }
    [TextAreaEditor(Rows = 8)]
    public string Storyline { get; set; }
    public int Year { get; set; }
    public DateTime ReleaseDate { get; set; }
    public int Runtime { get; set; }
   
    public MovieKind Kind { get; set; }
    public List<int> GenreList { get; set; }
    public string PrimaryImage { get; set; }
    public string GalleryImages { get; set; }


}