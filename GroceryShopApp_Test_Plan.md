# Test Plan – Grocery Shop Application

The following test plan contains 110 test cases covering user registration, login, product viewing, searching, cart management, purchasing, order history, wishlist, admin login, stock management, and data persistence.

All test cases are marked PASS as requested. These are prepared test-plan entries; PASS status should be confirmed against actual execution.

## 1. User Registration and Login

| TEST ID | SCENARIO | EXPECTED RESULT | PASS/FAIL |
|---|---|---|---|
| TC-01 | Register with valid username, password and required details | User account is created successfully | PASS |
| TC-02 | Register with an empty username | Registration is rejected and validation message is displayed | PASS |
| TC-03 | Register with an empty password | Registration is rejected and validation message is displayed | PASS |
| TC-04 | Register with all fields empty | Registration is rejected and required-field messages are displayed | PASS |
| TC-05 | Register using an already existing username | Duplicate account is not created | PASS |
| TC-06 | Register with a username containing only spaces | Registration is rejected | PASS |
| TC-07 | Register with a password that does not satisfy the validation rules | Registration is rejected with an appropriate message | PASS |
| TC-08 | Register with valid details after correcting invalid input | User account is created successfully | PASS |
| TC-09 | Login using valid username and password | User is logged in successfully | PASS |
| TC-10 | Login using an incorrect username | Login is rejected | PASS |
| TC-11 | Login using an incorrect password | Login is rejected | PASS |
| TC-12 | Login with both username and password empty | Login is rejected and validation messages are displayed | PASS |
| TC-13 | Login with an empty username | Login is rejected | PASS |
| TC-14 | Login with an empty password | Login is rejected | PASS |
| TC-15 | Login using an account that does not exist | Login is rejected with an appropriate message | PASS |

## 2. Product Viewing and Searching

| TEST ID | SCENARIO | EXPECTED RESULT | PASS/FAIL |
|---|---|---|---|
| TC-16 | View all available product categories | All available categories are displayed | PASS |
| TC-17 | Select the Vegetable category | All vegetable products are displayed | PASS |
| TC-18 | Select the Fruit category | All fruit products are displayed | PASS |
| TC-19 | Select the Cookware category | All cookware products are displayed | PASS |
| TC-20 | Select an invalid category option | Invalid selection is rejected | PASS |
| TC-21 | Search for an existing product | Matching product details are displayed | PASS |
| TC-22 | Search for a non-existing product | Product not found message is displayed | PASS |
| TC-23 | Search using an empty product name | Search is rejected or an appropriate message is displayed | PASS |
| TC-24 | Search using a product name with different letter casing | Matching product is found if the search is case-insensitive | PASS |
| TC-25 | View product details | Product name, price, category and available stock are displayed | PASS |
| TC-26 | View the product list when products are available | All available products are displayed | PASS |
| TC-27 | View products when no products are available | Appropriate no-products message is displayed | PASS |
| TC-28 | Select an invalid product menu option | Invalid selection is handled without application failure | PASS |

## 3. Cart Management

| TEST ID | SCENARIO | EXPECTED RESULT | PASS/FAIL |
|---|---|---|---|
| TC-29 | Add an available product to the cart | Product is added to the user's cart | PASS |
| TC-30 | Add multiple products to the cart | All selected products are added successfully | PASS |
| TC-31 | Add the same product multiple times | Cart quantity is updated according to the application's cart logic | PASS |
| TC-32 | Add a product with zero available stock | Product cannot be added to the cart | PASS |
| TC-33 | Add a product with a quantity greater than available stock | Addition is rejected with an appropriate message | PASS |
| TC-34 | Enter zero as the cart quantity | Invalid quantity is rejected | PASS |
| TC-35 | Enter a negative number as the cart quantity | Invalid quantity is rejected | PASS |
| TC-36 | Enter alphabetic characters as the cart quantity | Invalid quantity is rejected | PASS |
| TC-37 | View the cart after adding products | All added products and their quantities are displayed | PASS |
| TC-38 | View an empty cart | Appropriate empty-cart message is displayed | PASS |
| TC-39 | Remove an existing product from the cart | Selected product is removed successfully | PASS |
| TC-40 | Remove a product that does not exist in the cart | Removal is rejected with an appropriate message | PASS |
| TC-41 | Clear the cart containing multiple products | All products are removed from the cart | PASS |
| TC-42 | Clear an already empty cart | Application handles the operation without errors | PASS |
| TC-43 | Calculate the total cart amount | Total amount is calculated correctly | PASS |
| TC-44 | Add products to the cart and navigate back to the product menu | Existing cart items are retained | PASS |
| TC-45 | View the cart after removing one product | Removed product is not displayed in the cart | PASS |

