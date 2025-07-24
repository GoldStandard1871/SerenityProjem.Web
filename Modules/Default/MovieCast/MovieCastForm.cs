using Serenity.ComponentModel;
using System.ComponentModel;

namespace SerenityProjem.Default.Forms;

[FormScript("Default.MovieCast")]
[BasedOnRow(typeof(MovieCastRow), CheckNames = true)]
public class MovieCastForm
{
    [DisplayName("Movie"), Required]
    public int MovieId { get; set; }
    
    [DisplayName("Actor/Actress"), Required]
    public int PersonId { get; set; }
    
    [DisplayName("Character"), Required, MaxLength(50)]
    public string Character { get; set; }
}