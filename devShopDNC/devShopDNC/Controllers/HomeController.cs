using devShopDNC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;


namespace devShopDNC.Controllers
{
    public class HomeController : Controller
    {
        
      
        ProductsDB productsDB=new ProductsDB();

       
        public IActionResult Index()
        {
            List<ProductDetails> popularProds = productsDB.GetMostPopularProductsOfWeek().ToList();
            return View(popularProds);
        }
    }
}
