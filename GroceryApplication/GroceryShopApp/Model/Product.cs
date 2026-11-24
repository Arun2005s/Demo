
namespace GroceryShopApp.Model
{
    /// <summary>
    /// Product class is the class which consists of properties of shop products.
    /// </summary>
    internal class Product
    {
        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>Name of product</value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the price of the product.
        /// </summary>
        /// <value>Price of product</value>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the category of the product.
        /// </summary>
        /// <value>Category of product</value>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quantity of the product.
        /// </summary>
        /// <value>Quantity of product</value>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the Imagefile of the product.
        /// </summary>
        /// <value>Imagefile of product</value>
        public string ImageFile { get; set; } = string.Empty;
    }
}
