<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAdminStore } from '../../stores/admin'
import { useAuthStore } from '../../stores/auth'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import RuixenUsersTable from '../../components/ui/RuixenUsersTable.vue'

const admin = useAdminStore()
const auth = useAuthStore()
const toast = useToast()
const confirm = useConfirm()

const search = ref('')
const showDialog = ref(false)
const isEdit = ref(false)
const form = ref({
  UserId: 0,
  FullName: '',
  Email: '',
  Password: '',
  Phone: '',
  Address: '',
  AvatarUrl: '',
  IsActive: 1,
  ForcePasswordReset: 0,
  RoleId: 0,
})
const avatarFallback =
  'https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=400&q=80'

const filteredUsers = computed(() => {
  if (!search.value.trim()) return admin.users
  const q = search.value.toLowerCase()
  return admin.users.filter(
    (u) =>
      u.FullName.toLowerCase().includes(q) ||
      u.Email.toLowerCase().includes(q) ||
      (u.Phone || '').toLowerCase().includes(q),
  )
})

const roleOptions = computed(() =>
  admin.roles.map((role) => ({
    label: role.Name,
    value: role.RoleId,
  })),
)

const getDefaultRoleId = () => {
  const customerRole = admin.roles.find((role) => role.Name.toLowerCase().includes('khách'))
  return customerRole?.RoleId || admin.roles[0]?.RoleId || 0
}

const openCreate = () => {
  isEdit.value = false
  form.value = {
    UserId: 0,
    FullName: '',
    Email: '',
    Password: '',
    Phone: '',
    Address: '',
    AvatarUrl: '',
    IsActive: 1,
    ForcePasswordReset: 0,
    RoleId: getDefaultRoleId(),
  }
  showDialog.value = true
}

const openEdit = (row: any) => {
  isEdit.value = true
  form.value = {
    UserId: row.UserId,
    FullName: row.FullName,
    Email: row.Email,
    Password: '',
    Phone: row.Phone || '',
    Address: row.Address || '',
    AvatarUrl: row.AvatarUrl || '',
    IsActive: row.IsActive,
    ForcePasswordReset: row.ForcePasswordReset ?? 0,
    RoleId: row.RoleId || getDefaultRoleId(),
  }
  showDialog.value = true
}

const saveUser = async () => {
  try {
    if (isEdit.value) {
      await admin.updateUser(form.value.UserId, {
        FullName: form.value.FullName,
        Phone: form.value.Phone || null,
        Address: form.value.Address || null,
        AvatarUrl: form.value.AvatarUrl || null,
        IsActive: form.value.IsActive,
        ForcePasswordReset: form.value.ForcePasswordReset,
      })
      if (form.value.RoleId) {
        await admin.setUserRole(form.value.UserId, form.value.RoleId)
      }
      toast.add({ severity: 'success', summary: 'Đã cập nhật người dùng', life: 2000 })
    } else {
      await admin.addUser({
        FullName: form.value.FullName,
        Email: form.value.Email,
        Password: form.value.Password,
        Phone: form.value.Phone || null,
        Address: form.value.Address || null,
        AvatarUrl: form.value.AvatarUrl || null,
        IsActive: form.value.IsActive,
        ForcePasswordReset: form.value.ForcePasswordReset,
        RoleId: form.value.RoleId,
      })
      toast.add({ severity: 'success', summary: 'Đã tạo người dùng', life: 2000 })
    }
    showDialog.value = false
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Không thể lưu người dùng',
      detail: err?.response?.data?.message || 'Vui lòng thử lại.',
      life: 3000,
    })
  }
}

const toggleUserStatus = (row: any) => {
  const isActive = Boolean(row.IsActive)
  if (isActive && auth.user?.UserId === row.UserId) {
    toast.add({ severity: 'warn', summary: 'Không thể vô hiệu hóa quản trị viên hiện tại', life: 2500 })
    return
  }
  confirm.require({
    message: isActive ? `Vô hiệu hóa người dùng ${row.FullName}?` : `Kích hoạt lại người dùng ${row.FullName}?`,
    header: 'Xác nhận',
    icon: 'pi pi-exclamation-triangle',
    accept: async () => {
      try {
        if (isActive) {
          await admin.removeUser(row.UserId)
          toast.add({ severity: 'success', summary: 'Người dùng đã được vô hiệu hóa', life: 2000 })
        } else {
          await admin.updateUser(row.UserId, {
            FullName: row.FullName,
            Phone: row.Phone || null,
            Address: row.Address || null,
            AvatarUrl: row.AvatarUrl || null,
            IsActive: 1,
            ForcePasswordReset: row.ForcePasswordReset ?? 0,
          })
          toast.add({ severity: 'success', summary: 'Người dùng đã được kích hoạt', life: 2000 })
        }
      } catch (err: any) {
        toast.add({
          severity: 'error',
          summary: isActive ? 'Không thể vô hiệu hóa' : 'Không thể kích hoạt',
          detail: err?.response?.data?.message || 'Vui lòng thử lại.',
          life: 3000,
        })
      }
    },
  })
}

