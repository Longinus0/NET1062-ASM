<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAdminStore } from '../../stores/admin'
import { useToast } from 'primevue/usetoast'

const admin = useAdminStore()
const toast = useToast()

const showDialog = ref(false)
const selectedOrder = ref<any>(null)
const detail = ref<any>(null)
const statusValue = ref('Đang chuẩn bị')
const statusNote = ref('')
const search = ref('')

const statusOptions = [
  { label: 'Mới', value: 'Mới' },
  { label: 'Đang chuẩn bị', value: 'Đang chuẩn bị' },
  { label: 'Đang giao', value: 'Đang giao' },
  { label: 'Đã giao', value: 'Đã giao' },
  { label: 'Đã hủy', value: 'Đã hủy' },
]

const formatTimeAgo = (value: string) => {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '-'
  const diffMs = Date.now() - date.getTime()
  const diffMinutes = Math.floor(diffMs / 60000)
  if (diffMinutes < 1) return 'Vừa xong'
  if (diffMinutes < 60) return `${diffMinutes} phút trước`
  const diffHours = Math.floor(diffMinutes / 60)
  if (diffHours < 24) return `${diffHours} giờ trước`
  const diffDays = Math.floor(diffHours / 24)
  return `${diffDays} ngày trước`
}

const filteredOrders = computed(() => {
  if (!search.value.trim()) return admin.orders
  const q = search.value.toLowerCase()
  return admin.orders.filter(
    (order) =>
      order.OrderCode.toLowerCase().includes(q) ||
      order.Status.toLowerCase().includes(q) ||
      order.PaymentStatus.toLowerCase().includes(q) ||
      order.PaymentMethod.toLowerCase().includes(q) ||
      String(order.UserId).includes(q),
  )
})

const loadOrders = async () => {
  await admin.loadOrders()
}

const openDetail = async (row: any) => {
  selectedOrder.value = row
  detail.value = await admin.getOrderDetail(row.OrderId)
  statusValue.value = row.Status
  statusNote.value = ''
  showDialog.value = true
}

const updateStatus = async () => {
  if (!selectedOrder.value) return
  try {
    await admin.setOrderStatus(selectedOrder.value.OrderId, statusValue.value, statusNote.value)
    toast.add({ severity: 'success', summary: 'Đã cập nhật trạng thái', life: 2000 })
    showDialog.value = false
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Không thể cập nhật',
      detail: err?.response?.data?.message || 'Vui lòng thử lại.',
      life: 3000,
    })
  }
}

onMounted(loadOrders)
</script>

<template>
  <section>
    <div class="page-header">
      <div>
        <h2>Đơn hàng</h2>
        <p>Quản lý trạng thái đơn hàng.</p>
      </div>
    </div>

    <PToolbar class="toolbar">
      <template #start>
        <div class="search-field">
          <span class="search-icon" aria-hidden="true">
            <i class="pi pi-search"></i>
          </span>
          <PInputText v-model="search" placeholder="Tìm theo mã đơn, trạng thái..." />
        </div>
      </template>
    </PToolbar>

    <div class="table-shell">
      <PDataTable
        :value="filteredOrders"
        responsive-layout="scroll"
        class="table ruixen-table"
        scrollable
        scroll-height="655px"
        sort-mode="multiple"
        :removable-sort="true"
      >
        <PColumn field="OrderCode" header="Mã đơn" sortable />
        <PColumn field="UserId" header="Khách hàng" sortable />
        <PColumn field="Status" header="Trạng thái" sortable>
          <template #body="{ data }">
            <PTag
              :value="data.Status"
              :severity="
                data.Status === 'Đã giao'
                  ? 'success'
                  : data.Status === 'Đang giao'
                    ? 'info'
                    : data.Status === 'Đang chuẩn bị'
                      ? 'warning'
                      : data.Status === 'Đã hủy'
                        ? 'danger'
                        : 'secondary'
              "
            />
          </template>
        </PColumn>
        <PColumn field="CreatedAt" header="Thời gian" sortable>
          <template #body="{ data }">
            <span>{{ formatTimeAgo(data.CreatedAt) }}</span>
          </template>
        </PColumn>
        <PColumn field="GrandTotal" header="Tổng" sortable />
        <PColumn header="Thao tác" style="width: 140px">
          <template #body="{ data }">
            <PButton icon="pi pi-eye" class="p-button-text" @click="openDetail(data)" />
          </template>
        </PColumn>
      </PDataTable>
    </div>

    <PDialog v-model:visible="showDialog" modal header="Chi tiết đơn hàng" :style="{ width: '640px' }">
      <div v-if="detail" class="order-detail">
        <div class="summary">
          <div>
            <span>Mã đơn</span>
            <strong>{{ detail.Order.OrderCode }}</strong>
          </div>
          <div>
            <span>Trạng thái</span>
            <strong>{{ detail.Order.Status }}</strong>
          </div>
          <div>
            <span>Tổng tiền</span>
            <strong>{{ detail.Order.GrandTotal }}</strong>
          </div>
        </div>
        <div class="items">
          <div v-for="item in detail.Items" :key="item.OrderItemId" class="item">
            <span>{{ item.ProductNameSnapshot }}</span>
            <span>x{{ item.Quantity }}</span>
          </div>
        </div>
        <div class="status-form">
          <label>Cập nhật trạng thái</label>
          <PDropdown v-model="statusValue" :options="statusOptions" option-label="label" option-value="value" />
          <PTextarea v-model="statusNote" rows="2" placeholder="Ghi chú lý do" />
        </div>
      </div>
      <template #footer>
        <PButton label="Đóng" class="p-button-text" @click="showDialog = false" />
        <PButton label="Lưu" class="p-button-warning" @click="updateStatus" />
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

.order-detail {
  display: grid;
  gap: 16px;
}

.summary {
  display: grid;
  gap: 8px;
}

.summary div {
  display: flex;
  justify-content: space-between;
}

.items {
  border-top: 1px solid rgba(31, 19, 11, 0.08);
  padding-top: 8px;
  display: grid;
  gap: 6px;
}

.item {
  display: flex;
  justify-content: space-between;
}

.status-form {
  display: grid;
  gap: 8px;
}
</style>
