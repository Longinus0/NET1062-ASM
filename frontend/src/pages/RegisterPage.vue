<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import * as yup from 'yup'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '../stores/auth'
import RegisterForm from '../components/ui/RegisterForm.vue'

const router = useRouter()
const toast = useToast()
const authStore = useAuthStore()
const googleInitialized = ref(false)
const facebookInitialized = ref(false)

const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID
const facebookAppId = import.meta.env.VITE_FACEBOOK_APP_ID

const schema = yup.object({
  FullName: yup.string().min(3, 'Tối thiểu 3 ký tự').required('Vui lòng nhập họ tên'),
  Email: yup.string().email('Email không hợp lệ').required('Vui lòng nhập email'),
  Phone: yup.string().nullable(),
  Address: yup.string().nullable(),
  AvatarUrl: yup.string().url('URL không hợp lệ').nullable(),
  Password: yup.string().min(6, 'Mật khẩu tối thiểu 6 ký tự').required('Vui lòng nhập mật khẩu'),
  ConfirmPassword: yup
    .string()
    .oneOf([yup.ref('Password')], 'Mật khẩu không khớp')
    .required('Vui lòng xác nhận mật khẩu'),
})

const onSubmit = async (values: any) => {
  try {
    await authStore.registerUser(values)
    toast.add({ severity: 'success', summary: 'Đăng ký thành công', detail: 'Bạn đã sẵn sàng đặt món!', life: 2500 })
    router.push('/profile')
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Đăng ký thất bại',
      detail: err?.response?.data?.message || 'Không thể đăng ký tài khoản.',
      life: 3000,
    })
  }
}

const handleOAuthSuccess = async () => {
  toast.add({ severity: 'success', summary: 'Đăng ký thành công', detail: 'Bạn đã sẵn sàng đặt món!', life: 2500 })
  router.push('/profile')
}

const handleOAuthError = (err: any) => {
  toast.add({
    severity: 'error',
    summary: 'Đăng ký thất bại',
    detail: err?.response?.data?.message || 'Không thể đăng ký tài khoản.',
    life: 3000,
  })
}

const initGoogle = () => {
  if (!googleClientId || googleInitialized.value || !window.google?.accounts?.id) return
  window.google.accounts.id.initialize({
    client_id: googleClientId,
    callback: async (response: { credential?: string }) => {
      if (!response.credential) {
        handleOAuthError(null)
        return
      }
      try {
        await authStore.loginWithGoogleOAuth(response.credential)
        await handleOAuthSuccess()
      } catch (err: any) {
        handleOAuthError(err)
      }
    },
  })
  googleInitialized.value = true
}

const initFacebook = () => {
  if (!facebookAppId || facebookInitialized.value) return
  if (!window.FB) {
    window.fbAsyncInit = () => {
      window.FB.init({
        appId: facebookAppId,
        cookie: true,
        xfbml: false,
        version: 'v18.0',
      })
      facebookInitialized.value = true
    }
    return
  }
  window.FB.init({
    appId: facebookAppId,
    cookie: true,
    xfbml: false,
    version: 'v18.0',
  })
  facebookInitialized.value = true
}

const registerWithGoogle = () => {
  if (!googleClientId) {
    toast.add({ severity: 'warn', summary: 'Thiếu Google Client ID', life: 2500 })
    return
  }
  initGoogle()
  if (!window.google?.accounts?.id) {
    toast.add({ severity: 'warn', summary: 'Google SDK chưa sẵn sàng', life: 2500 })
    return
  }
  window.google.accounts.id.prompt()
}

const registerWithFacebook = () => {
  if (!facebookAppId) {
    toast.add({ severity: 'warn', summary: 'Thiếu Facebook App ID', life: 2500 })
    return
  }
  initFacebook()
  if (!window.FB) {
    toast.add({ severity: 'warn', summary: 'Facebook SDK chưa sẵn sàng', life: 2500 })
    return
  }
  window.FB.login(
    async (response: { authResponse?: { accessToken?: string } }) => {
      const accessToken = response.authResponse?.accessToken
      if (!accessToken) {
        handleOAuthError(null)
        return
      }
      try {
        await authStore.loginWithFacebookOAuth(accessToken)
        await handleOAuthSuccess()
      } catch (err: any) {
        handleOAuthError(err)
      }
    },
    { scope: 'email,public_profile' },
  )
}

onMounted(() => {
  initGoogle()
  initFacebook()
})
</script>

<template>
  <section class="container auth">
    <RegisterForm
      :schema="schema"
      heading="Đăng ký"
      subheading="Tạo tài khoản mới chỉ trong vài phút."
      @submit="onSubmit"
      @google="registerWithGoogle"
      @facebook="registerWithFacebook"
    >
      <template #footer>
        <div class="auth-footer">
          <span>Đã có tài khoản?</span>
          <PButton label="Đăng nhập" class="p-button-text" @click="router.push('/login')" />
        </div>
      </template>
    </RegisterForm>
  </section>
</template>

<style scoped>
.auth {
  padding: 32px 0 64px;
  display: flex;
  justify-content: center;
}

.auth-footer {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
  color: var(--muted);
}
</style>
