import api from './api'
import type { CreateOrderRequest, OrderDetailDTO, OrderSummaryDTO } from '../types/dtos'

export const createOrder = async (payload: CreateOrderRequest, idempotencyKey?: string) => {
  const { data } = await api.post<{ OrderId: number; OrderCode: string }>('/orders', payload, {
    headers: idempotencyKey ? { 'Idempotency-Key': idempotencyKey } : {},
  })
  return data
}

export const getOrdersByUser = async (userId: number) => {
  const { data } = await api.get<OrderSummaryDTO[]>(`/users/${userId}/orders`)
  return data
}

export const getOrderById = async (orderId: number) => {
  const { data } = await api.get<OrderDetailDTO>(`/orders/${orderId}`)
  return data
}

export const getOrderHistory = async (orderId: number) => {
  const { data } = await api.get<Array<{ FromStatus: string; ToStatus: string; ChangedAt: string; Note?: string | null }>>(
    `/orders/${orderId}/history`,
  )
  return data
}
