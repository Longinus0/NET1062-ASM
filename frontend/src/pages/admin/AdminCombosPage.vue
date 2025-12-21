<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAdminStore } from '../../stores/admin'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'

const admin = useAdminStore()
const toast = useToast()
const confirm = useConfirm()

const search = ref('')
const showDialog = ref(false)
const isEdit = ref(false)
const form = ref({
  ComboId: 0,
  Name: '',
  Description: '',
  Price: 0,
  ImageUrl: '',
  IsActive: 1,
})
const selectedProducts = ref<number[]>([])
const quantities = ref<Record<number, number>>({})

const filteredCombos = computed(() => {
  if (!search.value.trim()) return admin.combos
  const q = search.value.toLowerCase()
  return admin.combos.filter((c) => c.Name.toLowerCase().includes(q))
})

const openCreate = () => {
  isEdit.value = false
  form.value = { ComboId: 0, Name: '', Description: '', Price: 0, ImageUrl: '', IsActive: 1 }
  selectedProducts.value = []
  quantities.value = {}
  showDialog.value = true
}

const openEdit = (row: any) => {
  isEdit.value = true
  form.value = { ...row, ImageUrl: row.ImageUrl || '' }
  selectedProducts.value = []
  quantities.value = {}
  showDialog.value = true
}

const saveCombo = async () => {
  try {
    const payload = {
      Name: form.value.Name,
      Description: form.value.Description,
      Price: form.value.Price,
      ImageUrl: form.value.ImageUrl || null,
      IsActive: form.value.IsActive,
    }
    if (isEdit.value) {
      await admin.updateComboItem(form.value.ComboId, payload)
      toast.add({ severity: 'success', summary: 'Đã cập nhật combo', life: 2000 })
    } else {
      await admin.addCombo(payload)
      toast.add({ severity: 'success', summary: 'Đã tạo combo', life: 2000 })
    }
    if (selectedProducts.value.length) {
      toast.add({
        severity: 'info',
        summary: 'Lưu món trong combo',
        detail: 'API combo items chưa có, dữ liệu sẽ chỉ hiển thị tạm thời.',
        life: 2500,
      })
    }
    showDialog.value = false
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Không thể lưu combo',
      detail: err?.response?.data?.message || 'Vui lòng thử lại.',
      life: 3000,
    })
  }
}

const removeCombo = (row: any) => {
  confirm.require({
    message: `Xóa combo ${row.Name}?`,
    header: 'Xác nhận',
    icon: 'pi pi-exclamation-triangle',
    accept: async () => {
      await admin.removeCombo(row.ComboId)
      toast.add({ severity: 'success', summary: 'Đã xóa combo', life: 2000 })
    },
  })
}

const productOptions = computed(() =>
  admin.products.map((p) => ({ label: p.Name, value: p.ProductId, image: p.ImageUrl })),
)

const getProductById = (productId: number) => admin.products.find((p) => p.ProductId === productId)

const formatPrice = (value?: number) => {
  if (value === null || value === undefined) return '-'
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value)
}

const setQuantity = (productId: number, qty: number) => {
  quantities.value[productId] = qty
}

onMounted(async () => {
  await admin.loadCatalog()
  await admin.loadCombos()
})
</script>

