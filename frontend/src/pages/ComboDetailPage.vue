<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getComboById, getComboItems, getProducts } from '../services/catalog'
import type { ComboDTO, ComboItemDTO, ProductDTO } from '../types/dtos'

const route = useRoute()
const router = useRouter()

const combo = ref<ComboDTO | null>(null)
const items = ref<ComboItemDTO[]>([])
const products = ref<ProductDTO[]>([])
const isLoading = ref(true)
const error = ref('')

const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

const comboValue = computed(() => {
  return items.value.reduce((total, item) => {
    const price = products.value.find((p) => p.ProductId === item.ProductId)?.Price ?? 0
    return total + price * item.Quantity
  }, 0)
})

const loadData = async () => {
  isLoading.value = true
  error.value = ''
  try {
    const comboId = Number(route.params.id)
    const [comboData, itemData, productData] = await Promise.all([
      getComboById(comboId),
      getComboItems(comboId),
      getProducts(),
    ])
    combo.value = comboData
    items.value = itemData
    products.value = productData
  } catch (err) {
    error.value = 'Không tìm thấy combo.'
  } finally {
    isLoading.value = false
  }
}

onMounted(loadData)
</script>

<template>
  <section class="container detail">
    <PButton icon="pi pi-arrow-left" class="p-button-text" label="Quay lại" @click="router.back()" />

    <div v-if="isLoading" class="detail-card">
      <PSkeleton height="32px" width="60%" />
      <PSkeleton height="18px" width="40%" />
      <PSkeleton height="120px" />
      <PSkeleton height="42px" width="50%" />
    </div>

    <div v-else-if="combo" class="detail-card">
      <div class="header">
        <div>
          <h2>{{ combo.Name }}</h2>
          <p class="desc">{{ combo.Description }}</p>
        </div>
        <div class="price-box">
          <span class="price">{{ currency.format(combo.Price) }}</span>
          <PTag :value="combo.IsActive ? 'Đang bán' : 'Tạm dừng'" :severity="combo.IsActive ? 'success' : 'danger'" />
        </div>
      </div>

      <div class="value">
        <p>Giá trị combo theo từng món</p>
        <h3>{{ currency.format(comboValue) }}</h3>
      </div>

      <div class="list">
        <h4>Món trong combo</h4>
        <ul>
          <li v-for="item in items" :key="item.ProductId">
            <span>{{ item.ProductName }}</span>
            <span>x{{ item.Quantity }}</span>
          </li>
          <li v-if="!items.length">Chưa có món trong combo.</li>
        </ul>
      </div>

      <div class="cta">
        <PButton label="Thêm combo" icon="pi pi-shopping-cart" class="p-button-warning" />
        <PButton label="Đặt ngay" class="p-button-outlined" />
      </div>
    </div>

    <div v-else class="error">{{ error }}</div>
  </section>
</template>

<style scoped>
.detail {
  padding: 24px 0 40px;
}

.detail-card {
  margin-top: 16px;
  background: var(--surface-2);
  border-radius: var(--radius);
  padding: 24px;
  box-shadow: var(--shadow);
  display: grid;
  gap: 18px;
}

.header {
  display: flex;
  justify-content: space-between;
  gap: 20px;
  flex-wrap: wrap;
}

h2 {
  font-family: 'Fraunces', serif;
  font-size: clamp(1.8rem, 3vw, 2.4rem);
}

.desc {
  color: var(--muted);
  margin-top: 8px;
}

.price-box {
  text-align: right;
}

.price {
  color: var(--accent-2);
  font-weight: 600;
  font-size: 1.4rem;
}

.value {
  background: rgba(255, 107, 53, 0.12);
  border-radius: var(--radius);
  padding: 16px;
}

.value h3 {
  margin-top: 6px;
  color: var(--accent);
}

.list ul {
  list-style: none;
  display: grid;
  gap: 10px;
  color: var(--muted);
}

.list li {
  display: flex;
  justify-content: space-between;
  gap: 12px;
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
