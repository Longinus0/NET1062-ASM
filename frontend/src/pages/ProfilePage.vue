<script setup lang="ts">
import { Form, Field, ErrorMessage } from 'vee-validate'
import * as yup from 'yup'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '../stores/auth'
import { computed, onMounted, ref, watch } from 'vue'
import { changePassword, getProfile } from '../services/auth'

const toast = useToast()
const authStore = useAuthStore()

const profileSchema = yup.object({
  FullName: yup.string().min(3, 'Tối thiểu 3 ký tự').required('Vui lòng nhập họ tên'),
  Phone: yup.string().nullable(),
  Address: yup.string().nullable(),
  AvatarUrl: yup.string().url('URL không hợp lệ').nullable(),
})

const passwordSchema = yup.object({
  CurrentPassword: yup.string().required('Vui lòng nhập mật khẩu hiện tại'),
  NewPassword: yup.string().min(6, 'Mật khẩu tối thiểu 6 ký tự').required('Vui lòng nhập mật khẩu mới'),
  ConfirmPassword: yup
    .string()
    .oneOf([yup.ref('NewPassword')], 'Mật khẩu không khớp')
    .required('Vui lòng xác nhận mật khẩu'),
})

const isLoading = ref(false)
const isPasswordLoading = ref(false)
const showDialog = ref(false)
const dialogKey = ref(0)
const avatarPreview = ref('')
const fileInput = ref<HTMLInputElement | null>(null)

type ProfileForm = yup.InferType<typeof profileSchema>
type PasswordForm = yup.InferType<typeof passwordSchema>

const profileValues = ref<ProfileForm>({
  FullName: authStore.user?.FullName || '',
  Phone: authStore.user?.Phone || '',
  Address: authStore.user?.Address || '',
  AvatarUrl: authStore.user?.AvatarUrl || '',
})

const email = computed(() => authStore.user?.Email || '')
const dialogValues = ref<ProfileForm>({ ...profileValues.value })

const refreshProfile = async () => {
  if (!authStore.user) return
  try {
    const data = await getProfile(authStore.user.UserId)
    authStore.user = data
    profileValues.value = {
      FullName: data.FullName,
      Phone: data.Phone || '',
      Address: data.Address || '',
      AvatarUrl: data.AvatarUrl || '',
    }
    dialogValues.value = { ...profileValues.value }
    avatarPreview.value = dialogValues.value.AvatarUrl || ''
  } catch {
    // ignore
  }
}

const onSaveProfile = async (values: Record<string, unknown>) => {
  const payload = values as ProfileForm
  if (!authStore.user) return
  isLoading.value = true
  try {
    await authStore.saveProfile(payload)
    toast.add({ severity: 'success', summary: 'Cập nhật thành công', detail: 'Thông tin đã được lưu.', life: 2500 })
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Không thể cập nhật',
      detail: err?.response?.data?.message || 'Vui lòng thử lại.',
      life: 3000,
    })
  } finally {
    isLoading.value = false
  }
}

const openDialog = () => {
  dialogValues.value = { ...profileValues.value }
  avatarPreview.value = dialogValues.value.AvatarUrl || ''
  dialogKey.value += 1
  showDialog.value = true
}

const onDialogSave = async (values: Record<string, unknown>) => {
  await onSaveProfile(values)
  showDialog.value = false
}

const showSignoutConfirm = ref(false)

const onSignOut = () => {
  showSignoutConfirm.value = true
}

const onConfirmSignOut = () => {
  showSignoutConfirm.value = false
  authStore.logout()
  router.push('/login')
}

const onPickAvatar = () => {
  fileInput.value?.click()
}

const onAvatarChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return
  const url = URL.createObjectURL(file)
  avatarPreview.value = url
}

watch(
  () => dialogValues.value.AvatarUrl,
  (value) => {
    if (value) {
      avatarPreview.value = value
    }
  },
)

const onChangePassword = async (_values: Record<string, unknown>) => {
  if (!authStore.user) return
  const values = _values as PasswordForm
  isPasswordLoading.value = true
  try {
    await changePassword(authStore.user.UserId, {
      CurrentPassword: values.CurrentPassword,
      NewPassword: values.NewPassword,
    })
    toast.add({
      severity: 'success',
      summary: 'Đổi mật khẩu thành công',
      detail: 'Bạn có thể tiếp tục sử dụng tài khoản.',
      life: 2500,
    })
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Không thể đổi mật khẩu',
      detail: err?.response?.data?.message || 'Vui lòng thử lại.',
      life: 3000,
    })
  } finally {
    isPasswordLoading.value = false
  }
}

onMounted(refreshProfile)
</script>

