<script setup lang="ts">
import { computed } from 'vue'
import { useCartStore } from '../stores/cart'
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import ShoppingCart from '../components/ui/ShoppingCart.vue'

const cart = useCartStore()
const router = useRouter()
const toast = useToast()

const items = computed(() =>
  cart.items.map((item) => ({
    id: item.product.ProductId,
    name: item.product.Name,
    price: item.product.Price,
    quantity: item.quantity,
    imageUrl: item.product.ImageUrl,
  })),
)

const onClear = () => {
  cart.clearCart()
  toast.add({ severity: 'info', summary: 'Giỏ hàng đã được làm trống', life: 2000 })
}

const onQuantityChange = (id: number | string, quantity: number) => {
  cart.updateQty(Number(id), quantity)
}

const onRemove = (id: number | string) => {
  cart.removeItem(Number(id))
}
</script>

<template>
  <section class="container cart">
    <ShoppingCart
      :items="items"
      :shipping-fee="cart.deliveryFee"
      :combo-discount="cart.comboDiscount"
      @quantity-change="onQuantityChange"
      @remove="onRemove"
      @clear="onClear"
      @checkout="router.push('/checkout')"
      @continue="router.push('/menu')"
    />
  </section>
</template>

<style scoped>
.cart {
  padding: 28px 0 48px;
}
</style>