## 4. Purchase and Checkout

| TEST ID | SCENARIO | EXPECTED RESULT | PASS/FAIL |
|---|---|---|---|
| TC-46 | Purchase a product immediately using the purchase option | User is taken through the checkout process successfully | PASS |
| TC-47 | Choose not to purchase immediately | Selected product is added to the cart | PASS |
| TC-48 | Purchase all products in the cart | All cart products are included in the order | PASS |
| TC-49 | Purchase selected products from the cart | Only selected products are included in the order | PASS |
| TC-50 | Attempt to purchase an empty cart | Purchase is rejected with an appropriate message | PASS |
| TC-51 | Enter a valid delivery address | Delivery address is accepted | PASS |
| TC-52 | Enter an empty delivery address | Checkout is rejected with a validation message | PASS |
| TC-53 | Enter a delivery address containing only spaces | Invalid address is rejected | PASS |
| TC-54 | Enter a valid delivery date in dd-MM-yyyy format | Delivery date is accepted | PASS |
| TC-55 | Enter a delivery date in an incorrect format | Invalid date is rejected | PASS |
| TC-56 | Enter a delivery date in the past | Past delivery date is rejected | PASS |
| TC-57 | Enter an invalid calendar date such as 32-11-2026 | Invalid date is rejected | PASS |
| TC-58 | Select the 8 AM–12 PM delivery slot | Selected delivery time is saved successfully | PASS |
| TC-59 | Select the 12 PM–4 PM delivery slot | Selected delivery time is saved successfully | PASS |
| TC-60 | Select the 4 PM–8 PM delivery slot | Selected delivery time is saved successfully | PASS |
| TC-61 | Select an invalid delivery time option | Invalid selection is rejected | PASS |
| TC-62 | Complete checkout with valid delivery details | Order is created successfully | PASS |
| TC-63 | Complete checkout for selected cart products | Only selected products are included in the order | PASS |
| TC-64 | Complete checkout with multiple products | Order total is calculated correctly | PASS |
| TC-65 | Complete checkout successfully | Stock is reduced according to the purchased quantities | PASS |
| TC-66 | Attempt checkout when requested quantity exceeds available stock | Order is not completed and an appropriate message is displayed | PASS |
| TC-67 | Complete checkout with valid details | Purchased products are removed from the cart | PASS |
| TC-68 | Complete checkout successfully | A unique order ID is generated | PASS |

## 5. Order History and Tracking

| TEST ID | SCENARIO | EXPECTED RESULT | PASS/FAIL |
|---|---|---|---|
| TC-69 | Select Track Order from the main menu | User's order history is displayed | PASS |
| TC-70 | View order history when previous orders exist | All orders belonging to the logged-in user are displayed | PASS |
| TC-71 | View order history when no orders exist | Appropriate no-orders message is displayed | PASS |
| TC-72 | View an order containing multiple products | All purchased products are displayed correctly | PASS |
| TC-73 | View order details | Order ID, delivery address, delivery date and delivery time are displayed | PASS |
| TC-74 | View the total amount of an existing order | Correct order total is displayed | PASS |
| TC-75 | Login using another user account and view order history | Only the logged-in user's orders are displayed | PASS |

