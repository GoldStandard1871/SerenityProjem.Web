using FluentMigrator;
using System;

namespace Serene1.Migrations.DefaultDB;
[DefaultDB, MigrationKey(20250718_0003)]
public class DefaultDB_20250718_0003_MovieSample : AutoReversingMigration
{
    public override void Up()
    {
        Insert.IntoTable("Movie")
       .Row(new
       {
           Title = "The Matrix",
           Description = "A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.",
           Storyline = "Thomas A. Anderson is a man living two lives. By day he is an average computer programmer and by night a hacker known as Neo.",
           Year = 1999,
           ReleaseDate = new DateTime(1999, 03, 31),
           Runtime = 136
       })
       .Row(new
       {
           Title = "The Lord of the Rings: The Fellowship of the Ring",
           Description = "A meek hobbit of the Shire and eight companions set out on a journey to Mount Doom to destroy the One Ring and the dark lord Sauron.",
           Storyline = "An ancient Ring thought lost for centuries has been found, and through a strange twist in fate has been given to a small Hobbit named Frodo.",
           Year = 2001,
           ReleaseDate = new DateTime(2001, 12, 19),
           Runtime = 178
       })
       .Row(new
       {
           Title = "The Shawshank Redemption",
           Description = "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.",
           Storyline = "Andy Dufresne is a young and successful banker whose life changes drastically when he is convicted and sentenced to life imprisonment for the murder of his wife and her lover.",
           Year = 1994,
           ReleaseDate = new DateTime(1994, 10, 14),
           Runtime = 142
       })
       .Row(new
       {
           Title = "Inception",
           Description = "A thief who steals corporate secrets through use of dream-sharing technology is given the inverse task of planting an idea into the mind of a CEO.",
           Storyline = "Dom Cobb is a skilled thief, the absolute best in the dangerous art of extraction: stealing valuable secrets from deep within the subconscious.",
           Year = 2010,
           ReleaseDate = new DateTime(2010, 07, 16),
           Runtime = 148
       })
       .Row(new
       {
           Title = "Fight Club",
           Description = "An insomniac office worker and a devil-may-care soap maker form an underground fight club that evolves into something much more.",
           Storyline = "A ticking-time-bomb insomniac and a slippery soap salesman channel primal male aggression into a shocking new form of therapy.",
           Year = 1999,
           ReleaseDate = new DateTime(1999, 10, 15),
           Runtime = 139
       })
       .Row(new
       {
           Title = "Interstellar",
           Description = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.",
           Storyline = "When Earth becomes uninhabitable, a former NASA pilot leads a mission through a wormhole to find a new home for mankind.",
           Year = 2014,
           ReleaseDate = new DateTime(2014, 11, 07),
           Runtime = 169
       });

    }
}