# Database Schema

## Connection String
```
Server=localhost;Database=TestDB;Integrated Security=true;
```

## Tables

### Users
- **Type**: BASE TABLE
- **Columns**:
  - UserId (int, NOT NULL, Position: 1)
  - UserName (nvarchar(100), NOT NULL, Position: 2)
  - Email (nvarchar(255), NULL, Position: 3)
  - CreatedDate (datetime, NOT NULL, Position: 4)

### Orders
- **Type**: BASE TABLE
- **Columns**:
  - OrderId (int, NOT NULL, Position: 1)
  - UserId (int, NOT NULL, Position: 2)
    - **FK**: FK_Orders_Users → Users.UserId
  - OrderDate (datetime, NOT NULL, Position: 3)
  - TotalAmount (decimal, NULL, Position: 4)

### OrderDetails
- **Type**: BASE TABLE
- **Columns**:
  - OrderDetailId (int, NOT NULL, Position: 1)
  - OrderId (int, NOT NULL, Position: 2)
    - **FK**: FK_OrderDetails_Orders → Orders.OrderId
  - ProductId (int, NOT NULL, Position: 3)
    - **FK**: FK_OrderDetails_Products → Products.ProductId
  - Quantity (int, NOT NULL, Position: 4)

### Products
- **Type**: BASE TABLE
- **Columns**:
  - ProductId (int, NOT NULL, Position: 1)
  - ProductName (nvarchar(200), NOT NULL, Position: 2)
  - Price (decimal, NOT NULL, Position: 3)

### UserRoles
- **Type**: BASE TABLE
- **Columns**:
  - UserRoleId (int, NOT NULL, Position: 1)
  - UserId (int, NOT NULL, Position: 2)
    - **FK**: FK_UserRoles_Users → Users.UserId
  - RoleName (nvarchar(50), NOT NULL, Position: 3)

### vw_UserOrders
- **Type**: VIEW
- **Columns**:
  - UserId (int, NOT NULL, Position: 1)
  - UserName (nvarchar(100), NOT NULL, Position: 2)
  - OrderCount (int, NULL, Position: 3)

## Stored Procedures
- sp_GetUserOrders
- sp_CreateOrder
- sp_UpdateOrderStatus
- sp_GetUserById
- sp_DeleteUser
- usp_CalculateTotal
