using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace devShopDNC.Models {

    
    //*******************************************************
    //
    // ProductsDB Class
    //
    // Business/Data Logic Class that encapsulates all data
    // logic necessary to query products within
    // the devShop Products database.
    //
    //*******************************************************

    public class ProductsDB {
        
       

        string connString=Startup.ConnectionString;

        //*******************************************************
        //
        // ProductsDB.GetProductCategories() Method <a name="GetProductCategories"></a>
        //
        // The GetProductCategories method returns a DataReader that exposes all 
        // product categories (and their CategoryIDs) within the devShop Products   
        // database.  The SQLDataReaderResult struct also returns the
        // SQL connection, which must be explicitly closed after the
        // data from the DataReader is bound into the controls.
        //
        // Other relevant sources:
        //     + <a href="ProductCategoryList.htm" style="color:green">ProductCategoryList Stored Procedure</a>
        //
        //*******************************************************
        //public ProductsDB(IConfiguration configuration) { 
        //_configuration = configuration;

        //}

        public ProductsDB() { }

        public IEnumerable<Category> GetProductCategories()
        {
            using (var myConnection = new SqliteConnection(connString))
            {
                var myCommand = new SqliteCommand("SELECT CategoryId, CategoryName FROM DevShop_Categories", myConnection);

                myConnection.Open();
                using (var result = myCommand.ExecuteReader())
                {
                    var categories = new List<Category>();
                    while (result.Read())
                    {
                        var category = new Category
                        {
                            CategoryId = Convert.ToInt32(result["CategoryId"]),
                            CategoryName = result["CategoryName"].ToString()
                        };
                        categories.Add(category);
                    }
                    return categories;
                }
            }
        }

        //*******************************************************
        //
        // ProductsDB.GetProducts() Method <a name="GetProducts"></a>
        //
        // The GetProducts method returns a struct containing a forward-only,
        // read-only DataReader. This displays all products within a specified
        // product category.  The SQLDataReaderResult struct also returns the
        // SQL connection, which must be explicitly closed after the
        // data from the DataReader is bound into the controls.
        //
        // Other relevant sources:
        //     + <a href="ProductsByCategory.htm" style="color:green">ProductsByCategory Stored Procedure</a>
        //
        //*******************************************************

        public IEnumerable<ProductDetails> GetProducts(int categoryID)
        {
            using (var myConnection = new SqliteConnection(connString))
            {
                var myCommand = new SqliteCommand("SELECT * FROM DevShop_Product_Master WHERE CategoryID = @CategoryID", myConnection);
                myCommand.Parameters.AddWithValue("@CategoryID", categoryID);

                myConnection.Open();
                using (var result = myCommand.ExecuteReader())
                {
                    var productsList = new List<ProductDetails>();
                    while (result.Read())
                    {
                        var productDetails = new ProductDetails
                        {
                            ProductId = Convert.ToInt32(result["ProductId"]),
                            ProductDescription = result["ProductDescription"].ToString(),
                            ProductName = result["ProductName"].ToString(),
                            ProductPrice = Convert.ToInt32(result["ProductPrice"]),
                            ProductImage = result["ProductImage"].ToString()
                        };
                        productsList.Add(productDetails);
                    }
                    return productsList;
                }
            }
        }


        //*******************************************************
        //
        // ProductsDB.GetProductDetails() Method <a name="GetProductDetails"></a>
        //
        // The GetProductDetails method returns a ProductDetails
        // struct containing specific details about a specified
        // product within the devShop Products Database.
        //
        // Other relevant sources:
        //     + <a href="ProductDetail.htm" style="color:green">ProductDetail Stored Procedure</a>
        //
        //*******************************************************

        public ProductDetails GetProductDetails(int productID)
        {
            using (var myConnection = new SqliteConnection(connString))
            {
                var myCommand = new SqliteCommand("SELECT * FROM DevShop_Product_Master WHERE ProductId = @ProductID", myConnection);
                myCommand.Parameters.AddWithValue("@ProductID", productID);

                myConnection.Open();
                using (var result = myCommand.ExecuteReader())
                {
                    if (result.Read())
                    {
                        return new ProductDetails
                        {
                            ProductId = Convert.ToInt32(result["ProductId"]),
                            ProductName = result["ProductName"].ToString(),
                            ProductBrand = result["ProductBrand"].ToString(),
                            ProductImage = result["ProductImage"].ToString().Trim(),
                            ProductPrice = Convert.ToInt32(result["ProductPrice"]),
                            ProductDescription = result["ProductDescription"].ToString().Trim(),
                            ProductGender = result["ProductGender"].ToString().Trim()
                        };
                    }
                    return null;
                }
            }
        }

        //*******************************************************
        //
        // ProductsDB.GetProductsAlsoPurchased() Method <a name="GetProductsAlsoPurchased"></a>
        //
        // The GetPGetProductsAlsoPurchasedroducts method returns a struct containing
        // a forward-only, read-only DataReader.  This displays a list of other products
        // also purchased with a specified product  The SQLDataReaderResult struct also
        // returns the SQL connection, which must be explicitly closed after the
        // data from the DataReader is bound into the controls.
        //
        // Other relevant sources:
        //     + <a href="CustomerAlsoBought.htm" style="color:green">CustomerAlsoBought Stored Procedure</a>
        //
        //*******************************************************

        public IEnumerable<ProductDetails> GetProductsAlsoPurchased() {

            // Create Instance of Connection and Command Object
           
           
            SqlConnection myConnection = new SqlConnection(connString);
            SqlCommand myCommand = new SqlCommand("devShop_CustomerAlsoBought", myConnection);

            // Mark the Command as a SPROC
            myCommand.CommandType = CommandType.StoredProcedure;

          
            // Execute the command
            myConnection.Open();
            SqlDataReader result = myCommand.ExecuteReader(CommandBehavior.CloseConnection);

            // Return the datareader result
            List<ProductDetails> alsoBoughtProdList = new List<ProductDetails>();

            while(result.Read())
            {
                ProductDetails alsoBoughtProd = new ProductDetails();

                alsoBoughtProd.ProductId = Convert.ToInt32(result["ProductId"]);
                alsoBoughtProd.ProductBrand = result["ProductBrand"].ToString();
                alsoBoughtProd.ProductDescription = result["ProductDescription"].ToString();
                alsoBoughtProd.ProductName = result["ProductName"].ToString();
                alsoBoughtProd.ProductGender = result["ProductGender"].ToString();
                alsoBoughtProd.ProductColor = result["ProductColor"].ToString();
                alsoBoughtProd.ProductPrice =Convert.ToInt32(result["ProductPrice"]);
                alsoBoughtProd.ProductImage = result["ProductImage"].ToString();

                alsoBoughtProdList.Add(alsoBoughtProd);
            }
            myConnection.Close();

            return alsoBoughtProdList;
        }

        //*******************************************************
        //
        // ProductsDB.GetMostPopularProductsOfWeek() Method <a name="GetMostPopularProductsOfWeek"></a>
        //
        // The GetMostPopularProductsOfWeek method returns a struct containing a 
        // forward-only, read-only DataReader containing the most popular products 
        // of the week within the devShop Products database.  
        // The SQLDataReaderResult struct also returns the
        // SQL connection, which must be explicitly closed after the
        // data from the DataReader is bound into the controls.
        //
        // Other relevant sources:
        //     + <a href="ProductsMostPopular.htm" style="color:green">ProductsMostPopular Stored Procedure</a>
        //
        //*******************************************************

        public IEnumerable<ProductDetails> GetMostPopularProductsOfWeek() {

            // Create Instance of Connection and Command Object
            SqliteConnection myConnection = new SqliteConnection(connString);
            SqliteCommand myCommand = new SqliteCommand("SELECT * FROM DevShop_Product_Master LIMIT 5", myConnection);
            
            // Execute the command
            myConnection.Open();
                  
            using (var result = myCommand.ExecuteReader(CommandBehavior.CloseConnection))
            {
                // Return the datareader result
                var popularProdList = new List<ProductDetails>();


                while (result.Read())
                {
                    ProductDetails popularProd = new ProductDetails();

                    popularProd.ProductId = Convert.ToInt32(result["ProductId"]);

                    popularProd.ProductDescription = result["ProductDescription"].ToString();
                    popularProd.ProductName = result["ProductName"].ToString();


                    popularProd.ProductPrice = Convert.ToInt32(result["ProductPrice"]);
                    popularProd.ProductImage = result["ProductImage"].ToString();

                    popularProdList.Add(popularProd);
                }
                return popularProdList;
            }
          
        }

        //*******************************************************
        //
        // ProductsDB.SearchProductDescriptions() Method <a name="SearchProductDescriptions"></a>
        //
        // The SearchProductDescriptions method returns a struct containing
        // a forward-only, read-only DataReader.  This displays a list of all
        // products whose name and/or description contains the specified search
        // string. The SQLDataReaderResult struct also returns the SQL connection,
        // which must be explicitly closed after the data from the DataReader
        // is bound into the controls.
        //
        // Other relevant sources:
        //     + <a href="ProductSearch.htm" style="color:green">ProductSearch Stored Procedure</a>
        //
        //*******************************************************

        public IEnumerable<ProductDetails> SearchProductDescriptions(string searchString)
        {
            using (var myConnection = new SqliteConnection(connString))
            {
                var myCommand = new SqliteCommand("SELECT * FROM DevShop_Product_Master WHERE ProductDescription LIKE @Search", myConnection);
                myCommand.Parameters.AddWithValue("@Search", "%" + searchString + "%");

                myConnection.Open();
                using (var result = myCommand.ExecuteReader())
                {
                    var productsList = new List<ProductDetails>();
                    while (result.Read())
                    {
                        var productDetails = new ProductDetails
                        {
                            ProductId = Convert.ToInt32(result["ProductId"]),
                            ProductDescription = result["ProductDescription"].ToString(),
                            ProductName = result["ProductName"].ToString(),
                            ProductPrice = Convert.ToInt32(result["ProductPrice"]),
                            ProductImage = result["ProductImage"].ToString()
                        };
                        productsList.Add(productDetails);
                    }
                    return productsList;
                }
            }
        }

        public IEnumerable<ProductDetails> GetQuantityDiscounts()
        {
            using (var myConnection = new SqliteConnection(connString))
            {
                var myCommand = new SqliteCommand("SELECT * FROM QuantityDiscounts", myConnection);

                myConnection.Open();
                using (var result = myCommand.ExecuteReader())
                {
                    var discountsList = new List<ProductDetails>();
                    while (result.Read())
                    {
                        var discount = new ProductDetails
                        {
                            ProductId = Convert.ToInt32(result["ProductId"]),
                            ProductPrice = Convert.ToInt32(result["DiscountedPrice"])
                        };
                        discountsList.Add(discount);
                    }
                    return discountsList;
                }
            }
        }

        public static implicit operator ProductsDB(ProductDetails v)
        {
            throw new NotImplementedException();
        }
    }
}