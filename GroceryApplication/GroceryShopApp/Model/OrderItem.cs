namespace GroceryShopApp.Model
{
    /// <summary>
    /// OrderItem class consists of the purchased product details.
    /// </summary>
    internal class OrderItem
    {
        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>Name of product</value>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the price of the product.
        /// </summary>
        /// <value>Price of product</value>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the purchased quantity.
        /// </summary>
        /// <value>Quantity purchased</value>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the subtotal of the product.
        /// </summary>
        /// <value>Subtotal of product</value>
        public decimal SubTotal { get; set; }
    }
}