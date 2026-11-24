namespace GroceryShopApp.Model
{
    /// <summary>
    /// Order class consists of the details of customer orders.
    /// </summary>
    internal class Order
    {
        /// <summary>
        /// Gets or sets the order ID.
        /// </summary>
        /// <value>Unique order ID</value>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the username of the customer.
        /// </summary>
        /// <value>Username of customer</value>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delivery address.
        /// </summary>
        /// <value>Delivery address</value>
        public string DeliveryAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delivery time.
        /// </summary>
        /// <value>Delivery time</value>
        public string DeliveryTime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the order date.
        /// </summary>
        /// <value>Date of order</value>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Gets or sets the list of purchased products.
        /// </summary>
        /// <value>Purchased products</value>
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Gets or sets the total amount of the order.
        /// </summary>
        /// <value>Total order amount</value>
        public decimal TotalAmount { get; set; }
    }
}