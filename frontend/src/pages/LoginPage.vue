<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import * as yup from 'yup'
import { useToast } from 'primevue/usetoast'
import { useAuthStore } from '../stores/auth'
import LoginForm from '../components/ui/LoginForm.vue'

const router = useRouter()
const route = useRoute()
const toast = useToast()
const authStore = useAuthStore()
const googleInitialized = ref(false)
const facebookInitialized = ref(false)

const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID
const facebookAppId = import.meta.env.VITE_FACEBOOK_APP_ID

const schema = yup.object({
  Email: yup.string().email('Email không hợp lệ').required('Vui lòng nhập email'),
  Password: yup.string().min(6, 'Mật khẩu tối thiểu 6 ký tự').required('Vui lòng nhập mật khẩu'),
})

type LoginForm = yup.InferType<typeof schema>

const onSubmit = async (values: Record<string, unknown>) => {
  const payload = values as LoginForm
  try {
    await authStore.loginUser(payload)
    toast.add({ severity: 'success', summary: 'Đăng nhập thành công', detail: 'Chào mừng bạn quay lại!', life: 2500 })
    router.push((route.query.redirect as string) || '/profile')
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Đăng nhập thất bại',
      detail: err?.response?.data?.message || 'Sai email hoặc mật khẩu.',
      life: 3000,
    })
  }
}

const handleOAuthSuccess = async () => {
  toast.add({ severity: 'success', summary: 'Đăng nhập thành công', detail: 'Chào mừng bạn!', life: 2500 })
  router.push((route.query.redirect as string) || '/profile')
}

const handleOAuthError = (err: any) => {
  toast.add({
    severity: 'error',
    summary: 'Đăng nhập thất bại',
    detail: err?.response?.data?.message || 'Vui lòng thử lại.',
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

const loginWithGoogle = () => {
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

const loginWithFacebook = () => {
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
    <LoginForm
      :schema="schema"
      heading="Đăng nhập"
      subheading="Chào mừng bạn quay lại. Hãy đăng nhập để tiếp tục."
      @submit="onSubmit"
      @google="loginWithGoogle"
      @facebook="loginWithFacebook"
    >
      <template #footer>
        <div class="auth-footer">
          <span>Chưa có tài khoản?</span>
          <PButton label="Đăng ký" class="p-button-text" @click="router.push('/register')" />
        </div>
      </template>
    </LoginForm>
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