<template>
  <section>
    <div class="page-header">
      <div>
        <h2>Combos</h2>
        <p>Quản lý combo và món kèm.</p>
      </div>
      <PButton label="Thêm combo" class="p-button-warning" @click="openCreate" />
    </div>

    <PToolbar class="toolbar">
      <template #start>
        <div class="search-field">
          <span class="search-icon" aria-hidden="true">
            <i class="pi pi-search"></i>
          </span>
          <PInputText v-model="search" placeholder="Tìm combo..." />
        </div>
      </template>
    </PToolbar>

    <div class="table-shell">
      <PDataTable
        :value="filteredCombos"
        responsive-layout="scroll"
        class="table ruixen-table"
        scrollable
        scroll-height="655px"
        sort-mode="multiple"
        :removable-sort="true"
      >
        <PColumn field="Name" header="Tên" sortable />
        <PColumn field="Price" header="Giá" sortable />
        <PColumn field="IsActive" header="Trạng thái" sortable>
          <template #body="{ data }">
            <PTag :value="data.IsActive ? 'Đang bán' : 'Tạm dừng'" :severity="data.IsActive ? 'success' : 'danger'" />
          </template>
        </PColumn>
        <PColumn header="Actions" style="width: 160px">
          <template #body="{ data }">
            <PButton icon="pi pi-pencil" class="p-button-text" @click="openEdit(data)" />
            <PButton icon="pi pi-trash" class="p-button-text" @click="removeCombo(data)" />
          </template>
        </PColumn>
      </PDataTable>
    </div>

    <PDialog v-model:visible="showDialog" modal :header="isEdit ? 'Cập nhật combo' : 'Thêm combo'">
      <div class="dialog-grid">
        <div class="dialog-form">
          <div class="field">
            <label>Tên combo</label>
            <PInputText v-model="form.Name" />
          </div>
          <div class="field">
            <label>Mô tả</label>
            <PTextarea v-model="form.Description" rows="3" />
          </div>
          <div class="field">
            <label>Ảnh combo (URL)</label>
            <PInputText v-model="form.ImageUrl" placeholder="https://..." />
          </div>
          <div class="field">
            <label>Giá</label>
            <PInputNumber v-model="form.Price" :min="0" />
          </div>
          <div class="field inline">
            <label>Đang bán</label>
            <PInputSwitch v-model="form.IsActive" :true-value="1" :false-value="0" />
          </div>
        </div>

        <aside class="combo-panel">
          <div class="panel-header">
            <div>
              <p class="panel-title">Món trong combo</p>
              <span class="panel-subtitle">{{ selectedProducts.length }} món đã chọn</span>
            </div>
          </div>
          <div class="combo-preview">
            <img
              :src="
                form.ImageUrl ||
                'https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=400&q=80'
              "
              :alt="form.Name || 'Combo'"
            />
            <div class="preview-meta">
              <span class="preview-name">{{ form.Name || 'Combo mới' }}</span>
              <span class="preview-price">{{ formatPrice(form.Price) }}</span>
            </div>
          </div>
          <div class="field">
            <label>Chọn món</label>
            <PMultiSelect
              v-model="selectedProducts"
              :options="productOptions"
              option-label="label"
              option-value="value"
              placeholder="Chọn món"
              :max-selected-labels="1"
              selected-items-label="{0} món đã chọn"
              class="product-multiselect"
            >
              <template #option="slotProps">
                <div class="product-option">
                  <img :src="slotProps.option.image" :alt="slotProps.option.label" />
                  <span>{{ slotProps.option.label }}</span>
                </div>
              </template>
            </PMultiSelect>
          </div>
          <div v-if="selectedProducts.length" class="combo-items">
            <div v-for="id in selectedProducts" :key="id" class="combo-item">
              <div class="item-media">
                <img
                  :src="getProductById(id)?.ImageUrl || 'https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=200&q=80'"
                  :alt="getProductById(id)?.Name || 'Món ăn'"
                />
              </div>
              <div class="item-info">
                <p class="item-name">{{ getProductById(id)?.Name || 'Chưa có tên' }}</p>
                <span class="item-price">{{ formatPrice(getProductById(id)?.Price) }}</span>
              </div>
              <div class="item-qty">
                <span class="qty-label">SL</span>
                <PInputNumber
                  :min="1"
                  :model-value="quantities[id] || 1"
                  @update:model-value="(val) => setQuantity(id, Number(val))"
                />
              </div>
            </div>
          </div>
          <div v-else class="combo-empty">
            <p>Chưa có món nào trong combo.</p>
            <span>Chọn món để hiển thị ảnh, giá và số lượng.</span>
          </div>
        </aside>
      </div>
      <template #footer>
        <PButton label="Hủy" class="p-button-text" @click="showDialog = false" />
        <PButton label="Lưu" class="p-button-warning" @click="saveCombo" />
      </template>
    </PDialog>
  </section>
</template>

<style scoped>
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 16px;
  margin-bottom: 16px;
}

.page-header h2 {
  font-family: 'Fraunces', serif;
  font-size: clamp(1.6rem, 2.6vw, 2.2rem);
}

.page-header p {
  color: var(--muted);
}

.toolbar {
  margin-bottom: 12px;
  background: transparent;
  border: none;
}