onMounted(() => {
  admin.loadUsers()
  admin.loadRoles()
})
</script>

<template>
  <section>
    <div class="page-header">
      <div>
        <h2>Người dùng</h2>
        <p>Quản lý tài khoản người dùng và trạng thái hoạt động.</p>
      </div>
      <PButton label="Thêm người dùng" class="p-button-warning" @click="openCreate" />
    </div>

    <PToolbar class="toolbar">
      <template #start>
        <div class="search-field">
          <span class="search-icon" aria-hidden="true">
            <i class="pi pi-search"></i>
          </span>
          <PInputText v-model="search" placeholder="Tìm theo tên, email, SĐT..." />
        </div>
      </template>
    </PToolbar>

    <RuixenUsersTable :users="filteredUsers" @edit="openEdit" @toggle-status="toggleUserStatus" />

    <PDialog v-model:visible="showDialog" :draggable="false" :style="{ width: 'min(760px, 92vw)' }" modal closable>
      <template #header>
        <div class="dialog-header">
          <h3>{{ isEdit ? 'Cập nhật người dùng' : 'Thêm người dùng' }}</h3>
          <p>Cập nhật thông tin để quản lý tài khoản chính xác hơn.</p>
        </div>
      </template>

      <div class="dialog-hero"></div>
      <div class="dialog-avatar">
        <div class="avatar-wrap">
          <img :src="form.AvatarUrl || avatarFallback" alt="Ảnh đại diện" />
        </div>
      </div>

      <div class="dialog-body">
        <div class="dialog-form">
          <div class="row">
            <div class="field">
              <label>Họ tên</label>
              <PInputText v-model="form.FullName" />
            </div>
            <div class="field">
              <label>Email</label>
              <PInputText v-model="form.Email" :disabled="isEdit" />
            </div>
          </div>
          <div class="row" v-if="!isEdit">
            <div class="field">
              <label>Mật khẩu</label>
              <PPassword v-model="form.Password" :feedback="false" toggle-mask />
            </div>
          </div>
          <div class="row">
            <div class="field">
              <label>SĐT</label>
              <PInputText v-model="form.Phone" />
            </div>
            <div class="field">
              <label>Địa chỉ</label>
              <PInputText v-model="form.Address" />
            </div>
          </div>
          <div class="row">
            <div class="field">
              <label>Vai trò</label>
              <PDropdown v-model="form.RoleId" :options="roleOptions" option-label="label" option-value="value" />
            </div>
            <div class="field">
              <label>Trạng thái</label>
              <PDropdown
                v-model="form.IsActive"
                :options="[
                  { label: 'Đang hoạt động', value: 1 },
                  { label: 'Tạm khóa', value: 0 },
                ]"
                option-label="label"
                option-value="value"
              />
            </div>
          </div>
          <div class="row">
            <div class="field full">
              <label>Ảnh đại diện (URL)</label>
              <PInputText v-model="form.AvatarUrl" />
            </div>
          </div>
        </div>
        <div class="dialog-footer">
          <button class="ghost-btn" type="button" @click="showDialog = false">Hủy</button>
          <PButton label="Lưu thay đổi" class="p-button-warning" @click="saveUser" />
        </div>
      </div>
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

.dialog-form {
  display: grid;
  gap: 12px;
}

.field {
  display: grid;
  gap: 6px;
}

.dialog-header {
  display: grid;
  gap: 6px;
}

.dialog-header p {
  color: var(--muted);
  font-size: 0.9rem;
}

.dialog-hero {
  height: 120px;
  border-radius: 14px;
  margin: 0 20px;
  background: radial-gradient(circle at top left, rgba(255, 107, 53, 0.45), rgba(245, 158, 11, 0.3));
}

.dialog-avatar {
  display: flex;
  justify-content: center;
  margin-top: -46px;
}

.avatar-wrap {
  width: 96px;
  height: 96px;
  border-radius: 999px;
  border: 4px solid var(--surface);
  overflow: hidden;
  box-shadow: var(--shadow);
}

.avatar-wrap img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.dialog-body {
  padding: 18px 20px 22px;
}

.row {
  display: grid;
  gap: 14px;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.field.full {
  grid-column: 1 / -1;
}

.switch-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 12px;
  border-radius: 12px;
  border: 1px solid rgba(31, 19, 11, 0.08);
  background: var(--surface-2);
  font-size: 0.9rem;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 10px;
}

.ghost-btn {
  border: 1px solid rgba(31, 19, 11, 0.12);
  background: var(--surface);
  color: var(--accent);
  border-radius: 999px;
  padding: 8px 14px;
  cursor: pointer;
  font-weight: 600;
}
</style>
