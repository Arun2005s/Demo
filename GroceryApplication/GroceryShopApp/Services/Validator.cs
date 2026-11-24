namespace GroceryShopApp.Services
{
    /// <summary>
    /// Validates the fields for appropriate data usage.
    /// </summary>
    internal static class Validator
    {
        /// <summary>
        /// Validates the user entered registering details.
        /// </summary>
        /// <param name="fullName">Full name of the user</param>
        /// <param name="userName">Unique username of the user</param>
        /// <param name="phone">Phone number of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>Returns whether the details entered are valid or not</returns>
        internal static string ValidateSignupFields(string? fullName, string? userName, string? phone, string? password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(password))
            {
                return "Full-Name is invalid";
            }

            if (string.IsNullOrWhiteSpace(userName) || userName.Length < 4 || userName.Length > 10)
            {
                return $"User Name is invalid";
            }

            if (string.IsNullOrWhiteSpace(phone) || phone.Length != 10 || !IsDigit(phone))
            {
                return $"Phone number is invalid";
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || !IsAlphaNumeric(password))
            {
                return $"Password is invalid";
            }

            return "passed";
        }

        /// <summary>
        ///  Validates the user entered log-in details.
        /// </summary>
        /// <param name="userName">Unique username of the user</param>
        /// <param name="password">Password of the user</param>
        /// <returns>Returns whether the details entered are valid or not</returns>
        internal static string ValidateLoginFields(string? userName, string? password)
        {
            if (string.IsNullOrWhiteSpace(userName) || userName.Length < 4 || userName.Length > 10)
            {
                return $"User Name is invalid";
            }

            if (string.IsNullOrWhiteSpace(password) || !IsAlphaNumeric(password) || password.Length < 8)
            {
                return $"Password is invalid";
            }

            return "passed";
        }

        /// <summary>
        /// Validates the delivery address entered by the user.
        /// </summary>
        /// <param name="deliveryAddress">Delivery address.</param>
        /// <returns>Returns whether the address is valid or not.</returns>
        internal static string ValidateDeliveryAddress(string? deliveryAddress)
        {
            if (string.IsNullOrWhiteSpace(deliveryAddress))
            {
                return "Delivery address cannot be empty.";
            }

            if (deliveryAddress.Trim().Length < 10 || deliveryAddress.Trim().Length > 200)
            {
                return "Delivery address must contain 10 to 200 characters.";
            }

            return "passed";
        }

        /// <summary>
        /// Validates the delivery time entered by the user.
        /// </summary>
        /// <param name="deliveryTime">Delivery time.</param>
        /// <returns>Returns whether the delivery time is valid or not.</returns>
        internal static string ValidateDeliveryTime(string? deliveryTime)
        {
            if (string.IsNullOrWhiteSpace(deliveryTime))
            {
                return "Delivery time cannot be empty.";
            }

            if (!TimeOnly.TryParseExact(deliveryTime, "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out TimeOnly time))
            {
                return "Enter delivery time in HH:mm format.";
            }

            if (time < new TimeOnly(8, 0) || time > new TimeOnly(22, 0))
            {
                return "Delivery time must be between 08:00 and 22:00.";
            }

            return "passed";
        }


        /// <summary>
        /// Validates the category name entered by the admin.
        /// </summary>
        /// <param name="categoryName">Category name.</param>
        /// <returns>Returns whether the category name is valid or not.</returns>
        internal static string ValidateCategory(string? categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return "Category name cannot be empty.";
            }

            if (categoryName.Trim().Length < 3 || categoryName.Trim().Length > 30)
            {
                return "Category name must contain 3 to 30 characters.";
            }

            foreach (char character in categoryName.Trim())
            {
                if (!char.IsLetterOrDigit(character) && character != ' ' && character != '&' && character != '-')
                {
                    return "Category name can contain only letters, digits, spaces, & and -.";
                }
            }

            return "passed";
        }

        /// <summary>
        /// Validates the product name entered by the admin.
        /// </summary>
        /// <param name="productName">Product name.</param>
        /// <returns>Returns whether the product name is valid or not.</returns>
        internal static string ValidateProductName(string? productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                return "Product name cannot be empty.";
            }

            if (productName.Trim().Length < 2 || productName.Trim().Length > 50)
            {
                return "Product name must contain 2 to 50 characters.";
            }

            return "passed";
        }

        /// <summary>
        /// Validates the price entered by the admin.
        /// </summary>
        /// <param name="price">Product price.</param>
        /// <returns>Returns whether the price is valid or not.</returns>
        internal static string ValidateProductPrice(string? price)
        {
            if (!decimal.TryParse(price, out decimal productPrice) || productPrice <= 0)
            {
                return "Product price must be greater than zero.";
            }

            return "passed";
        }

        /// <summary>
        /// Validates the stock quantity entered by the admin.
        /// </summary>
        /// <param name="quantity">Product quantity.</param>
        /// <returns>Returns whether the quantity is valid or not.</returns>
        internal static string ValidateProductQuantity(string? quantity)
        {
            if (!int.TryParse(quantity, out int productQuantity) || productQuantity < 0)
            {
                return "Product quantity must be a valid non-negative number.";
            }

            return "passed";
        }

        /// <summary>
        /// Validates the stock quantity to be added.
        /// </summary>
        /// <param name="quantity">Quantity to be added.</param>
        /// <returns>Returns whether the quantity is valid or not.</returns>
        internal static string ValidateStockQuantity(string? quantity)
        {
            if (!int.TryParse(quantity, out int stockQuantity) || stockQuantity <= 0)
            {
                return "Stock quantity must be greater than zero.";
            }

            return "passed";
        }

        private static bool IsAlphaNumeric(string data)
        {
            int digit = 0;
            int letter = 0;

            foreach (char c in data)
            {
                if (char.IsDigit(c))
                {
                    digit += 1;
                }

                if (char.IsLetter(c))
                {
                    letter += 1;
                }
            }

            return digit > 0 && letter > 0;
        }

        private static bool IsDigit(string data)
        {
            foreach (char c in data)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
