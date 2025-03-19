
using devShopDNC.Models;
using Microsoft.AspNetCore.Mvc;

namespace devShopDNC.Controllers
{
    public class ProductListController : Controller
    {
        ProductsDB productsDB = new ProductsDB();

        public IActionResult ProductList(int CategoryId)
        {
           
            List<ProductDetails> prodByCategory = productsDB.GetProducts(CategoryId).ToList();
            return View(prodByCategory);
        }
    }
}
