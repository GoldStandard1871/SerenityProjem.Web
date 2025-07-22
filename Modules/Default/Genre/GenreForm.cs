using Serenity.ComponentModel;

namespace SerenityProjem.Default.Forms;

[FormScript("Default.Genre")]
[BasedOnRow(typeof(GenreRow), CheckNames = true)]
public class GenreForm
{
    public string Name { get; set; }
}