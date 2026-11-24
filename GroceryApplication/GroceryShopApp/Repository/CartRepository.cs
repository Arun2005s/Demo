using System.Text.Json;
using System.Text.Json.Serialization;
using GroceryShopApp.Model;

namespace GroceryShopApp.Repository
{
    /// <summary>
    /// Handles the storage and retrieval of cart data to and from files.
    /// </summary>
    internal class CartRepository
    {
        private readonly List<Cart> _carts = new List<Cart>();
        private readonly string _filePath;

        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CartRepository"/> class.
        /// </summary>
        /// <param name="filePath">Path where the cart data-file is located.</param>
        public CartRepository(string filePath)
        {
            this._filePath = filePath;
            this.LoadAll();
        }

        /// <summary>
        /// Retrieves all cart products of a particular user.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        /// <returns>List of cart products.</returns>
        internal List<Cart> GetCart(string userName)
        {
            return this._carts.Where(cart => cart.UserName == userName).ToList();
        }

        /// <summary>
        /// Adds a product to the user's cart or updates its quantity.
        /// </summary>
        /// <param name="cart">Cart details.</param>
        internal void AddToCart(Cart cart)
        {
            Cart? existingCart = this._carts.FirstOrDefault(item => item.UserName == cart.UserName && item.Product.Name == cart.Product.Name);
            if (existingCart != null)
            {
                existingCart.CartQuantity = cart.CartQuantity;
                existingCart.Product = cart.Product;
            }
            else
            {
                this._carts.Add(cart);
            }

            this.WriteAll();
        }

        /// <summary>
        /// Removes all cart products of a particular user.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        internal void RemoveCart(string userName)
        {
            this._carts.RemoveAll(cart => cart.UserName == userName);
            this.WriteAll();
        }

        private void WriteAll()
        {
            string fileData = JsonSerializer.Serialize(this._carts, this._options);
            File.WriteAllText(this._filePath, fileData);
        }

        private void LoadAll()
        {
            if (!File.Exists(this._filePath))
            {
                this.WriteAll();
                return;
            }

            string fileData = File.ReadAllText(this._filePath);
            if (string.IsNullOrWhiteSpace(fileData))
            {
                return;
            }

            List<Cart> carts = JsonSerializer.Deserialize<List<Cart>>(fileData, this._options) ?? new List<Cart>();
            this._carts.AddRange(carts);
        }
    }
}