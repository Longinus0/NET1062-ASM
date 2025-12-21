<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getProductById } from '../services/catalog'
import type { ProductDTO } from '../types/dtos'

const route = useRoute()
const router = useRouter()
const product = ref<ProductDTO | null>(null)
const isLoading = ref(true)
const error = ref('')

const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })
const fallbackImage = 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=900&q=80'

const tagSeverity = computed(() => (product.value?.IsAvailable ? 'success' : 'danger'))

const onImageError = (event: Event) => {
  const target = event.target as HTMLImageElement
  target.src = fallbackImage
}

const loadProduct = async () => {
  isLoading.value = true
  error.value = ''
  try {
    product.value = await getProductById(Number(route.params.id))
  } catch (err) {
    error.value = 'Không tìm thấy món ăn.'
  } finally {
    isLoading.value = false
  }
}

onMounted(loadProduct)
</script>

<template>
  <section class="container detail">
    <PButton icon="pi pi-arrow-left" class="p-button-text" label="Quay lại" @click="router.back()" />

    <div v-if="isLoading" class="detail-grid">
      <PSkeleton height="320px" border-radius="12px" />
      <div class="detail-panel">
        <PSkeleton height="32px" width="70%" />
        <PSkeleton height="18px" width="40%" />
        <PSkeleton height="120px" />
        <PSkeleton height="48px" width="60%" />
      </div>
    </div>

    <div v-else-if="product" class="detail-grid">
      <div class="image-box">
        <img v-if="product.ImageUrl" :src="product.ImageUrl" :alt="product.Name" @error="onImageError" />
        <img v-else :src="fallbackImage" :alt="product.Name" />
      </div>
      <div class="detail-panel">
        <h2>{{ product.Name }}</h2>
        <span class="price">{{ currency.format(product.Price) }}</span>
        <p class="desc">{{ product.Description }}</p>
        <div class="tags">
          <PTag :value="product.TopicTag" severity="warning" />
          <PTag :value="product.IsAvailable ? 'Còn hàng' : 'Tạm hết'" :severity="tagSeverity" />
        </div>
        <div class="cta">
          <PButton label="Thêm vào giỏ" icon="pi pi-shopping-cart" class="p-button-warning" />
          <PButton label="Đặt ngay" class="p-button-outlined" />
        </div>
      </div>
    </div>

    <div v-else class="error">{{ error }}</div>
  </section>
</template>

<style scoped>
.detail {
  padding: 24px 0 40px;
}

.detail-grid {
  display: grid;
  gap: 24px;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  margin-top: 16px;
}

.image-box {
  border-radius: var(--radius);
  overflow: hidden;
  background: linear-gradient(140deg, #ff8f3f, #ff5f1f);
  box-shadow: var(--shadow);
}

.image-box img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  min-height: 320px;
}

.image-fallback {
  min-height: 320px;
}

.detail-panel {
  background: var(--surface);
  border-radius: var(--radius);
  padding: 24px;
  box-shadow: var(--shadow);
  display: grid;
  gap: 16px;
}

h2 {
  font-family: 'Fraunces', serif;
  font-size: clamp(1.8rem, 3vw, 2.6rem);
}

.price {
  color: var(--accent-2);
  font-size: 1.4rem;
  font-weight: 600;
}

.desc {
  color: var(--muted);
}

.tags {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.cta {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.error {
  color: #fca5a5;
  padding: 24px 0;
}
</style>
