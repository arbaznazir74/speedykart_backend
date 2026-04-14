# SpeedyKart — Order Flow API Documentation

> Base URL: `https://p01--speedykart-backend--m8hzvkqskqhx.code.run`
> All requests wrapped in `{ "reqData": { ... } }`
> All responses wrapped in `{ "successData": ..., "isError": false, "errorData": null }`
> Auth: `Authorization: Bearer <JWT_TOKEN>`

---

## 1. ADD TO CART

### `POST /api/v1/Cart/add`
**Role:** User

**Request:**
```json
{
  "reqData": {
    "productVariantId": 80,
    "quantity": 2,
    "platformType": 1,
    "selectedToppings": [
      {
        "toppingId": 1,
        "toppingName": "Extra Cheese",
        "price": 25.00,
        "quantity": 1
      }
    ],
    "selectedAddons": [
      {
        "addonProductId": 5,
        "addonName": "French Fries",
        "price": 60.00,
        "quantity": 1,
        "categoryId": 3,
        "categoryName": "Sides"
      }
    ]
  }
}
```

> `platformType`: `1` = HotBox, `2` = SpeedyMart
> `selectedToppings` and `selectedAddons` are **optional** — omit or send `null` if none.
> Server overrides prices from DB (server is source of truth).

**Response:** `CombineCartSM`
```json
{
  "successData": {
    "id": 11,
    "hotBoxCart": {
      "id": 5,
      "userId": 100,
      "platformType": 1,
      "subTotal": 725.00,
      "taxAmount": 0,
      "discountAmount": 0,
      "grandTotal": 725.00
    },
    "speedyMartCart": null,
    "hotBoxCartItems": [
      {
        "id": 101,
        "cartId": 5,
        "productVariantId": 80,
        "hotBoxProductDetails": {
          /* product variant info: name, image, price, indicator, etc. */
        },
        "quantity": 2,
        "unitPrice": 320.00,
        "totalPrice": 725.00,
        "selectedToppings": [
          { "toppingId": 1, "toppingName": "Extra Cheese", "price": 25.00, "quantity": 1 }
        ],
        "selectedAddons": [
          { "addonProductId": 5, "addonName": "French Fries", "price": 60.00, "quantity": 1, "categoryId": 3, "categoryName": "Sides" }
        ],
        "toppingsTotal": 25.00,
        "addonsTotal": 60.00
      }
    ],
    "speedyMartCartItems": null
  }
}
```

---

## 2. GET MY CART

### `GET /api/v1/Cart/mine`
**Role:** User

**Response:** Same `CombineCartSM` as above.

---

## 3. UPDATE CART ITEM QUANTITY

### `PUT /api/v1/Cart/update`
**Role:** User

**Request:**
```json
{
  "reqData": {
    "productVariantId": 80,
    "quantity": 3,
    "platformType": 1
  }
}
```

> Send `quantity: 0` to remove the item.

**Response:** `CombineCartSM` (updated cart).

---

## 4. REMOVE FROM CART

### `DELETE /api/v1/Cart/remove`
**Role:** User

**Request:**
```json
{
  "reqData": {
    "productVariantId": 80,
    "platformType": 1
  }
}
```

**Response:** `CombineCartSM` (updated cart).

---

## 5. CLEAR CART

### `DELETE /api/v1/Cart/clear?platform=1`
**Role:** User

**Response:** `CombineCartSM` (empty cart).

---

## 6. CHECK DELIVERY AVAILABILITY

### `GET /api/v1/Order/is-product-servicable?productVariantId=80`
**Role:** User

**Response:**
```json
{
  "successData": {
    "boolResponse": true,
    "message": "Delivery available at your location"
  }
}
```

---

## 7. GET DELIVERY CHARGES

### `GET /api/v1/Order/delivery-charges`
**Role:** User

**Response:**
```json
{
  "successData": {
    "deliveryCharge": 30.00
  }
}
```

---

## 8. PLACE ORDER

### `POST /api/v1/Order/mine`
**Role:** User

> **Prerequisite:** User must have a **default address** set.

**Request:**
```json
{
  "reqData": {
    "order": {
      "currency": "INR",
      "amount": 785.00,
      "tipAmount": 0,
      "isCutlaryInculded": false,
      "deliveryCharge": 30.00,
      "platormCharge": 5.00,
      "cutlaryCharge": 0,
      "lowCartFeeCharge": 0,
      "paymentMode": 1,
      "sellerId": 2
    },
    "orderItems": [
      {
        "productVariantId": 80,
        "quantity": 2,
        "unitPrice": 320.00,
        "selectedToppings": [
          { "toppingId": 1, "toppingName": "Extra Cheese", "price": 25.00, "quantity": 1 }
        ],
        "selectedAddons": [
          { "addonProductId": 5, "addonName": "French Fries", "price": 60.00, "quantity": 1, "categoryId": 3, "categoryName": "Sides" }
        ]
      }
    ]
  }
}
```

