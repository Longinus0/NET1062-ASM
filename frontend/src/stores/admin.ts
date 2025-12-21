import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { CategoryDTO, ComboDTO, ProductDTO } from '../types/dtos'
import type { AdminOrderSummary, AdminRole, AdminUser } from '../services/admin'
import {
  createAdminUser,
  createCombo,
  createProduct,
  deleteAdminUser,
  deleteCombo,
  deleteProduct,
  getAdminUsers,
  getCategories,
  getCombos,
  getOrders,
  getProducts,
  getRoles,
  getRevenueSummary,
  updateAdminUserRole,
  updateOrderStatus,
  updateAdminUser,
  updateCombo,
  updateProduct,
} from '../services/admin'
import { useAuthStore } from './auth'
import { getOrderById } from '../services/orders'
import type { OrderDetailDTO } from '../types/dtos'

export const useAdminStore = defineStore('admin', () => {
  const users = ref<AdminUser[]>([])
  const products = ref<ProductDTO[]>([])
  const categories = ref<CategoryDTO[]>([])
  const combos = ref<ComboDTO[]>([])
  const orders = ref<AdminOrderSummary[]>([])
  const isLoading = ref(false)
  const revenueTotal = ref(0)
  const revenueSeries = ref<{ Date: string; Total: number }[]>([])
  const revenueDays = ref(30)
  const roles = ref<AdminRole[]>([])

  const auth = useAuthStore()

  const adminUserId = computed(() => auth.user?.UserId)

  const loadDashboard = async () => {
    isLoading.value = true
    try {
      const [userData, productData, orderData] = await Promise.all([
        getAdminUsers(adminUserId.value),
        getProducts(),
        getOrders(undefined, adminUserId.value),
      ])
      users.value = userData
      products.value = productData
      orders.value = orderData
    } finally {
      isLoading.value = false
    }
  }

  const loadRevenue = async (days = 30) => {
    revenueDays.value = days
    const data = await getRevenueSummary(days, adminUserId.value)
    revenueTotal.value = data.Total
    revenueSeries.value = data.Series
  }

  const loadUsers = async () => {
    users.value = await getAdminUsers(adminUserId.value)
  }

  const addUser = async (payload: Parameters<typeof createAdminUser>[0]) => {
    await createAdminUser(payload, adminUserId.value)
    await loadUsers()
  }

  const updateUser = async (userId: number, payload: Parameters<typeof updateAdminUser>[1]) => {
    await updateAdminUser(userId, payload, adminUserId.value)
    await loadUsers()
  }

  const removeUser = async (userId: number) => {
    await deleteAdminUser(userId, adminUserId.value)
    await loadUsers()
  }

  const setUserRole = async (userId: number, roleId: number) => {
    await updateAdminUserRole(userId, roleId, adminUserId.value)
    await loadUsers()
  }

  const loadCatalog = async () => {
    const [categoryData, productData] = await Promise.all([getCategories(), getProducts()])
    categories.value = categoryData
    products.value = productData
  }

  const addProduct = async (payload: Parameters<typeof createProduct>[0]) => {
    await createProduct(payload, adminUserId.value)
    await loadCatalog()
  }

  const updateProductItem = async (productId: number, payload: Parameters<typeof updateProduct>[1]) => {
    await updateProduct(productId, payload, adminUserId.value)
    await loadCatalog()
  }

  const removeProduct = async (productId: number) => {
    await deleteProduct(productId, adminUserId.value)
    await loadCatalog()
  }

  const loadCombos = async () => {
    combos.value = await getCombos()
  }

  const addCombo = async (payload: Parameters<typeof createCombo>[0]) => {
    await createCombo(payload, adminUserId.value)
    await loadCombos()
  }

  const updateComboItem = async (comboId: number, payload: Parameters<typeof updateCombo>[1]) => {
    await updateCombo(comboId, payload, adminUserId.value)
    await loadCombos()
  }

  const removeCombo = async (comboId: number) => {
    await deleteCombo(comboId, adminUserId.value)
    await loadCombos()
  }

  const loadOrders = async (status?: string) => {
    orders.value = await getOrders(status, adminUserId.value)
  }

  const loadRoles = async () => {
    roles.value = await getRoles(adminUserId.value)
  }

  const getOrderDetail = async (orderId: number) => {
    return await getOrderById(orderId)
  }

  const setOrderStatus = async (orderId: number, status: string, note?: string) => {
    await updateOrderStatus(orderId, { Status: status, ChangedByUserId: adminUserId.value, Note: note || null })
    await loadOrders()
  }

  return {
    users,
    products,
    categories,
    combos,
    orders,
    revenueTotal,
    revenueSeries,
    revenueDays,
    roles,
    isLoading,
    loadDashboard,
    loadRevenue,
    loadUsers,
    addUser,
    updateUser,
    removeUser,
    setUserRole,
    loadCatalog,
    addProduct,
    updateProductItem,
    removeProduct,
    loadCombos,
    addCombo,
    updateComboItem,
    removeCombo,
    loadOrders,
    loadRoles,
    getOrderDetail,
    setOrderStatus,
  }
})
