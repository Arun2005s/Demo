using GroceryShopApp.Model;
using GroceryShopApp.Repository;

namespace GroceryShopApp.Services
{
    /// <summary>
    /// Handles the business logics for cart and checkout operations.
    /// </summary>
    internal class CartServices
    {
        private readonly CartRepository _cartRepository;
        private readonly ProductRepository _productRepository;
        private readonly OrderRepository _orderRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CartServices"/> class.
        /// </summary>
        /// <param name="cartRepository">Repository of the cart.</param>
        /// <param name="productRepository">Repository of the products.</param>
        /// <param name="orderRepository">Repository of the orders.</param>
        public CartServices(CartRepository cartRepository, ProductRepository productRepository, OrderRepository orderRepository)
        {
            this._cartRepository = cartRepository;
            this._productRepository = productRepository;
            this._orderRepository = orderRepository;
        }

        /// <summary>
        /// Adds a product to the user's cart.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        /// <param name="productName">Name of the product.</param>
        /// <param name="quantity">Quantity to be added.</param>
        /// <returns>Returns whether the product was added or not.</returns>
        internal string AddToCart(string userName, string productName, int quantity)
        {
            if (quantity <= 0)
            {
                return "Quantity must be greater than zero.";
            }

            Product? product = this._productRepository.GetAllProducts().FirstOrDefault(item => item.Name == productName);
            if (product == null)
            {
                return "Product not found.";
            }

            if (product.Quantity == 0)
            {
                return "Product is out of stock.";
            }

            Cart? existingCart = this._cartRepository.GetCart(userName).FirstOrDefault(item => item.Product.Name == productName);
            int cartQuantity = quantity;
            if (existingCart != null)
            {
                cartQuantity += existingCart.CartQuantity;
            }

            if (cartQuantity > product.Quantity)
            {
                return $"Only {product.Quantity} quantity is available.";
            }

            Cart cart = new Cart
            {
                UserName = userName,
                Product = product,
                CartQuantity = cartQuantity,
            };

            this._cartRepository.AddToCart(cart);
            return "Product added to cart successfully.";
        }

        /// <summary>
        /// Retrieves all products in the user's cart.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        /// <returns>List of cart products.</returns>
        internal List<Cart> GetCart(string userName)
        {
            return this._cartRepository.GetCart(userName);
        }

        /// <summary>
        /// Calculates the total amount of the user's cart.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        /// <returns>Total cart amount.</returns>
        internal decimal GetCartTotal(string userName)
        {
            List<Cart> carts = this.GetCart(userName);
            decimal totalAmount = 0;
            foreach (Cart cart in carts)
            {
                totalAmount += cart.Product.Price * cart.CartQuantity;
            }

            return totalAmount;
        }

        /// <summary>
        /// Processes the checkout of the user's cart.
        /// </summary>
        /// <param name="userName">Username of the user.</param>
        /// <param name="deliveryAddress">Delivery address.</param>
        /// <param name="deliveryTime">Delivery time.</param>
        /// <returns>Returns the order after successful checkout.</returns>
        internal Order? Checkout(string userName, string deliveryAddress, string deliveryTime)
        {
            List<Cart> carts = this.GetCart(userName);
            if (carts.Count == 0)
            {
                return null;
            }

            List<Product> products = this._productRepository.GetAllProducts();
            foreach (Cart cart in carts)
            {
                Product? product = products.FirstOrDefault(item => item.Name == cart.Product.Name);

                if (product == null || product.Quantity < cart.CartQuantity)
                {
                    return null;
                }
            }

            Order order = new Order
            {
                OrderId = this._orderRepository.GetAllOrders().Select(item => item.OrderId).DefaultIfEmpty(0).Max() + 1,
                UserName = userName,
                DeliveryAddress = deliveryAddress,
                DeliveryTime = deliveryTime,
                OrderDate = DateTime.Now,
                TotalAmount = 0,
            };

            foreach (Cart cart in carts)
            {
                OrderItem orderItem = new OrderItem
                {
                    ProductName = cart.Product.Name,
                    Price = cart.Product.Price,
                    Quantity = cart.CartQuantity,
                    SubTotal = cart.Product.Price * cart.CartQuantity,
                };
                order.Items.Add(orderItem);
                order.TotalAmount += orderItem.SubTotal;
            }

            foreach (Cart cart in carts)
            {
                this._productRepository.UpdateProductStock(cart.Product.Name, cart.CartQuantity);
            }

            this._orderRepository.AddOrder(order);
            this._cartRepository.RemoveCart(userName);
            return order;
        }
    }
}