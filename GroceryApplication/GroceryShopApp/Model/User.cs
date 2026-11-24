namespace GroceryShopApp.Model
{
    /// <summary>
    /// Product class is the class which consists of properties of shop products.
    /// </summary>
    internal class User
    {
        /// <summary>
        /// Gets or sets the fullname of the user.
        /// </summary>
        /// <value>Fullname of user</value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the unique username of the user.
        /// </summary>
        /// <value>Username of user</value>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        /// <value>Phone number of user</value>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        /// <value>Password of user</value>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user is admin or not.
        /// </summary>
        /// <value>True if the user is admin or vice-versa</value>
        public bool IsAdmin { get; set; } = false;
    }
}