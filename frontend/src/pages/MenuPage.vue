<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import ProductCard from '../components/ProductCard.vue'
import { getCategories, getProducts } from '../services/catalog'
import type { CategoryDTO, ProductDTO } from '../types/dtos'

const categories = ref<CategoryDTO[]>([])
const products = ref<ProductDTO[]>([])
const isLoading = ref(true)
const error = ref('')

const search = ref('')
const selectedCategory = ref<number | null>(null)
const sortOption = ref('newest')
const route = useRoute()

const page = ref(0)
const rows = ref(8)

const sortOptions = [
  { label: 'Mới nhất', value: 'newest' },
  { label: 'Giá thấp → cao', value: 'price-asc' },
  { label: 'Giá cao → thấp', value: 'price-desc' },
]

const filteredProducts = computed(() => {
  let list = [...products.value]
  if (search.value.trim()) {
    const query = search.value.toLowerCase()
    list = list.filter((item) => item.Name.toLowerCase().includes(query))
  }
  if (selectedCategory.value) {
    list = list.filter((item) => item.CategoryId === selectedCategory.value)
  }
  if (sortOption.value === 'price-asc') {
    list.sort((a, b) => a.Price - b.Price)
  } else if (sortOption.value === 'price-desc') {
    list.sort((a, b) => b.Price - a.Price)
  } else {
    list.sort((a, b) => new Date(b.CreatedAt).getTime() - new Date(a.CreatedAt).getTime())
  }
  return list
})

const pagedProducts = computed(() => {
  const start = page.value * rows.value
  return filteredProducts.value.slice(start, start + rows.value)
})

const totalRecords = computed(() => filteredProducts.value.length)

const onPageChange = (event: { page: number }) => {
  page.value = event.page
}

const loadData = async () => {
  isLoading.value = true
  error.value = ''
  try {
    const [categoryData, productData] = await Promise.all([getCategories(), getProducts()])
    categories.value = categoryData
    products.value = productData
  } catch (err) {
    error.value = 'Không thể tải dữ liệu menu. Hãy kiểm tra backend.'
  } finally {
    isLoading.value = false
  }
}

watch([search, selectedCategory, sortOption], () => {
  page.value = 0
})

watch(
  () => route.query.category,
  (value) => {
    if (!value) {
      selectedCategory.value = null
      return
    }
    const parsed = Number(value)
    selectedCategory.value = Number.isNaN(parsed) ? null : parsed
  },
  { immediate: true },
)

onMounted(loadData)
</script>

<template>
  <section class="container section">
    <div class="section-head">
      <div>
        <h2>Menu hôm nay</h2>
        <p>Chọn món nhanh, lọc thông minh, chuẩn vị Việt.</p>
      </div>
      <PButton label="Làm mới" icon="pi pi-refresh" class="p-button-text" @click="loadData" />
    </div>

    <div class="filters">
      <div class="search-field">
        <span class="search-icon" aria-hidden="true">
          <i class="pi pi-search"></i>
        </span>
        <PInputText v-model="search" placeholder="Tìm theo tên món..." />
      </div>
      <PDropdown
        v-model="selectedCategory"
        :options="categories"
        option-label="Name"
        option-value="CategoryId"
        placeholder="Danh mục"
        class="filter-dropdown"
      />
      <PDropdown v-model="sortOption" :options="sortOptions" option-label="label" option-value="value" />
    </div>

    <div v-if="isLoading" class="grid">
      <PSkeleton v-for="idx in 6" :key="idx" height="280px" border-radius="12px" />
    </div>

    <div v-else>
      <div class="grid">
        <ProductCard v-for="product in pagedProducts" :key="product.ProductId" :product="product" />
      </div>
      <div v-if="!pagedProducts.length" class="empty-state">
        <p>Chưa có món phù hợp. Hãy thử lại với bộ lọc khác.</p>
      </div>
    </div>

    <PPaginator
      v-if="totalRecords > rows"
      :rows="rows"
      :total-records="totalRecords"
      :first="page * rows"
      class="paginator"
      @page="onPageChange"
    />
  </section>

  <div v-if="error" class="container error">{{ error }}</div>
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

.grid {
  margin-top: 10px;
}

.filter-dropdown {
  width: 100%;
}

.grid {
  display: grid;
  gap: 28px;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
}

.paginator {
  margin-top: 24px;
}

.empty-state {
  padding: 20px 0;
  color: var(--muted);
}

.error {
  color: #fca5a5;
  padding-bottom: 32px;
}
</style>
