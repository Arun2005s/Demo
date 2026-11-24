namespace GroceryShopApp.Model
{
    /// <summary>
    /// Cart class is the class which consists of properties of cart products.
    /// </summary>
    internal class Cart
    {
        /// <summary>
        /// Gets or sets the UserName of the user.
        /// </summary>
        /// <value>Name of user</value>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        /// <value>Product values</value>
        public Product Product { get; set; } = new Product();

        /// <summary>
        /// Gets or sets the quantity of the product in cart.
        /// </summary>
        /// <value>Quantity of product in cart</value>
        public int CartQuantity { get; set; }
    }
}