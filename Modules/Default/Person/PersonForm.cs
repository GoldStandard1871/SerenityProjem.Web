using Serenity.ComponentModel;
using System;

namespace SerenityProjem.Default.Forms;

[FormScript("Default.Person")]
[BasedOnRow(typeof(PersonRow), CheckNames = true)]
public class PersonForm
{
    [Tab("Person")]
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public Gender Gender { get; set; }
    
    public int Height { get; set; }

    [Tab("Movies"), IgnoreName]
    public string MoviesGrid { get; set; }
    public string PrimaryImage { get; set; }
    public string GalleryImages { get; set; }
}