<template>
  <section class="container profile">
    <div class="profile-header">
      <div>
        <h2>Hồ sơ cá nhân</h2>
        <p>Cập nhật thông tin và bảo mật tài khoản.</p>
      </div>
      <PButton label="Đăng xuất" class="p-button-warning signout-btn" @click="onSignOut" />
    </div>

    <div class="profile-grid">
      <div class="profile-card panel">
        <div class="profile-hero"></div>
        <div class="profile-body">
          <div class="profile-avatar">
            <img
              :src="profileValues.AvatarUrl || 'https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=400&q=80'"
              alt="Ảnh đại diện"
            />
          </div>
          <div class="profile-info">
            <div>
              <h3>{{ profileValues.FullName || 'Khách hàng Saigon Fast' }}</h3>
              <p>{{ email }}</p>
            </div>
            <button class="ghost-btn" type="button" @click="openDialog">Chỉnh sửa hồ sơ</button>
          </div>
          <div class="profile-grid-info">
            <div>
              <span>Số điện thoại</span>
              <strong>{{ profileValues.Phone || 'Chưa cập nhật' }}</strong>
            </div>
            <div>
              <span>Địa chỉ</span>
              <strong>{{ profileValues.Address || 'Chưa cập nhật' }}</strong>
            </div>
          </div>
        </div>
      </div>

      <PCard class="panel">
        <template #title>Đổi mật khẩu</template>
        <template #content>
          <Form :validation-schema="passwordSchema" class="form" @submit="onChangePassword">
            <div class="field">
              <label>Mật khẩu hiện tại</label>
              <Field name="CurrentPassword" v-slot="{ field }">
                <PPassword
                  :model-value="field.value"
                  :feedback="false"
                  toggle-mask
                  @update:model-value="field.onChange"
                  @blur="field.onBlur"
                />
              </Field>
              <ErrorMessage name="CurrentPassword" class="error" />
            </div>
            <div class="field">
              <label>Mật khẩu mới</label>
              <Field name="NewPassword" v-slot="{ field }">
                <PPassword
                  :model-value="field.value"
                  toggle-mask
                  @update:model-value="field.onChange"
                  @blur="field.onBlur"
                />
              </Field>
              <ErrorMessage name="NewPassword" class="error" />
            </div>
            <div class="field">
              <label>Xác nhận mật khẩu</label>
              <Field name="ConfirmPassword" v-slot="{ field }">
                <PPassword
                  :model-value="field.value"
                  :feedback="false"
                  toggle-mask
                  @update:model-value="field.onChange"
                  @blur="field.onBlur"
                />
              </Field>
              <ErrorMessage name="ConfirmPassword" class="error" />
            </div>
            <PButton label="Cập nhật mật khẩu" type="submit" class="p-button-warning" :loading="isPasswordLoading" />
          </Form>
        </template>
      </PCard>
    </div>

    <PDialog
      v-model:visible="showDialog"
      :draggable="false"
      :style="{ width: 'min(720px, 92vw)' }"
      modal
      closable
    >
      <template #header>
        <div class="dialog-header">
          <h3>Chỉnh sửa hồ sơ</h3>
          <p>Cập nhật thông tin để cá nhân hóa trải nghiệm.</p>
        </div>
      </template>
      <div class="dialog-hero"></div>
      <div class="dialog-avatar">
        <div class="avatar-wrap">
          <img
            :src="avatarPreview || 'https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=400&q=80'"
            alt="Ảnh đại diện"
          />
          <button class="avatar-action" type="button" @click="onPickAvatar">
            <i class="pi pi-image"></i>
          </button>
          <input ref="fileInput" type="file" accept="image/*" class="hidden-input" @change="onAvatarChange" />
        </div>
      </div>

      <div class="dialog-body">
        <Form
          :key="dialogKey"
          :validation-schema="profileSchema"
          :initial-values="dialogValues"
          class="form"
          @submit="onDialogSave"
        >
          <div class="row">
            <div class="field">
              <label>Họ tên</label>
              <Field name="FullName" v-slot="{ field }">
                <PInputText v-bind="field" />
              </Field>
              <ErrorMessage name="FullName" class="error" />
            </div>
            <div class="field">
              <label>Email</label>
              <PInputText :model-value="email" disabled />
            </div>
          </div>
          <div class="row">
            <div class="field">
              <label>Số điện thoại</label>
              <Field name="Phone" v-slot="{ field }">
                <PInputText v-bind="field" />
              </Field>
              <ErrorMessage name="Phone" class="error" />
            </div>
            <div class="field">
              <label>Địa chỉ</label>
              <Field name="Address" v-slot="{ field }">
                <PInputText v-bind="field" />
              </Field>
              <ErrorMessage name="Address" class="error" />
            </div>
          </div>
          <div class="row">
            <div class="field full">
              <label>Ảnh đại diện (URL)</label>
              <Field name="AvatarUrl" v-slot="{ field }">
                <PInputText v-bind="field" />
              </Field>
              <ErrorMessage name="AvatarUrl" class="error" />
            </div>
          </div>
          <div class="dialog-footer">
            <button class="ghost-btn" type="button" @click="showDialog = false">Hủy</button>
            <PButton label="Lưu thay đổi" type="submit" class="p-button-warning" :loading="isLoading" />
          </div>
        </Form>
      </div>
    </PDialog>

    <div v-if="showSignoutConfirm" class="alert-overlay" @click.self="showSignoutConfirm = false">
      <div class="alert-card" role="dialog" aria-modal="true" aria-labelledby="signout-title">
        <div class="alert-header">
          <div class="alert-title">
            <i class="pi pi-sign-out"></i>
            <h4 id="signout-title">Xác nhận đăng xuất</h4>
          </div>
          <button class="icon-btn" type="button" @click="showSignoutConfirm = false" aria-label="Đóng">
            <i class="pi pi-times"></i>
          </button>
        </div>
        <p class="alert-desc">Bạn muốn đăng xuất khỏi tài khoản này?</p>
        <div class="alert-actions">
          <button class="ghost-btn" type="button" @click="showSignoutConfirm = false">Hủy</button>
          <button class="primary-btn" type="button" @click="onConfirmSignOut">Đăng xuất</button>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.profile {
  padding: 32px 0 48px;
}

