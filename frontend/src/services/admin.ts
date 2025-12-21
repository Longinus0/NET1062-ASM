import api from './api'
import type { CategoryDTO, ComboDTO, ProductDTO } from '../types/dtos'

export interface AdminUser {
  UserId: number
  FullName: string
  Email: string
  Phone?: string | null
  Address?: string | null
  AvatarUrl?: string | null
  IsActive: number
  ForcePasswordReset?: number
  CreatedAt: string
  UpdatedAt: string
  RoleId?: number | null
  RoleName?: string | null
}

export interface AdminOrderSummary {
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

export interface AdminRevenuePoint {
  Date: string
  Total: number
}

export interface AdminRevenueSummary {
  Total: number
  Days: number
  Series: AdminRevenuePoint[]
}

export interface AdminAuditLog {
  AuditLogId: number
  ActorUserId: number
  ActorName?: string | null
  ActorEmail?: string | null
  Action: string
  EntityName: string
  EntityId: number
  OldValuesJson: string
  NewValuesJson: string
  CreatedAt: string
  IpAddress: string
}

export interface AdminRole {
  RoleId: number
  Name: string
  UserCount: number
  Users: { FullName: string; Email: string }[]
}

const withAdminHeader = (adminUserId?: number) => ({
  headers: adminUserId ? { 'X-Admin-UserId': adminUserId } : {},
})

export const getAdminUsers = async (adminUserId?: number) => {
  const { data } = await api.get<AdminUser[]>('/admin/users', withAdminHeader(adminUserId))
  return data
}

export const createAdminUser = async (
  payload: {
    FullName: string
    Email: string
    Password: string
    Phone?: string | null
    Address?: string | null
    AvatarUrl?: string | null
    IsActive: number
    ForcePasswordReset?: number
    RoleId: number
  },
  adminUserId?: number,
) => {
  const { data } = await api.post('/admin/users', payload, withAdminHeader(adminUserId))
  return data
}

export const updateAdminUser = async (
  userId: number,
  payload: {
    FullName: string
    Phone?: string | null
    Address?: string | null
    AvatarUrl?: string | null
    IsActive?: number
    ForcePasswordReset?: number
  },
  adminUserId?: number,
) => {
  await api.put(`/admin/users/${userId}`, payload, withAdminHeader(adminUserId))
}

export const updateAdminUserRole = async (userId: number, roleId: number, adminUserId?: number) => {
  await api.put(`/admin/users/${userId}/role`, { RoleId: roleId }, withAdminHeader(adminUserId))
}

export const deleteAdminUser = async (userId: number, adminUserId?: number) => {
  await api.delete(`/admin/users/${userId}`, withAdminHeader(adminUserId))
}

export const getCategories = async () => {
  const { data } = await api.get<CategoryDTO[]>('/categories')
  return data
}

export const getProducts = async () => {
  const { data } = await api.get<ProductDTO[]>('/products')
  return data
}

export const createProduct = async (
  payload: Omit<ProductDTO, 'ProductId' | 'CreatedAt' | 'UpdatedAt'>,
  adminUserId?: number,
) => {
  const { data } = await api.post('/admin/products', payload, withAdminHeader(adminUserId))
  return data
}

export const updateProduct = async (
  productId: number,
  payload: Omit<ProductDTO, 'ProductId' | 'CreatedAt' | 'UpdatedAt'>,
  adminUserId?: number,
) => {
  await api.put(`/admin/products/${productId}`, payload, withAdminHeader(adminUserId))
}

export const deleteProduct = async (productId: number, adminUserId?: number) => {
  await api.delete(`/admin/products/${productId}`, withAdminHeader(adminUserId))
}

export const getCombos = async () => {
  const { data } = await api.get<ComboDTO[]>('/combos')
  return data
}

export const createCombo = async (
  payload: { Name: string; Description: string; Price: number; ImageUrl?: string | null; IsActive: number },
  adminUserId?: number,
) => {
  const { data } = await api.post('/admin/combos', payload, withAdminHeader(adminUserId))
  return data
}

export const updateCombo = async (
  comboId: number,
  payload: { Name: string; Description: string; Price: number; ImageUrl?: string | null; IsActive: number },
  adminUserId?: number,
) => {
  await api.put(`/admin/combos/${comboId}`, payload, withAdminHeader(adminUserId))
}

export const deleteCombo = async (comboId: number, adminUserId?: number) => {
  await api.delete(`/admin/combos/${comboId}`, withAdminHeader(adminUserId))
}

export const getOrders = async (status?: string, adminUserId?: number) => {
  const { data } = await api.get<AdminOrderSummary[]>('/admin/orders', {
    ...withAdminHeader(adminUserId),
    params: status ? { status } : {},
  })
  return data
}

export const updateOrderStatus = async (
  orderId: number,
  payload: { Status: string; ChangedByUserId?: number; Note?: string | null },
) => {
  await api.put(`/orders/${orderId}/status`, payload)
}

export const getRevenueSummary = async (days: number, adminUserId?: number) => {
  const { data } = await api.get<AdminRevenueSummary>('/admin/revenue', {
    ...withAdminHeader(adminUserId),
    params: { days },
  })
  return data
}

export const getAuditLogs = async (limit = 200, adminUserId?: number) => {
  const { data } = await api.get<AdminAuditLog[]>('/admin/audit-logs', {
    ...withAdminHeader(adminUserId),
    params: { limit },
  })
  return data
}

export const getRoles = async (adminUserId?: number) => {
  const { data } = await api.get<AdminRole[]>('/admin/roles', withAdminHeader(adminUserId))
  return data
}
