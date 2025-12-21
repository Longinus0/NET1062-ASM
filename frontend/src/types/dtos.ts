export interface CategoryDTO {
  CategoryId: number
  Name: string
  Description: string
}

export interface ProductDTO {
  ProductId: number
  CategoryId: number
  Name: string
  Description: string
  Price: number
  ImageUrl: string
  TopicTag: string
  IsAvailable: number
  StockQty: number
  CreatedAt: string
  UpdatedAt: string
}

export interface ComboDTO {
  ComboId: number
  Name: string
  Description: string
  Price: number
  ImageUrl?: string | null
  IsActive: number
}

export interface ComboItemDTO {
  ProductId: number
  ProductName: string
  Quantity: number
}

export interface CreateOrderRequest {
  UserId: number
  PaymentMethod: string
  PromoCode?: string | null
  ComboDiscount?: number | null
  Note?: string | null
  Items: Array<{ ProductId: number; Quantity: number }>
}

export interface OrderSummaryDTO {
  OrderId: number
  UserId: number
  OrderCode: string
  Status: string
  SubTotal: number
  DiscountTotal: number
  DeliveryFee: number
  GrandTotal: number
  PaymentStatus: string
  PaymentMethod: string
  Note?: string | null
  CreatedAt: string
}

export interface OrderItemDTO {
  OrderItemId: number
  OrderId: number
  ProductId: number
  ProductNameSnapshot: string
  UnitPriceSnapshot: string
  Quantity: number
  LineTotal: number
}

export interface OrderDetailDTO {
  Order: OrderSummaryDTO
  Items: OrderItemDTO[]
}
