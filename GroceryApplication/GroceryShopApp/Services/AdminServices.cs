
using System.Security.Cryptography;
using System.Text;
using GroceryShopApp.Model;
using GroceryShopApp.Repository;

namespace GroceryShopApp.Services
{
    /// <summary>
    /// Handles the business logics for admin operations.
    /// </summary>
    internal class AdminServices
    {
        private const int LowStockLimit = 5;
        private readonly UserRepository _userRepository;
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;
        private readonly CategoryRepository _categoryRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminServices"/> class.
        /// </summary>
        /// <param name="userRepository">Repository of users.</param>
        /// <param name="productRepository">Repository of products.</param>
        /// <param name="orderRepository">Repository of orders.</param>
        /// <param name="categoryRepository">Repository of categories.</param>
        public AdminServices(UserRepository userRepository, ProductRepository productRepository, OrderRepository orderRepository, CategoryRepository categoryRepository)
        {
            this._userRepository = userRepository;
            this._productRepository = productRepository;
            this._orderRepository = orderRepository;
            this._categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Checks whether the user is an admin or not.
        /// </summary>
        /// <param name="userName">Username of admin.</param>
        /// <param name="password">Password of admin.</param>
        /// <returns>Returns whether the user is admin or not.</returns>
        internal bool IsAdmin(string userName, string password)
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            string hashedPassword = Convert.ToHexString(hash).ToLowerInvariant();
            if (this._userRepository.IsUserAdmin(userName, hashedPassword))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a new category.
        /// </summary>
        /// <param name="categoryName">Category name.</param>
        /// <returns>Returns whether the category is added.</returns>
        internal string AddCategory(string categoryName)
        {
            categoryName = categoryName.Trim();
            string validationMessage = Validator.ValidateCategory(categoryName);
            if (validationMessage != "passed")
            {
                return validationMessage;
            }

            if (this._categoryRepository.IsCategoryExists(categoryName))
            {
                return "Category already exists.";
            }

            this._categoryRepository.AddCategory(categoryName);
            return "Category added successfully.";
        }

        /// <summary>
        /// Adds a new product to an existing category.
        /// </summary>
        /// <param name="productName">Product name.</param>
        /// <param name="category">Product category.</param>
        /// <param name="price">Product price.</param>
        /// <param name="quantity">Available quantity.</param>
        /// <param name="imageFile">Product image filename.</param>
        /// <returns>Returns whether the product is added.</returns>
        internal string AddProduct(string productName, string category, decimal price, int quantity, string imageFile)
        {
            productName = productName.Trim();
            string validationMessage = Validator.ValidateProductName(productName);
            if (validationMessage != "passed")
            {
                return validationMessage;
            }

            if (this._productRepository.IsProductExists(productName))
            {
                return "Product already exists.";
            }

            if (!this._categoryRepository.IsCategoryExists(category))
            {
                return "Selected category does not exist.";
            }

            if (price <= 0)
            {
                return "Product price must be greater than zero.";
            }

            if (quantity < 0)
            {
                return "Product quantity cannot be negative.";
            }

            Product product = new Product
            {
                Name = productName,
                Category = category,
                Price = price,
                Quantity = quantity,
                ImageFile = imageFile.Trim(),
            };

            this._productRepository.AddProduct(product);
            return "Product added successfully.";
        }

        /// <summary>
        /// Updates the stock of an existing product.
        /// </summary>
        /// <param name="productName">Product name.</param>
        /// <param name="quantity">Quantity to be added.</param>
        /// <returns>Returns whether the stock is updated.</returns>
        internal string UpdateProductStock(string productName, int quantity)
        {
            if (quantity <= 0)
            {
                return "Stock quantity must be greater than zero.";
            }

            Product? product = this._productRepository.GetAllProducts().FirstOrDefault(item => item.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));
            if (product == null)
            {
                return "Product not found.";
            }

            if (product.Quantity > int.MaxValue - quantity)
            {
                return "Stock quantity exceeds the allowed limit.";
            }

            this._productRepository.AddProductStock(productName, quantity);
            return "Product stock updated successfully.";
        }

        /// <summary>
        /// Retrieves all registered users.
        /// </summary>
        /// <returns>List of all users.</returns>
        internal List<User> GetAllUsers()
        {
            return this._userRepository.GetAllUsers();
        }

        /// <summary>
        /// Promotes a customer to admin.
        /// </summary>
        /// <param name="userName">Username of the customer.</param>
        /// <returns>Returns whether the customer is promoted.</returns>
        internal string PromoteUser(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return "Username cannot be empty.";
            }

            bool isPromoted = this._userRepository.PromoteUser(userName.Trim());
            if (!isPromoted)
            {
                return "User not found or already an admin.";
            }

            return "User promoted to admin successfully.";
        }

        /// <summary>
        /// Retrieves products with low stock.
        /// </summary>
        /// <returns>List of low-stock products.</returns>
        internal List<Product> GetLowStockProducts()
        {
            return this._productRepository.GetAllProducts().Where(product => product.Quantity <= LowStockLimit).ToList();
        }

        /// <summary>
        /// Retrieves all the customer orders.
        /// </summary>
        /// <returns>List of all orders.</returns>
        internal List<Order> GetAllOrders()
        {
            return this._orderRepository.GetAllOrders();
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