## 6. Wishlist Management

| TEST ID | SCENARIO | EXPECTED RESULT | PASS/FAIL |
|---|---|---|---|
| TC-76 | Add an available product to the wishlist | Product is added to the wishlist successfully | PASS |
| TC-77 | Add multiple products to the wishlist | All selected products are added successfully | PASS |
| TC-78 | Add the same product to the wishlist again | Duplicate wishlist entry is prevented if duplicate checking is implemented | PASS |
| TC-79 | View the wishlist containing products | All wishlist products are displayed | PASS |
| TC-80 | View an empty wishlist | Appropriate empty-wishlist message is displayed | PASS |
| TC-81 | Purchase a product from the wishlist | Selected wishlist product proceeds through checkout | PASS |
| TC-82 | Purchase multiple available wishlist products | Selected products proceed through checkout successfully | PASS |
| TC-83 | Purchase a wishlist product with zero stock | Purchase is rejected with an appropriate message | PASS |
| TC-84 | Attempt to purchase a product from an empty wishlist | Purchase is rejected | PASS |

## 7. Admin Login and Stock Management

| TEST ID | SCENARIO | EXPECTED RESULT | PASS/FAIL |
|---|---|---|---|
| TC-85 | Login using valid admin credentials | Admin is logged in successfully | PASS |
| TC-86 | Login using an incorrect admin username | Admin login is rejected | PASS |
| TC-87 | Login using an incorrect admin password | Admin login is rejected | PASS |
| TC-88 | Login using normal user credentials in the admin login option | Access to the admin menu is denied | PASS |
| TC-89 | Login to the admin account when products have low stock | Low-stock warning message is displayed in red | PASS |
| TC-90 | Login to the admin account when all products have sufficient stock | No low-stock warning is displayed | PASS |
| TC-91 | View the admin stock management product list | All products are displayed with serial numbers and available stock | PASS |
| TC-92 | Select a valid product serial number | Corresponding product is selected for stock updating | PASS |
| TC-93 | Enter an invalid product serial number | Invalid serial number is rejected | PASS |
| TC-94 | Enter zero as the product serial number | Invalid serial number is rejected | PASS |
| TC-95 | Enter a serial number greater than the number of available products | Invalid serial number is rejected | PASS |
| TC-96 | Enter alphabetic characters instead of a serial number | Invalid input is rejected | PASS |
| TC-97 | Enter a valid new stock quantity | Selected product stock is updated successfully | PASS |
| TC-98 | Enter zero as the new stock quantity | Product stock is updated to zero | PASS |
| TC-99 | Enter a negative stock quantity | Invalid stock quantity is rejected | PASS |
| TC-100 | Enter alphabetic characters as the new stock quantity | Invalid stock quantity is rejected | PASS |

## 8. Data Storage and Persistence

| TEST ID | SCENARIO | EXPECTED RESULT | PASS/FAIL |
|---|---|---|---|
| TC-101 | Register a new user | User details are saved in the JSON file | PASS |
| TC-102 | Add a new product to the cart | Cart details are saved successfully | PASS |
| TC-103 | Complete an order | Order details are saved in the JSON file | PASS |
| TC-104 | Update product stock as admin | Updated stock quantity is saved in the product JSON file | PASS |
| TC-105 | Restart the application after registering a user | Previously registered user details are retained | PASS |
| TC-106 | Restart the application after updating stock | Updated product stock is retained | PASS |
| TC-107 | Restart the application after placing an order | Previously placed order details are retained | PASS |
| TC-108 | Load the application when a JSON data file does not exist | Application handles the missing file according to its initialization logic | PASS |
| TC-109 | Load the application with an empty JSON data file | Application handles empty data without unexpected termination | PASS |
| TC-110 | Perform multiple operations and restart the application | Previously saved application data is loaded successfully | PASS |

---

**Total Test Cases: 110**
