
using GroceryShopApp.Model;
using GroceryShopApp.Repository;

namespace GroceryShopApp.Services
{
    /// <summary>
    /// Handles the business logics for the products.
    /// </summary>
    internal class ProductServices
    {
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductServices"/> class.
        /// </summary>
        /// <param name="productRepository">Repository of the products.</param>
        /// <param name="categoryRepository">Repository of the categories.</param>
        public ProductServices(ProductRepository productRepository, CategoryRepository categoryRepository)
        {
            this._productRepository = productRepository;
            this._categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Retrieves the list of products from a particular category.
        /// </summary>
        /// <param name="category">Category of the product.</param>
        /// <returns>Returns the list of product of selected category.</returns>
        internal List<Product> GetProductsByCategory(string category)
        {
            List<Product> products = this._productRepository.GetAllProducts();
            return products.Where(product => product.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Handles the searching of products and returns the selected product.
        /// </summary>
        /// <param name="category">Category of the product to be found.</param>
        /// <param name="searchName">Name of the product to be found.</param>
        /// <returns>Returns the selected product.</returns>
        internal List<Product> SearchProducts(string category, string searchName)
        {
            List<Product> products = this.GetProductsByCategory(category);
            return products.Where(product => product.Name.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// Retrieves all products from the repository.
        /// </summary>
        /// <returns>List of all products.</returns>
        internal List<Product> GetAllProducts()
        {
            return this._productRepository.GetAllProducts();
        }

        /// <summary>
        /// Retrieves all available categories.
        /// </summary>
        /// <returns>List of categories.</returns>
        internal List<string> GetAllCategories()
        {
            return this._categoryRepository.GetAllCategories();
        }
    }
}
