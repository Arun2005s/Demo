using System.Text.Json;
using GroceryShopApp.Model;

namespace GroceryShopApp.Repository
{
    /// <summary>
    /// Handles the storage and retrieval of category data to and from files.
    /// </summary>
    internal class CategoryRepository
    {
        private readonly List<string> _categories = new List<string>();
        private readonly string _filePath;

        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
        /// </summary>
        /// <param name="filePath">Path where the category data-file is located.</param>
        public CategoryRepository(string filePath)
        {
            this._filePath = filePath;
            this.LoadAll();
        }

        /// <summary>
        /// Retrieves all available categories.
        /// </summary>
        /// <returns>List of categories.</returns>
        internal List<string> GetAllCategories()
        {
            return this._categories.ToList();
        }

        /// <summary>
        /// Adds a new category to the repository.
        /// </summary>
        /// <param name="category">Category name.</param>
        internal void AddCategory(string category)
        {
            this._categories.Add(category);
            this.WriteAll();
        }

        /// <summary>
        /// Checks whether the category already exists.
        /// </summary>
        /// <param name="category">Category name.</param>
        /// <returns>Returns true if category exists.</returns>
        internal bool IsCategoryExists(string category)
        {
            return this._categories.Any(item => item.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        private void WriteAll()
        {
            string fileData = JsonSerializer.Serialize(this._categories, this._options);
            File.WriteAllText(this._filePath, fileData);
        }

        private void LoadAll()
        {
            if (!File.Exists(this._filePath))
            {
                this._categories.Add(Categories.Vegetable);
                this._categories.Add(Categories.Fruit);
                this._categories.Add(Categories.Cookware);
                this.WriteAll();
                return;
            }

            string fileData = File.ReadAllText(this._filePath);

            if (string.IsNullOrWhiteSpace(fileData))
            {
                this._categories.Add(Categories.Vegetable);
                this._categories.Add(Categories.Fruit);
                this._categories.Add(Categories.Cookware);
                this.WriteAll();
                return;
            }

            List<string> categories = JsonSerializer.Deserialize<List<string>>(fileData, this._options) ?? new List<string>();
            this._categories.AddRange(categories);
            if (!this.IsCategoryExists(Categories.Vegetable))
            {
                this._categories.Add(Categories.Vegetable);
            }

            if (!this.IsCategoryExists(Categories.Fruit))
            {
                this._categories.Add(Categories.Fruit);
            }

            if (!this.IsCategoryExists(Categories.Cookware))
            {
                this._categories.Add(Categories.Cookware);
            }

            this.WriteAll();
        }
    }
}
