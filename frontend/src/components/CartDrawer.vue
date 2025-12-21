<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '../stores/cart'
import ShoppingCart from './ui/ShoppingCart.vue'

const props = defineProps<{ visible: boolean }>()
const emit = defineEmits<{ (e: 'update:visible', value: boolean): void }>()

const router = useRouter()
const cart = useCartStore()

const items = computed(() =>
  cart.items.map((item) => ({
    id: item.product.ProductId,
    name: item.product.Name,
    price: item.product.Price,
    quantity: item.quantity,
    imageUrl: item.product.ImageUrl,
  })),
)

const close = () => emit('update:visible', false)

const onQuantityChange = (id: number | string, quantity: number) => {
  cart.updateQty(Number(id), quantity)
}

const onRemove = (id: number | string) => {
  cart.removeItem(Number(id))
}

const onClear = () => {
  cart.clearCart()
}
</script>

<template>
  <PSidebar
    :visible="props.visible"
    position="right"
    class="drawer"
    :style="{ width: 'min(620px, 96vw)' }"
    @update:visible="emit('update:visible', $event)"
  >
    <ShoppingCart
      :items="items"
      :shipping-fee="cart.deliveryFee"
      :combo-discount="cart.comboDiscount"
      @quantity-change="onQuantityChange"
      @remove="onRemove"
      @clear="onClear"
      @checkout="router.push('/checkout'); close()"
      @continue="router.push('/menu'); close()"
    />
  </PSidebar>
</template>

<style scoped>
:global(.p-sidebar.drawer) {
  width: min(620px, 96vw) !important;
}

:global(.p-sidebar.drawer .p-sidebar-content) {
  padding: 18px !important;
  box-sizing: border-box;
}

@media (max-width: 720px) {
  :global(.p-sidebar.drawer) {
    width: min(94vw, 520px) !important;
  }
}
</style>
