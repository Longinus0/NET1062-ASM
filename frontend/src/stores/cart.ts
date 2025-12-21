import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'
import type { ProductDTO } from '../types/dtos'

export interface CartItem {
  product: ProductDTO
  quantity: number
}

export const useCartStore = defineStore('cart', () => {
  const CART_KEY = 'cart_state_v1'
  const items = ref<CartItem[]>([])
  const comboDiscount = ref(0)

  const hydrate = () => {
    const raw = localStorage.getItem(CART_KEY)
    if (!raw) return
    try {
      const data = JSON.parse(raw) as { items?: CartItem[]; comboDiscount?: number }
      if (Array.isArray(data.items)) {
        items.value = data.items
      }
      if (typeof data.comboDiscount === 'number') {
        comboDiscount.value = data.comboDiscount
      }
    } catch {
      // Ignore invalid cache
    }
  }

  hydrate()

  const addItem = (product: ProductDTO, quantity = 1) => {
    const existing = items.value.find((item) => item.product.ProductId === product.ProductId)
    if (existing) {
      existing.quantity += quantity
    } else {
      items.value.push({ product, quantity })
    }
  }

  const updateQty = (productId: number, quantity: number) => {
    const item = items.value.find((entry) => entry.product.ProductId === productId)
    if (!item) return
    if (quantity <= 0) {
      items.value = items.value.filter((entry) => entry.product.ProductId !== productId)
      return
    }
    item.quantity = quantity
  }

  const removeItem = (productId: number) => {
    items.value = items.value.filter((entry) => entry.product.ProductId !== productId)
  }

  const clearCart = () => {
    items.value = []
    comboDiscount.value = 0
  }

  const itemCount = computed(() => items.value.reduce((total, item) => total + item.quantity, 0))
  const subTotal = computed(() =>
    items.value.reduce((total, item) => total + item.product.Price * item.quantity, 0),
  )
  const deliveryFee = computed(() => (items.value.length ? 15000 : 0))
  const grandTotal = computed(() => subTotal.value + deliveryFee.value)

  watch(
    () => items.value,
    () => {
      if (!items.value.length) {
        comboDiscount.value = 0
        return
      }
      if (comboDiscount.value > subTotal.value) {
        comboDiscount.value = subTotal.value
      }
    },
    { deep: true },
  )

  watch(
    () => ({ items: items.value, comboDiscount: comboDiscount.value }),
    (value) => {
      localStorage.setItem(CART_KEY, JSON.stringify(value))
    },
    { deep: true },
  )

  return {
    items,
    itemCount,
    subTotal,
    deliveryFee,
    grandTotal,
    comboDiscount,
    addItem,
    updateQty,
    removeItem,
    clearCart,
    addComboDiscount: (amount: number) => {
      comboDiscount.value = Math.max(0, comboDiscount.value + amount)
      if (comboDiscount.value > subTotal.value) {
        comboDiscount.value = subTotal.value
      }
    },
  }
})