> `paymentMode`: `1` = COD, `2` = Online
> `selectedToppings` / `selectedAddons`: Send as **JSON arrays** (not strings). Server serializes them internally.

**Response:** `OrderSM`
```json
{
  "successData": {
    "id": 43,
    "orderNumber": "ORD-20260414-XXXX",
    "transactionId": 123456789,
    "razorpayOrderId": null,
    "userId": 100,
    "customerName": "John",
    "receipt": "Receipt#...",
    "currency": "INR",
    "amount": 785.00,
    "tipAmount": 0,
    "paidAmount": 0,
    "dueAmount": 785.00,
    "deliveryCharge": 30.00,
    "platormCharge": 5.00,
    "cutlaryCharge": 0,
    "lowCartFeeCharge": 0,
    "paymentStatus": 0,
    "orderStatus": 0,
    "paymentMode": 1,
    "sellerId": 2,
    "preparationTimeInMinutes": 0,
    "sellerAcceptedAt": null,
    "preparationStatus": null,
    "preparationStatusMessage": null
  }
}
```

> `paymentStatus`: `0`=Pending, `1`=Paid, `2`=Failed, `3`=RefundInitiated, `4`=Refunded, `5`=Cancelled
> `orderStatus`: `0`=Created, `1`=Processing, `2`=SellerAccepted, `3`=Shipped, `4`=Delivered, `5`=Cancelled, `6`=CancelledBySeller

---

## 9. ONLINE PAYMENT (Razorpay)

### `POST /api/Payment/mine/checkout?orderId=43`
**Role:** User

> Only needed for `paymentMode: 2` (Online). Creates a Razorpay order.

**Response:** `OrderSM` (with `razorpayOrderId` populated).

### `POST /api/Payment/webhook` (Razorpay callback — server-to-server)

---

## 10. GET MY ORDERS

### `GET /api/v1/Order/mine?skip=0&top=10`
**Role:** User

**Response:**
```json
{
  "successData": [
    {
      "id": 43,
      "orderNumber": "ORD-20260414-XXXX",
      "customerName": "John",
      "amount": 785.00,
      "paymentStatus": 1,
      "orderStatus": 2,
      "preparationStatus": "Preparing",
      "preparationStatusMessage": "Your order is being prepared. Estimated 25 min remaining."
      /* ... all OrderSM fields */
    }
  ]
}
```

### `GET /api/v1/Order/mine/count`
Returns `{ "successData": { "intResponse": 5, "message": "..." } }`

---

## 11. GET ORDER ITEMS (Detail View)

### `GET /api/v1/Order/order-items/{orderId}?skip=0&top=50`
**Role:** User, Seller, Admin

**Response:** `List<OrderItemDetailSM>`
```json
{
  "successData": [
    {
      "id": 53,
      "orderId": 43,
      "customerName": "John",
      "productId": 10,
      "baseProductName": "Margherita Pizza",
      "productVariantId": 80,
      "variantName": "Medium",
      "variantImageBase64": "data:image/...",
      "indicator": "Veg",
      "quantity": 2,
      "unitPrice": 320.00,
      "totalPrice": 640.00,
      "paymentStatus": 1,
      "orderStatus": 2,
      "paymentMode": 1,
      "toppings": [
        { "toppingId": 1, "toppingName": "Extra Cheese", "price": 25.00, "quantity": 1 }
      ],
      "addons": [
        { "addonProductId": 5, "addonName": "French Fries", "price": 60.00, "quantity": 1, "categoryId": 3, "categoryName": "Sides" }
      ]
    }
  ]
}
```

---

## 12. GET INVOICE

### `GET /api/v1/Order/invoice/{orderId}`
**Role:** User, Seller, Admin

