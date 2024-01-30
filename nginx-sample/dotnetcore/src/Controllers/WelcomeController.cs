using Microsoft.AspNetCore.Mvc;

namespace Dotnet8WelcomeApp.Controllers
{
    public class WelcomeController : Controller
    {

        [HttpGet]
        [Route("/")]
        public string Welcome()
        {
            Console.WriteLine("Welcome endpoint called!");
            return "Welcome from .NET 8 App";
        }
    }
}
