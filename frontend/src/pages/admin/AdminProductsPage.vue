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
  ProductId: 0,
  CategoryId: 0,
  Name: '',
  Description: '',
  Price: 0,
  ImageUrl: '',
  TopicTag: '',
  IsAvailable: 1,
  StockQty: 0,
})

const filteredProducts = computed(() => {
  if (!search.value.trim()) return admin.products
  const q = search.value.toLowerCase()
  return admin.products.filter((p) => p.Name.toLowerCase().includes(q))
})

const openCreate = () => {
  isEdit.value = false
  form.value = {
    ProductId: 0,
    CategoryId: admin.categories[0]?.CategoryId || 0,
    Name: '',
    Description: '',
    Price: 0,
    ImageUrl: '',
    TopicTag: '',
    IsAvailable: 1,
    StockQty: 0,
  }
  showDialog.value = true
}

const openEdit = (row: any) => {
  isEdit.value = true
  form.value = { ...row }
  showDialog.value = true
}

const saveProduct = async () => {
  if (form.value.Price <= 0) {
    toast.add({ severity: 'warn', summary: 'Giá phải lớn hơn 0', life: 2000 })
    return
  }
  try {
    const payload = {
      CategoryId: form.value.CategoryId,
      Name: form.value.Name,
      Description: form.value.Description,
      Price: form.value.Price,
      ImageUrl: form.value.ImageUrl,
      TopicTag: form.value.TopicTag,
      IsAvailable: form.value.IsAvailable,
      StockQty: form.value.StockQty,
    }
    if (isEdit.value) {
      await admin.updateProductItem(form.value.ProductId, payload)
      toast.add({ severity: 'success', summary: 'Đã cập nhật sản phẩm', life: 2000 })
    } else {
      await admin.addProduct(payload)
      toast.add({ severity: 'success', summary: 'Đã tạo sản phẩm', life: 2000 })
    }
    showDialog.value = false
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Không thể lưu sản phẩm',
      detail: err?.response?.data?.message || 'Vui lòng thử lại.',
      life: 3000,
    })
  }
}

const removeProduct = (row: any) => {
  confirm.require({
    message: `Xóa sản phẩm ${row.Name}?`,
    header: 'Xác nhận',
    icon: 'pi pi-exclamation-triangle',
    accept: async () => {
      await admin.removeProduct(row.ProductId)
      toast.add({ severity: 'success', summary: 'Đã xóa sản phẩm', life: 2000 })
    },
  })
}

onMounted(admin.loadCatalog)
</script>

<template>
  <section>
    <div class="page-header">
      <div>
        <h2>Products</h2>
        <p>Quản lý thực đơn và hàng tồn.</p>
      </div>
      <PButton label="Thêm sản phẩm" class="p-button-warning" @click="openCreate" />
    </div>

    <PToolbar class="toolbar">
      <template #start>
        <div class="search-field">
          <span class="search-icon" aria-hidden="true">
            <i class="pi pi-search"></i>
          </span>
          <PInputText v-model="search" placeholder="Tìm theo tên..." />
        </div>
      </template>
    </PToolbar>

    <div class="table-shell">
      <PDataTable
        :value="filteredProducts"
        responsive-layout="scroll"
        class="table ruixen-table"
        scrollable
        scroll-height="655px"
        sort-mode="multiple"
        :removable-sort="true"
      >
        <PColumn header="Ảnh">
          <template #body="{ data }">
            <div class="thumb">
              <img :src="data.ImageUrl" :alt="data.Name" />
            </div>
          </template>
        </PColumn>
      <PColumn field="Name" header="Tên" sortable />
      <PColumn field="Price" header="Giá" sortable />
      <PColumn field="StockQty" header="Tồn" sortable>
        <template #body="{ data }">
          {{ data.StockQty }}
        </template>
      </PColumn>
      <PColumn field="IsAvailable" header="Trạng thái" sortable>
        <template #body="{ data }">
          <PTag :value="data.IsAvailable ? 'Còn hàng' : 'Tạm hết'" :severity="data.IsAvailable ? 'success' : 'danger'" />
        </template>
      </PColumn>
      <PColumn header="Actions" style="width: 160px">
        <template #body="{ data }">
          <PButton icon="pi pi-pencil" class="p-button-text" @click="openEdit(data)" />
          <PButton icon="pi pi-trash" class="p-button-text" @click="removeProduct(data)" />
        </template>
      </PColumn>
      </PDataTable>
    </div>

    <PDialog v-model:visible="showDialog" modal :header="isEdit ? 'Cập nhật sản phẩm' : 'Thêm sản phẩm'">
      <div class="dialog-preview">
        <img v-if="form.ImageUrl" :src="form.ImageUrl" :alt="form.Name || 'Preview'" />
        <div v-else class="preview-placeholder">Ảnh xem trước</div>
      </div>
      <div class="dialog-form">
        <div class="field">
          <label>Danh mục</label>
          <PDropdown v-model="form.CategoryId" :options="admin.categories" option-label="Name" option-value="CategoryId" />
        </div>
        <div class="field">
          <label>Tên</label>
          <PInputText v-model="form.Name" />
        </div>
        <div class="field">
          <label>Mô tả</label>
          <PTextarea v-model="form.Description" rows="3" />
        </div>
        <div class="field">
          <label>Giá</label>
          <PInputNumber v-model="form.Price" :min="0" />
        </div>
        <div class="field">
          <label>Image URL</label>
          <PInputText v-model="form.ImageUrl" />
        </div>
        <div class="field">
          <label>Tag</label>
          <PInputText v-model="form.TopicTag" />
        </div>
        <div class="field inline">
          <label>Còn hàng</label>
          <PInputSwitch v-model="form.IsAvailable" :true-value="1" :false-value="0" />
        </div>
        <div class="field">
          <label>Tồn kho</label>
          <PInputNumber v-model="form.StockQty" :min="0" />
        </div>
      </div>
      <template #footer>
        <PButton label="Hủy" class="p-button-text" @click="showDialog = false" />
        <PButton label="Lưu" class="p-button-warning" @click="saveProduct" />
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

.thumb {
  width: 44px;
  height: 44px;
  border-radius: var(--radius);
  overflow: hidden;
  border: 1px solid rgba(31, 19, 11, 0.12);
  background: rgba(255, 224, 196, 0.8);
}

.thumb img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.dialog-form {
  display: grid;
  gap: 12px;
}

.dialog-preview {
  width: min(100%, 720px);
  height: 220px;
  margin: 16px auto 12px;
  border-radius: var(--radius);
  border: 1px solid rgba(31, 19, 11, 0.12);
  background: var(--surface-2);
  overflow: hidden;
  display: grid;
  place-items: center;
}

.dialog-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.preview-placeholder {
  color: var(--muted);
  font-weight: 600;
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
</style>
