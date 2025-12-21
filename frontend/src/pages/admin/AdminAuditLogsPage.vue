<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAuthStore } from '../../stores/auth'
import { getAuditLogs, type AdminAuditLog } from '../../services/admin'

const auth = useAuthStore()
const logs = ref<AdminAuditLog[]>([])
const isLoading = ref(false)
const error = ref('')
const showDialog = ref(false)
const selectedLog = ref<AdminAuditLog | null>(null)
const search = ref('')

const loadLogs = async () => {
  if (!auth.user?.UserId) {
    error.value = 'Thiếu thông tin tài khoản quản trị.'
    return
  }
  isLoading.value = true
  error.value = ''
  try {
    logs.value = await getAuditLogs(200, auth.user?.UserId)
  } catch (err) {
    error.value = 'Không thể tải nhật ký. Hãy kiểm tra backend.'
  } finally {
    isLoading.value = false
  }
}

const openDetail = (log: AdminAuditLog) => {
  selectedLog.value = log
  showDialog.value = true
}

const filteredLogs = computed(() => {
  if (!search.value.trim()) return logs.value
  const q = search.value.toLowerCase()
  return logs.value.filter(
    (log) =>
      log.Action.toLowerCase().includes(q) ||
      log.EntityName.toLowerCase().includes(q) ||
      String(log.EntityId).includes(q) ||
      (log.ActorName || '').toLowerCase().includes(q) ||
      (log.ActorEmail || '').toLowerCase().includes(q) ||
      (log.IpAddress || '').toLowerCase().includes(q),
  )
})

onMounted(async () => {
  if (!auth.user) {
    await auth.loadUserFromToken()
  }
  await loadLogs()
})
</script>

<template>
  <section>
    <div class="page-header">
      <div>
        <h2>Nhật ký hệ thống</h2>
        <p>Theo dõi các thay đổi dữ liệu gần đây.</p>
      </div>
      <PButton label="Làm mới" icon="pi pi-refresh" class="p-button-text" @click="loadLogs" />
    </div>

    <PToolbar class="toolbar">
      <template #start>
        <div class="search-field">
          <span class="search-icon" aria-hidden="true">
            <i class="pi pi-search"></i>
          </span>
          <PInputText v-model="search" placeholder="Tìm theo hành động, bảng, người thao tác..." />
        </div>
      </template>
    </PToolbar>

    <div v-if="error" class="error">{{ error }}</div>

    <div class="table-shell">
      <PDataTable
        :value="filteredLogs"
        :loading="isLoading"
        responsive-layout="scroll"
        class="table ruixen-table"
        :paginator="filteredLogs.length > 10"
        :rows="10"
        scrollable
        scroll-height="655px"
      >
        <PColumn field="CreatedAt" header="Thời gian" />
        <PColumn header="Người thao tác">
          <template #body="{ data }">
            <div class="actor">
              <span class="actor-name">{{ data.ActorName || 'Hệ thống' }}</span>
              <span class="actor-email">{{ data.ActorEmail || '—' }}</span>
            </div>
          </template>
        </PColumn>
        <PColumn field="Action" header="Hành động" />
        <PColumn field="EntityName" header="Bảng" />
        <PColumn field="EntityId" header="ID" />
        <PColumn field="IpAddress" header="IP" />
        <PColumn header="Chi tiết" style="width: 120px">
          <template #body="{ data }">
            <PButton icon="pi pi-eye" class="p-button-text" @click="openDetail(data)" />
          </template>
        </PColumn>
      </PDataTable>
    </div>

    <PDialog v-model:visible="showDialog" modal header="Chi tiết nhật ký" :style="{ width: '720px' }">
      <div v-if="selectedLog" class="log-detail">
        <div class="log-meta">
          <div>
            <span>Hành động</span>
            <strong>{{ selectedLog.Action }}</strong>
          </div>
          <div>
            <span>Bảng</span>
            <strong>{{ selectedLog.EntityName }} #{{ selectedLog.EntityId }}</strong>
          </div>
          <div>
            <span>Người thao tác</span>
            <strong>{{ selectedLog.ActorName || 'Hệ thống' }}</strong>
          </div>
          <div>
            <span>Thời gian</span>
            <strong>{{ selectedLog.CreatedAt }}</strong>
          </div>
        </div>
        <div class="log-json">
          <div>
            <p>Giá trị cũ</p>
            <pre>{{ selectedLog.OldValuesJson }}</pre>
          </div>
          <div>
            <p>Giá trị mới</p>
            <pre>{{ selectedLog.NewValuesJson }}</pre>
          </div>
        </div>
      </div>
      <template #footer>
        <PButton label="Đóng" class="p-button-text" @click="showDialog = false" />
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

.ruixen-table :deep(.p-datatable-tbody > tr > td) {
  border-color: rgba(31, 19, 11, 0.06);
  padding: 14px 16px;
}

.ruixen-table :deep(.p-datatable-tbody > tr) {
  height: 56px;
}

.actor {
  display: grid;
  gap: 2px;
}

.actor-name {
  font-weight: 600;
  color: var(--text);
}

.actor-email {
  font-size: 0.75rem;
  color: var(--muted);
}

.log-detail {
  display: grid;
  gap: 16px;
}

.log-meta {
  display: grid;
  gap: 8px;
}

.log-meta div {
  display: flex;
  justify-content: space-between;
}

.log-json {
  display: grid;
  gap: 16px;
}

.log-json pre {
  background: var(--surface-2);
  border-radius: var(--radius);
  border: 1px solid rgba(31, 19, 11, 0.08);
  padding: 12px;
  max-height: 220px;
  overflow: auto;
  white-space: pre-wrap;
  word-break: break-word;
}

.error {
  color: #fca5a5;
  padding-bottom: 12px;
}
</style>
