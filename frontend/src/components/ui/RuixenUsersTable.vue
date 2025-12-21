<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { FilterMatchMode } from 'primevue/api'
import type { PropType } from 'vue'
import type { AdminUser } from '../../services/admin'

const props = defineProps({
  users: {
    type: Array as PropType<AdminUser[]>,
    required: true,
  },
})

const emit = defineEmits<{
  (event: 'edit', user: AdminUser): void
  (event: 'toggle-status', user: AdminUser): void
}>()

const avatarFallback = reactive<Record<number, boolean>>({})
const filters = ref({
  RoleName: { value: [] as string[], matchMode: FilterMatchMode.IN },
})

const roleOptions = computed(() => {
  const names = props.users.map((user) => user.RoleName || '—')
  return Array.from(new Set(names)).map((name) => ({ label: name, value: name }))
})

const formatDate = (value?: string) => {
  if (!value) return '-'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '-'
  return new Intl.DateTimeFormat('vi-VN', { dateStyle: 'medium' }).format(date)
}

const getInitials = (name?: string) => {
  if (!name) return 'U'
  const parts = name.trim().split(/\s+/).slice(0, 2)
  return parts.map((part) => part[0]?.toUpperCase() || '').join('') || 'U'
}

const getRoleTag = (role?: string | null) => {
  const normalized = (role || '').toLowerCase()
  if (!normalized || normalized === '—') {
    return { label: '-', severity: 'secondary' as const }
  }
  if (normalized.includes('admin') || normalized.includes('quản trị') || normalized.includes('quan tri')) {
    return { label: role!, severity: 'danger' as const }
  }
  if (normalized.includes('nhân viên') || normalized.includes('staff')) {
    return { label: role!, severity: 'warning' as const }
  }
  if (normalized.includes('khách') || normalized.includes('customer')) {
    return { label: role!, severity: 'success' as const }
  }
  return { label: role!, severity: 'info' as const }
}
</script>

<template>
  <div class="table-shell">
    <PDataTable
      :value="props.users"
      responsive-layout="scroll"
      class="table ruixen-table"
      scrollable
      scroll-height="670px"
      sort-mode="multiple"
      :removable-sort="true"
      v-model:filters="filters"
      filter-display="menu"
    >
      <PColumn header="Ảnh">
        <template #body="{ data }">
          <div class="avatar">
            <img
              v-if="data.AvatarUrl && !avatarFallback[data.UserId]"
              :src="data.AvatarUrl"
              :alt="data.FullName"
              @error="avatarFallback[data.UserId] = true"
            />
            <span v-else>{{ getInitials(data.FullName) }}</span>
          </div>
        </template>
      </PColumn>
      <PColumn field="FullName" header="Họ tên" sortable />
      <PColumn field="Email" header="Email" sortable />
      <PColumn field="RoleName" header="Vai trò" sortable :show-filter-menu="false">
        <template #body="{ data }">
          <PTag :value="getRoleTag(data.RoleName).label" :severity="getRoleTag(data.RoleName).severity" />
        </template>
        <template #filter>
          <PMultiSelect
            v-model="filters.RoleName.value"
            :options="roleOptions"
            placeholder="Chọn vai trò"
            class="role-filter"
          />
        </template>
      </PColumn>
      <PColumn field="Phone" header="SĐT" sortable>
        <template #body="{ data }">
          <span>{{ data.Phone || '-' }}</span>
        </template>
      </PColumn>
      <PColumn field="Address" header="Địa chỉ" sortable>
        <template #body="{ data }">
          <span>{{ data.Address || '-' }}</span>
        </template>
      </PColumn>
      <PColumn field="IsActive" header="Trạng thái" sortable>
        <template #body="{ data }">
          <PTag :value="data.IsActive ? 'Đang hoạt động' : 'Tạm khóa'" :severity="data.IsActive ? 'success' : 'danger'" />
        </template>
      </PColumn>
      <PColumn field="CreatedAt" header="Ngày tạo" sortable>
        <template #body="{ data }">
          <span>{{ formatDate(data.CreatedAt) }}</span>
        </template>
      </PColumn>
      <PColumn header="Thao tác" style="width: 150px">
        <template #body="{ data }">
          <PButton icon="pi pi-pencil" class="p-button-text" @click="emit('edit', data)" />
          <PButton
            :icon="data.IsActive ? 'pi pi-ban' : 'pi pi-check'"
            class="p-button-text"
            @click="emit('toggle-status', data)"
          />
        </template>
      </PColumn>
    </PDataTable>
  </div>
</template>

<style scoped>
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

.role-filter {
  min-width: 180px;
}

.avatar {
  width: 36px;
  height: 36px;
  border-radius: 999px;
  overflow: hidden;
  display: grid;
  place-items: center;
  font-weight: 700;
  font-size: 0.75rem;
  color: #7d5133;
  background: rgba(255, 224, 196, 0.8);
  border: 1px solid rgba(31, 19, 11, 0.12);
}

.avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

</style>
