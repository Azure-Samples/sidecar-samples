
using devShopDNC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace devShopDNC.ViewComponents
{
    [ViewComponent(Name = "Category")]
    public class CategoryViewComponent:ViewComponent
    {
      //  private readonly devShopDNCContext _context;
        Models.ProductsDB ProductsDB = new Models.ProductsDB();
       
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Category> categories = new List<Category>();
            categories = ProductsDB.GetProductCategories().ToList();
            return View("Index", categories);
        }

    }
}
