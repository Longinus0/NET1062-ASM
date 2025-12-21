<script setup lang="ts">
import { computed } from 'vue'

interface ShoppingCartItem {
  id: number | string
  name: string
  price: number
  quantity: number
  imageUrl: string
}

interface ShoppingCartProps {
  items: ShoppingCartItem[]
  shippingFee?: number
  comboDiscount?: number
}

const props = defineProps<ShoppingCartProps>()

const emit = defineEmits<{
  (e: 'quantity-change', id: ShoppingCartItem['id'], quantity: number): void
  (e: 'remove', id: ShoppingCartItem['id']): void
  (e: 'clear'): void
  (e: 'checkout'): void
  (e: 'continue'): void
}>()

const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

const subtotal = computed(() =>
  props.items.reduce((sum, item) => sum + item.price * item.quantity, 0),
)

const itemCount = computed(() =>
  props.items.reduce((sum, item) => sum + item.quantity, 0),
)

const shipping = computed(() => props.shippingFee ?? 0)
const discount = computed(() => Math.max(0, props.comboDiscount ?? 0))

const total = computed(() => Math.max(0, subtotal.value - discount.value + shipping.value))

const onInput = (id: ShoppingCartItem['id'], value: string) => {
  const next = Number.parseInt(value, 10)
  if (!Number.isNaN(next) && next >= 1) {
    emit('quantity-change', id, next)
  }
}
</script>

<template>
  <section class="cart-shell">
    <header class="cart-header">
      <div class="title">
        <i class="pi pi-shopping-cart"></i>
        <h2>Giỏ hàng của bạn</h2>
      </div>
      <div class="meta">
        <span class="count">{{ itemCount }} món</span>
        <button class="ghost" type="button" :disabled="!items.length" @click="emit('clear')">
          Xóa
        </button>
      </div>
    </header>

    <div v-if="!items.length" class="empty">
      Giỏ hàng đang trống. Hãy thêm món nhé!
    </div>

    <div v-else class="cart-body">
      <div class="item" v-for="item in items" :key="item.id">
        <img :src="item.imageUrl" :alt="item.name" />
        <div class="details">
          <h3>{{ item.name }}</h3>
          <p>{{ currency.format(item.price) }} / món</p>
          <div class="qty">
            <button
              class="qty-btn"
              type="button"
              :disabled="item.quantity <= 1"
              @click="emit('quantity-change', item.id, item.quantity - 1)"
            >
              <i class="pi pi-minus"></i>
            </button>
            <input
              type="number"
              min="1"
              :value="item.quantity"
              @input="onInput(item.id, ($event.target as HTMLInputElement).value)"
            />
            <button
              class="qty-btn"
              type="button"
              @click="emit('quantity-change', item.id, item.quantity + 1)"
            >
              <i class="pi pi-plus"></i>
            </button>
          </div>
        </div>
        <div class="price">
          <strong>{{ currency.format(item.price * item.quantity) }}</strong>
          <button class="icon danger" type="button" @click="emit('remove', item.id)">
            <i class="pi pi-trash"></i>
          </button>
        </div>
      </div>

      <div class="summary">
        <div class="row">
          <span>Tạm tính</span>
          <span>{{ currency.format(subtotal) }}</span>
        </div>
        <div class="row" v-if="discount">
          <span>Giảm giá combo</span>
          <span>-{{ currency.format(discount) }}</span>
        </div>
        <div class="row">
          <span>Phí giao hàng</span>
          <span>{{ currency.format(shipping) }}</span>
        </div>
        <div class="row total">
          <span>Tổng cộng</span>
          <span>{{ currency.format(total) }}</span>
        </div>
      </div>

      <button class="primary" type="button" :disabled="!items.length" @click="emit('checkout')">
        Thanh toán
      </button>
      <button class="link" type="button" @click="emit('continue')">Tiếp tục mua sắm</button>
    </div>
  </section>
</template>

<style scoped>
.cart-shell {
  background: var(--surface);
  border-radius: var(--radius);
  padding: 18px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  box-shadow: var(--shadow);
}

.cart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}

.title {
  display: inline-flex;
  align-items: center;
  gap: 10px;
}

.title i {
  font-size: 1.1rem;
}

.cart-header h2 {
  font-size: 1.2rem;
  font-weight: 600;
  margin: 0;
}

.meta {
  display: inline-flex;
  align-items: center;
  gap: 12px;
}

.ghost {
  border: none;
  background: transparent;
  color: var(--accent);
  font-size: 0.85rem;
  cursor: pointer;
}

.ghost:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.empty {
  text-align: center;
  color: var(--muted);
  padding: 28px 0;
}

.cart-body {
  display: grid;
  gap: 14px;
}

.item {
  display: grid;
  grid-template-columns: 86px 1fr 120px;
  gap: 12px;
  align-items: center;
  padding: 12px;
  border-radius: var(--radius);
  background: var(--surface);
  border: 1px solid rgba(31, 19, 11, 0.1);
  box-shadow: 0 8px 16px rgba(17, 12, 8, 0.08);
}

.item img {
  width: 86px;
  height: 86px;
  object-fit: cover;
  border-radius: var(--radius);
}

.details h3 {
  font-size: 0.98rem;
  margin-bottom: 4px;
}

.details p {
  color: var(--muted);
  font-size: 0.85rem;
}

.qty {
  margin-top: 10px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.qty input {
  width: 54px;
  height: 28px;
  border-radius: calc(var(--radius) - 4px);
  border: 1px solid rgba(31, 19, 11, 0.12);
  text-align: center;
  font-size: 0.85rem;
}

.qty-btn {
  width: 28px;
  height: 28px;
  border-radius: calc(var(--radius) - 4px);
  border: 1px solid rgba(31, 19, 11, 0.12);
  background: var(--surface);
  cursor: pointer;
}

.qty-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.price {
  display: grid;
  justify-items: end;
  gap: 6px;
}

.icon {
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 1rem;
}

.icon.danger {
  color: var(--accent);
}

.summary {
  border-top: 1px solid rgba(31, 19, 11, 0.08);
  padding-top: 14px;
  display: grid;
  gap: 8px;
}

.row {
  display: flex;
  justify-content: space-between;
  font-size: 0.9rem;
}

.total {
  font-weight: 700;
  font-size: 1rem;
  border-top: 1px solid rgba(31, 19, 11, 0.08);
  padding-top: 10px;
  margin-top: 4px;
}

.primary {
  margin-top: 10px;
  height: 44px;
  border-radius: var(--radius);
  border: none;
  background: var(--accent);
  color: #fff;
  font-weight: 600;
  cursor: pointer;
}

.primary:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.primary:hover {
  background: var(--accent-2);
}

.link {
  border: none;
  background: transparent;
  color: var(--accent);
  font-size: 0.9rem;
  cursor: pointer;
  padding: 6px 0 0;
}

.link:hover {
  color: var(--accent-2);
}

@media (max-width: 720px) {
  .item {
    grid-template-columns: 70px 1fr;
    grid-template-rows: auto auto;
  }

  .price {
    justify-items: start;
  }
}
</style>
