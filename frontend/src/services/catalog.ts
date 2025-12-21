import api from './api'
import type { CategoryDTO, ComboDTO, ComboItemDTO, ProductDTO } from '../types/dtos'

export const getCategories = async () => {
  const { data } = await api.get<CategoryDTO[]>('/categories')
  return data
}

export const getProducts = async (params?: Record<string, string | number | undefined>) => {
  const { data } = await api.get<ProductDTO[]>('/products', { params })
  return data
}

export const getProductById = async (id: number) => {
  const { data } = await api.get<ProductDTO>(`/products/${id}`)
  return data
}

export const getCombos = async () => {
  const { data } = await api.get<ComboDTO[]>('/combos')
  return data
}

export const getComboById = async (id: number) => {
  const { data } = await api.get<ComboDTO>(`/combos/${id}`)
  return data
}

export const getComboItems = async (id: number) => {
  const { data } = await api.get<ComboItemDTO[]>(`/combos/${id}/items`)
  return data
}