**Response:** `OrderInvoiceSM`
```json
{
  "successData": {
    "invoiceNumber": "INV-...",
    "invoiceDate": "2026-04-14T12:00:00Z",
    "orderId": 43,
    "orderNumber": "ORD-20260414-XXXX",
    "seller": {
      "id": 2,
      "name": "Seller Name",
      "storeName": "Store Name",
      "email": "seller@example.com",
      "mobile": "9876543210",
      "address": "...",
      "city": "...",
      "state": "...",
      "country": "India",
      "fssaiLicNo": "...",
      "taxName": "GST",
      "taxNumber": "..."
    },
    "customer": {
      "id": 100,
      "name": "John",
      "mobile": "9876543210",
      "email": "john@example.com"
    },
    "deliveryAddress": {
      "name": "John",
      "mobile": "9876543210",
      "address": "123 Main St",
      "landmark": "Near Park",
      "area": "Downtown",
      "pincode": "110001",
      "city": "Delhi",
      "state": "Delhi",
      "country": "India"
    },
    "items": [
      {
        "productVariantId": 80,
        "baseProductName": "Margherita Pizza",
        "variantName": "Medium",
        "indicator": "Veg",
        "quantity": 2,
        "unitPrice": 320.00,
        "totalPrice": 640.00,
        "toppings": [
          { "toppingId": 1, "toppingName": "Extra Cheese", "price": 25.00, "quantity": 1 }
        ],
        "toppingsTotal": 25.00,
        "addons": [
          { "addonProductId": 5, "addonName": "French Fries", "price": 60.00, "quantity": 1, "categoryId": 3, "categoryName": "Sides" }
        ],
        "addonsTotal": 60.00,
        "lineTotal": 725.00
      }
    ],
    "subtotal": 725.00,
    "deliveryCharge": 30.00,
    "platformCharge": 5.00,
    "cutleryCharge": 0,
    "lowCartFeeCharge": 0,
    "tipAmount": 0,
    "totalAmount": 785.00,
    "currency": "INR",
    "paymentMode": "Cod",
    "paymentStatus": "Paid",
    "orderStatus": "SellerAccepted"
  }
}
```

---

## 13. SELLER — ORDER MANAGEMENT

### Get Seller Orders
`GET /api/v1/Seller/mine/orders?skip=0&top=10` → `List<OrderItemSM>`

### Get Single Order Item Detail
`GET /api/v1/Seller/mine/order/{orderItemId}` → `OrderItemExtendedDetailsSM`
```json
{
  "successData": {
    "orderItem": {
      "id": 53,
      "orderId": 43,
      "productVariantId": 80,
      "productName": "Medium",
      "quantity": 2,
      "unitPrice": 320.00,
      "totalPrice": 640.00,
      "selectedToppings": [ { "toppingId": 1, "toppingName": "Extra Cheese", "price": 25.00, "quantity": 1 } ],
      "selectedAddons": [ { "addonProductId": 5, "addonName": "French Fries", "price": 60.00, "quantity": 1 } ]
    },
    "paymentStatus": 1,
    "orderStatus": 2
  }
}
```

### Accept Order
`PUT /api/v1/Seller/mine/order/{orderId}/accept?preparationTimeInMinutes=30` → `OrderSM`

### Cancel Order
`PUT /api/v1/Seller/mine/order/{orderId}/cancel` → `OrderSM`

### Seller Orders Count
`GET /api/v1/Seller/mine/orders/count` → `IntResponseRoot`

---

## FLOW SUMMARY

```
┌─────────────┐     POST /Cart/add        ┌──────────┐
│  Browse      │ ──────────────────────► │   Cart   │
│  Products    │     (with toppings/addons)│          │
└─────────────┘                           └────┬─────┘
                                               │
                    GET /Cart/mine             │ (view cart)
                  ◄────────────────────────────┘
                                               │
                    GET /Order/delivery-charges │
                  ◄────────────────────────────┘
                                               │
                    POST /Order/mine           ▼
┌─────────────┐ ◄────────────────── ┌──────────────────┐
│  Order       │   (order created)   │  Checkout Page   │
│  Created     │                     │  (build request  │
│  (status=0)  │                     │   from cart)     │
└──────┬───────┘                     └──────────────────┘
       │
       │ (if paymentMode=Online)
       │   POST /Payment/mine/checkout
       │   → Razorpay flow → webhook confirms
       │
       │ (if COD, order is ready)
       ▼
┌──────────────┐   PUT /Seller/mine/order/{id}/accept
│  Seller Gets │ ──────────────────────────────────────►  OrderStatus = SellerAccepted
│  Notification│
└──────┬───────┘
       │
       │   GET /Order/order-items/{orderId}  (seller views items with toppings/addons)
       │   GET /Order/invoice/{orderId}      (invoice with full breakdown)
       ▼
┌──────────────┐
│  Delivery    │   Delivery boy assigned, picked up, delivered
│  Flow        │
└──────────────┘
```
