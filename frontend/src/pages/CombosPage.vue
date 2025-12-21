<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'
import { getComboItems, getCombos, getProducts } from '../services/catalog'
import type { ComboDTO, ComboItemDTO, ProductDTO } from '../types/dtos'
import { useCartStore } from '../stores/cart'

const router = useRouter()
const toast = useToast()
const cart = useCartStore()
const combos = ref<ComboDTO[]>([])
const comboItems = ref<Record<number, ComboItemDTO[]>>({})
const products = ref<ProductDTO[]>([])
const productById = ref<Record<number, ProductDTO>>({})
const isLoading = ref(true)
const error = ref('')
const addingCombos = ref<Set<number>>(new Set())

const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

const loadCombos = async () => {
  isLoading.value = true
  error.value = ''
  try {
    const [comboData, productData] = await Promise.all([getCombos(), getProducts()])
    combos.value = comboData
    products.value = productData
    productById.value = Object.fromEntries(productData.map((product) => [product.ProductId, product]))
    const itemsEntries = await Promise.all(
      comboData.map(async (combo) => [combo.ComboId, await getComboItems(combo.ComboId)] as const),
    )
    comboItems.value = Object.fromEntries(itemsEntries)
  } catch (err) {
    error.value = 'Không thể tải combo. Hãy kiểm tra backend.'
  } finally {
    isLoading.value = false
  }
}

onMounted(loadCombos)

const getComboImage = (comboId: number) => {
  const combo = combos.value.find((item) => item.ComboId === comboId)
  if (combo?.ImageUrl) return combo.ImageUrl
  const items = comboItems.value[comboId]
  if (!items?.length) return ''
  const firstItem = items[0]
  return productById.value[firstItem.ProductId]?.ImageUrl ?? ''
}

const ensureComboItems = async (comboId: number) => {
  if (comboItems.value[comboId]?.length) return comboItems.value[comboId]
  const items = await getComboItems(comboId)
  comboItems.value = { ...comboItems.value, [comboId]: items }
  return items
}

const ensureProducts = async () => {
  if (products.value.length) return
  products.value = await getProducts()
  productById.value = Object.fromEntries(products.value.map((product) => [product.ProductId, product]))
}

const addComboToCart = async (combo: ComboDTO) => {
  if (!combo.IsActive) {
    toast.add({ severity: 'warn', summary: 'Combo đang tạm dừng', life: 2000 })
    return
  }
  if (addingCombos.value.has(combo.ComboId)) return
  addingCombos.value.add(combo.ComboId)
  let added = 0
  let itemTotal = 0
  try {
    await ensureProducts()
    const items = await ensureComboItems(combo.ComboId)
    if (!items?.length) {
      toast.add({ severity: 'warn', summary: 'Combo chưa có món', life: 2000 })
      return
    }
    for (const item of items) {
      const product = productById.value[item.ProductId]
      if (!product) continue
      cart.addItem(product, item.Quantity)
      itemTotal += product.Price * item.Quantity
      added += 1
    }
    const discount = Math.max(0, itemTotal - combo.Price)
    if (discount > 0) {
      cart.addComboDiscount(discount)
    }
    if (added) {
      toast.add({ severity: 'success', summary: `Đã thêm combo ${combo.Name}`, life: 2000 })
    } else {
      toast.add({ severity: 'warn', summary: 'Không thể thêm combo', life: 2000 })
    }
  } catch {
    toast.add({ severity: 'error', summary: 'Lỗi khi thêm combo', life: 2000 })
  } finally {
    addingCombos.value.delete(combo.ComboId)
  }
}
</script>

<template>
  <section class="container section">
    <div class="section-head">
      <div>
        <h2>Combo trọn vị</h2>
        <p>Chọn nhanh combo phù hợp, đủ món cho mọi khẩu vị.</p>
      </div>
      <PButton label="Làm mới" icon="pi pi-refresh" class="p-button-text" @click="loadCombos" />
    </div>

    <div v-if="isLoading" class="grid">
      <PSkeleton v-for="idx in 4" :key="idx" height="240px" border-radius="12px" />
    </div>

    <div v-else class="grid">
      <PCard v-for="combo in combos" :key="combo.ComboId" class="combo-card">
        <template #header>
          <div class="combo-image">
            <img v-if="getComboImage(combo.ComboId)" :src="getComboImage(combo.ComboId)" :alt="combo.Name" />
            <div v-else class="combo-placeholder"></div>
          </div>
        </template>
        <template #content>
          <div class="combo-body">
            <div class="combo-top">
              <h3>{{ combo.Name }}</h3>
              <span class="price">{{ currency.format(combo.Price) }}</span>
            </div>
            <p class="desc">{{ combo.Description }}</p>
            <ul class="item-list" v-if="comboItems[combo.ComboId]?.length">
              <li v-for="item in comboItems[combo.ComboId]" :key="item.ProductId">
                {{ item.Quantity }} x {{ item.ProductName }}
              </li>
            </ul>
            <div class="combo-tags">
              <PTag :value="combo.IsActive ? 'Đang bán' : 'Tạm dừng'" :severity="combo.IsActive ? 'success' : 'danger'" />
            </div>
          </div>
        </template>
        <template #footer>
          <div class="card-footer">
            <PButton
              label="Xem chi tiết"
              class="p-button-text"
              icon="pi pi-arrow-right"
              @click="router.push(`/combo/${combo.ComboId}`)"
            />
            <PButton
              label="Thêm combo"
              icon="pi pi-shopping-cart"
              class="p-button-warning"
              :loading="addingCombos.has(combo.ComboId)"
              :disabled="addingCombos.has(combo.ComboId)"
              @click="addComboToCart(combo)"
            />
          </div>
        </template>
      </PCard>
    </div>

    <div v-if="!isLoading && !combos.length" class="empty-state">
      <p>Chưa có combo nào đang mở bán.</p>
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

.grid {
  display: grid;
  gap: 28px;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
}

.combo-card {
  background: var(--surface-2);
  border: 1px solid rgba(31, 19, 11, 0.08);
  color: var(--text);
  box-shadow: var(--shadow);
}

.combo-image {
  height: 170px;
  border-top-left-radius: var(--radius);
  border-top-right-radius: var(--radius);
  overflow: hidden;
  background: rgba(255, 107, 53, 0.08);
}

.combo-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.combo-placeholder {
  height: 100%;
  background: radial-gradient(circle at 20% 20%, rgba(255, 107, 53, 0.2), transparent 55%);
}

.combo-body {
  display: grid;
  gap: 12px;
}

.combo-top {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: baseline;
}

.price {
  color: var(--accent);
  font-weight: 600;
}

.desc {
  color: var(--muted);
}

.item-list {
  list-style: none;
  display: grid;
  gap: 6px;
  font-size: 0.88rem;
  color: var(--text);
}

.combo-tags {
  display: flex;
  gap: 8px;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: center;
}

.empty-state {
  padding: 20px 0;
  color: var(--muted);
}

.error {
  color: #fca5a5;
  padding-top: 16px;
}
</style>
