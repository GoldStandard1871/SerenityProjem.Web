using Serenity.Navigation;
using MyPages = SerenityProjem.Default.Pages;

[assembly: NavigationMenu(6000, "Movie Database", icon: "fa-film")]
[assembly: NavigationLink(6100, "Movie Database/Movies",
    typeof(MyPages.MoviePage), icon: "fa-video-camera")]

[assembly: NavigationLink(6200, "Movie Database/Genres", typeof(MyPages.GenrePage), icon: "fa-thumb-tack")]

[assembly: NavigationLink(6300, "Movie Database/Person", typeof(MyPages.PersonPage), icon: "fa-users")]
[assembly: NavigationLink(6400, "Movie Database/Movie Cast", typeof(MyPages.MovieCastPage), icon: "fa-users")]