.profile-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 20px;
  gap: 16px;
}

.profile-header h2 {
  font-family: 'Fraunces', serif;
  font-size: clamp(1.8rem, 3vw, 2.4rem);
}

.profile-header p {
  color: var(--muted);
}

.profile-grid {
  display: grid;
  gap: 20px;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
}

.panel {
  background: var(--surface);
  border: 1px solid rgba(31, 19, 11, 0.08);
  box-shadow: var(--shadow);
}

.profile-card {
  border-radius: var(--radius);
  overflow: hidden;
}

.profile-hero {
  height: 120px;
  background: radial-gradient(circle at top left, rgba(255, 107, 53, 0.5), rgba(245, 158, 11, 0.35));
}

.profile-body {
  padding: 0 20px 20px;
}

.profile-avatar {
  margin-top: -44px;
  width: 88px;
  height: 88px;
  border-radius: 999px;
  border: 4px solid var(--surface);
  overflow: hidden;
  box-shadow: var(--shadow);
}

.profile-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.profile-info {
  margin-top: 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.profile-info h3 {
  font-size: 1.2rem;
}

.profile-info p {
  color: var(--muted);
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

.profile-grid-info {
  margin-top: 16px;
  display: grid;
  gap: 12px;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
}

.profile-grid-info span {
  color: var(--muted);
  font-size: 0.85rem;
}

.profile-grid-info strong {
  display: block;
  margin-top: 4px;
}

.meta {
  display: flex;
  justify-content: space-between;
  background: var(--surface-2);
  padding: 10px 12px;
  border-radius: var(--radius);
  margin-bottom: 16px;
}

.form {
  display: grid;
  gap: 14px;
}

.row {
  display: grid;
  gap: 14px;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.field.full {
  grid-column: 1 / -1;
}

.field {
  display: grid;
  gap: 6px;
}

label {
  font-weight: 600;
}

.error {
  color: #ef4444;
  font-size: 0.8rem;
}

.signout-btn {
  color: #fff;
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
  margin: 0 18px;
  background: radial-gradient(circle at top left, rgba(255, 107, 53, 0.45), rgba(245, 158, 11, 0.3));
}

.dialog-avatar {
  display: flex;
  justify-content: center;
  margin-top: -46px;
}

.avatar-wrap {
  position: relative;
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

.avatar-action {
  position: absolute;
  right: -2px;
  bottom: -2px;
  width: 30px;
  height: 30px;
  border-radius: 999px;
  border: none;
  background: rgba(31, 19, 11, 0.7);
  color: #fff;
  cursor: pointer;
}

.hidden-input {
  display: none;
}

.dialog-body {
  padding: 16px 18px 20px;
}


.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 8px;
}

.alert-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 10, 6, 0.35);
  display: grid;
  place-items: center;
  z-index: 60;
}

.alert-card {
  width: min(360px, 92vw);
  background: var(--surface);
  border-radius: 16px;
  border: 1px solid rgba(31, 19, 11, 0.12);
  box-shadow: var(--shadow);
  padding: 16px;
  display: grid;
  gap: 12px;
}

.alert-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.alert-title {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  font-weight: 600;
}

.alert-desc {
  color: var(--muted);
  font-size: 0.9rem;
}

.alert-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.icon-btn {
  border: none;
  background: transparent;
  cursor: pointer;
  color: var(--muted);
}

.primary-btn {
  border: none;
  background: var(--accent);
  color: #fff;
  padding: 8px 14px;
  border-radius: 999px;
  font-weight: 600;
  cursor: pointer;
}

.primary-btn:hover {
  background: var(--accent-2);
}
</style>