.search-field {
  display: flex;
  align-items: center;
  gap: 10px;
}

.search-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: var(--radius);
  border: 1px solid rgba(31, 19, 11, 0.12);
  background: var(--surface-2);
  color: var(--accent);
  flex-shrink: 0;
}

.search-field :deep(.p-inputtext) {
  height: 36px;
  min-width: min(320px, 60vw);
  border-radius: var(--radius);
  border: 1px solid rgba(31, 19, 11, 0.12);
  background: var(--surface);
}

.table-shell {
  background: var(--surface);
  border-radius: var(--radius);
  border: 1px solid rgba(31, 19, 11, 0.08);
  box-shadow: var(--shadow);
  padding: 6px;
}

.table {
  border: 1px solid rgba(31, 19, 11, 0.08);
  border-radius: var(--radius);
  overflow: hidden;
}

.ruixen-table :deep(.p-datatable-header) {
  background: transparent;
}

.ruixen-table :deep(.p-datatable-thead > tr > th) {
  background: rgba(255, 246, 235, 0.9);
  font-weight: 600;
  color: rgba(31, 19, 11, 0.7);
  border-color: rgba(31, 19, 11, 0.06);
  padding: 14px 16px;
}

.ruixen-table :deep(.p-column-header-content) {
  gap: 6px;
}

.ruixen-table :deep(.p-datatable-tbody > tr > td) {
  border-color: rgba(31, 19, 11, 0.06);
  padding: 14px 16px;
}

.ruixen-table :deep(.p-datatable-tbody > tr) {
  height: 56px;
}

.dialog-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(260px, 0.9fr);
  gap: 18px;
}

.dialog-form {
  display: grid;
  gap: 12px;
}

.field {
  display: grid;
  gap: 6px;
}

.field.inline {
  grid-auto-flow: column;
  align-items: center;
  justify-content: space-between;
}

.combo-panel {
  background: rgba(255, 246, 235, 0.65);
  border: 1px solid rgba(31, 19, 11, 0.08);
  border-radius: 14px;
  padding: 14px;
  display: grid;
  gap: 12px;
  align-content: start;
}

.combo-preview {
  display: grid;
  gap: 10px;
  background: #fff;
  border-radius: 14px;
  padding: 10px;
  border: 1px solid rgba(31, 19, 11, 0.08);
}

.combo-preview img {
  width: 100%;
  height: 140px;
  object-fit: cover;
  border-radius: 12px;
  display: block;
}

.product-option {
  display: flex;
  align-items: center;
  gap: 10px;
}

.product-option img {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  object-fit: cover;
  border: 1px solid rgba(31, 19, 11, 0.08);
}

.product-multiselect :deep(.p-multiselect-label) {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.preview-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
}

.preview-name {
  font-weight: 600;
}

.preview-price {
  color: var(--accent);
  font-weight: 600;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.panel-title {
  font-weight: 700;
  margin: 0;
}

.panel-subtitle {
  color: var(--muted);
  font-size: 0.85rem;
}

.combo-items {
  display: grid;
  gap: 10px;
}

.combo-item {
  display: grid;
  grid-template-columns: 56px minmax(0, 1fr) auto;
  gap: 10px;
  align-items: center;
  background: #fff;
  border-radius: 12px;
  padding: 8px;
  border: 1px solid rgba(31, 19, 11, 0.08);
}

.item-media img {
  width: 56px;
  height: 56px;
  border-radius: 10px;
  object-fit: cover;
  display: block;
}

.item-info {
  display: grid;
  gap: 4px;
}

.item-name {
  font-weight: 600;
  margin: 0;
}

.item-price {
  color: var(--accent);
  font-size: 0.9rem;
}

.item-qty {
  display: grid;
  gap: 4px;
  justify-items: end;
}

.qty-label {
  font-size: 0.75rem;
  color: var(--muted);
}

.combo-empty {
  border: 1px dashed rgba(31, 19, 11, 0.2);
  border-radius: 12px;
  padding: 14px;
  text-align: center;
  color: var(--muted);
}

.combo-empty p {
  margin: 0 0 6px;
  font-weight: 600;
  color: rgba(31, 19, 11, 0.65);
}

@media (max-width: 900px) {
  .dialog-grid {
    grid-template-columns: 1fr;
  }
}
</style>
