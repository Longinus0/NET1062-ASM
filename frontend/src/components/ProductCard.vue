<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useCartStore } from '../stores/cart'
import type { ProductDTO } from '../types/dtos'

const props = defineProps<{
  product: ProductDTO
}>()

const router = useRouter()
const cart = useCartStore()
const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })
const fallbackImage = 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=900&q=80'

const onImageError = (event: Event) => {
  const target = event.target as HTMLImageElement
  target.src = fallbackImage
}
</script>

<template>
  <PCard class="product-card">
    <template #header>
      <div class="media">
        <img
          v-if="props.product.ImageUrl"
          :src="props.product.ImageUrl"
          :alt="props.product.Name"
          @error="onImageError"
        />
        <img v-else :src="fallbackImage" :alt="props.product.Name" />
      </div>
    </template>
    <template #content>
      <div class="card-body">
        <div class="card-top">
          <h3>{{ props.product.Name }}</h3>
          <span class="price">{{ currency.format(props.product.Price) }}</span>
        </div>
        <p class="desc">{{ props.product.Description }}</p>
        <div class="tags">
          <PTag :value="props.product.TopicTag" severity="warning" />
          <PTag
            :value="props.product.IsAvailable ? 'Còn hàng' : 'Tạm hết'"
            :severity="props.product.IsAvailable ? 'success' : 'danger'"
          />
        </div>
      </div>
    </template>
    <template #footer>
      <div class="card-footer">
        <PButton
          label="Chi tiết"
          class="p-button-text detail-button"
          icon="pi pi-arrow-right"
          icon-pos="right"
          @click="router.push(`/product/${props.product.ProductId}`)"
        />
        <PButton
          label="Thêm vào giỏ"
          icon="pi pi-shopping-cart"
          class="p-button-warning"
          @click="cart.addItem(props.product, 1)"
        />
      </div>
    </template>
  </PCard>
</template>

<style scoped>
.product-card {
  background: var(--surface);
  border: 1px solid rgba(31, 19, 11, 0.08);
  color: var(--text);
  box-shadow: var(--shadow);
  overflow: hidden;
  height: 100%;
}

.media {
  height: 180px;
  background: linear-gradient(140deg, #ffe1c8, #ffb387);
}

.media img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.media-fallback {
  width: 100%;
  height: 100%;
  background: radial-gradient(circle at top, rgba(255, 209, 102, 0.4), transparent 60%);
}

.card-body {
  display: grid;
  gap: 12px;
  min-height: 190px;
}

.card-top {
  display: flex;
  justify-content: space-between;
  gap: 16px;
}

.card-top h3 {
  font-size: 1.05rem;
}

.price {
  color: var(--accent);
  font-weight: 600;
}

.desc {
  color: var(--muted);
  min-height: 44px;
  line-height: 1.5;
}

.tags {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  align-items: center;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: center;
}

.card-footer :deep(.p-button) {
  padding: 0.45rem 0.8rem;
  font-size: 0.82rem;
  border-radius: var(--radius);
  height: 34px;
}

.card-footer :deep(.p-button-text) {
  padding: 0.4rem 0.6rem;
}

.card-footer :deep(.detail-button) {
  border: 1px solid rgba(155, 109, 74, 0.4);
  color: #9b6d4a;
  background: transparent;
  transition: background 0.2s ease, border-color 0.2s ease, color 0.2s ease;
}

.card-footer :deep(.detail-button:hover) {
  background: rgba(255, 226, 200, 0.6);
  border-color: rgba(155, 109, 74, 0.6);
  color: #7d5133;
}

.card-footer :deep(.p-button-label) {
  font-size: 0.82rem;
}
</style>
