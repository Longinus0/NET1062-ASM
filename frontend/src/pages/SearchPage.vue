<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ProductCard from '../components/ProductCard.vue'
import { getCategories, getProducts } from '../services/catalog'
import type { CategoryDTO, ProductDTO } from '../types/dtos'

const route = useRoute()
const router = useRouter()

const categories = ref<CategoryDTO[]>([])
const results = ref<ProductDTO[]>([])
const isLoading = ref(true)
const error = ref('')

const name = ref('')
const minPrice = ref('')
const maxPrice = ref('')
const categoryId = ref<number | null>(null)
const topicTag = ref<string | null>(null)
const tagOptions = ref<string[]>([])

const queryParams = computed(() => ({
  name: name.value || undefined,
  minPrice: minPrice.value || undefined,
  maxPrice: maxPrice.value || undefined,
  categoryId: categoryId.value?.toString() || undefined,
  topicTag: topicTag.value || undefined,
}))

const syncFromQuery = () => {
  name.value = typeof route.query.name === 'string' ? route.query.name : ''
  minPrice.value = typeof route.query.minPrice === 'string' ? route.query.minPrice : ''
  maxPrice.value = typeof route.query.maxPrice === 'string' ? route.query.maxPrice : ''
  categoryId.value = typeof route.query.categoryId === 'string' ? Number(route.query.categoryId) : null
  topicTag.value = typeof route.query.topicTag === 'string' ? route.query.topicTag : null
}

const searchProducts = async () => {
  isLoading.value = true
  error.value = ''
  try {
    results.value = await getProducts({
      search: name.value || undefined,
      minPrice: minPrice.value || undefined,
      maxPrice: maxPrice.value || undefined,
      categoryId: categoryId.value || undefined,
      topicTag: topicTag.value || undefined,
    })
  } catch (err) {
    error.value = 'Không thể tìm kiếm món ăn.'
  } finally {
    isLoading.value = false
  }
}

const applyFilters = async () => {
  await router.replace({ query: { ...queryParams.value } })
}

watch(
  () => route.query,
  () => {
    syncFromQuery()
    searchProducts()
  },
  { immediate: true },
)

onMounted(async () => {
  try {
    const [categoryData, productData] = await Promise.all([getCategories(), getProducts()])
    categories.value = categoryData
    tagOptions.value = Array.from(
      new Set(productData.map((item) => item.TopicTag).filter(Boolean)),
    ).sort((a, b) => a.localeCompare(b))
  } catch (err) {
    error.value = 'Không thể tải danh mục hoặc tag.'
  }
})
</script>

<template>
  <section class="container section">
    <div class="section-head">
      <div>
        <h2>Tìm kiếm nâng cao</h2>
        <p>Lọc theo tên, giá, danh mục và tag.</p>
      </div>
      <PButton label="Áp dụng" icon="pi pi-filter" class="p-button-text" @click="applyFilters" />
    </div>

    <div class="filters">
      <div class="search-field">
        <span class="search-icon" aria-hidden="true">
          <i class="pi pi-search"></i>
        </span>
        <PInputText v-model="name" placeholder="Tên món ăn" @keyup.enter="applyFilters" />
      </div>
      <PInputText v-model="minPrice" placeholder="Giá từ (VND)" type="number" />
      <PInputText v-model="maxPrice" placeholder="Giá đến (VND)" type="number" />
      <PDropdown
        v-model="categoryId"
        :options="categories"
        option-label="Name"
        option-value="CategoryId"
        placeholder="Danh mục"
      />
      <PDropdown v-model="topicTag" :options="tagOptions" placeholder="Tag" filter show-clear />
    </div>

    <div v-if="isLoading" class="grid">
      <PSkeleton v-for="idx in 6" :key="idx" height="280px" border-radius="12px" />
    </div>

    <div v-else class="grid">
      <ProductCard v-for="item in results" :key="item.ProductId" :product="item" />
    </div>

    <div v-if="!results.length && !isLoading" class="empty">
      <p>Không có kết quả phù hợp. Hãy thử điều chỉnh bộ lọc.</p>
    </div>

    <div v-if="error" class="error">{{ error }}</div>
  </section>
</template>

<style scoped>
.section {
  padding: 24px 0 40px;
}

.section-head {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 16px;
  margin-bottom: 20px;
}

.section-head h2 {
  font-family: 'Fraunces', serif;
  font-size: clamp(1.8rem, 3vw, 2.4rem);
}

.section-head p {
  color: var(--muted);
}

.filters {
  display: grid;
  gap: 10px;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  margin-bottom: 20px;
  padding: 10px 12px;
  background: var(--surface-2);
  border-radius: var(--radius);
  border: 1px solid rgba(31, 19, 11, 0.08);
  box-shadow: 0 6px 14px rgba(31, 19, 11, 0.08);
  align-items: center;
}

.filters > * {
  width: 100%;
}

.filters > .p-input-icon-left {
  display: flex;
  align-items: center;
  width: 100%;
}

.search-field {
  display: flex;
  align-items: stretch;
  gap: 10px;
  width: 100%;
  height: 44px;
}

.search-icon {
  display: grid;
  place-items: center;
  width: 44px;
  height: 100%;
  border-radius: var(--radius);
  border: 1px solid rgba(31, 19, 11, 0.12);
  background: #fff7f0;
  color: #9b6d4a;
  flex-shrink: 0;
}

.search-field :deep(.p-inputtext) {
  height: 100%;
}

.filters :deep(.p-inputtext),
.filters :deep(.p-dropdown) {
  height: 44px;
  width: 100%;
}

.filters :deep(.p-dropdown .p-dropdown-clear-icon) {
  top: 50%;
  transform: translateY(-50%);
  right: 2.8rem;
}

.grid {
  margin-top: 10px;
}

.grid {
  display: grid;
  gap: 28px;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
}

.empty {
  padding: 20px 0;
  color: var(--muted);
}

.error {
  color: #fca5a5;
  padding-top: 12px;
}
</style>
