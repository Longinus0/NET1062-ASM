import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { AuthUser, LoginPayload, RegisterPayload } from '../services/auth'
import { getProfile, getUserRole, login, loginWithFacebook, loginWithGoogle, register, updateProfile } from '../services/auth'

const USER_KEY = 'auth_user'
const TOKEN_KEY = 'auth_token'
const ROLE_KEY = 'auth_role'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthUser | null>(null)
  const token = ref<string | null>(null)
  const roleName = ref<string | null>(null)

  const isAuthenticated = computed(() => !!user.value)
  const isAdmin = computed(() => {
    if (localStorage.getItem('auth_is_admin') === 'true') return true
    if (roleName.value) {
      return ['Admin', 'Quản trị', 'Quan tri'].includes(roleName.value)
    }
    return !!user.value?.Email?.toLowerCase().includes('admin')
  })

  const resolveRole = async () => {
    if (!user.value) return
    try {
      const role = await getUserRole(user.value.UserId)
      roleName.value = role.RoleName
      localStorage.setItem(ROLE_KEY, role.RoleName)
      localStorage.setItem('auth_is_admin', isAdmin.value ? 'true' : 'false')
    } catch {
      // Keep cached role if API fails
    }
  }

  const loadUserFromToken = async () => {
    const storedUser = localStorage.getItem(USER_KEY)
    const storedRole = localStorage.getItem(ROLE_KEY)
    if (!storedUser) {
      return
    }
    const storedToken = localStorage.getItem(TOKEN_KEY)
    token.value = storedToken
    user.value = JSON.parse(storedUser) as AuthUser
    roleName.value = storedRole
    try {
      const fresh = await getProfile(user.value.UserId)
      user.value = fresh
      localStorage.setItem(USER_KEY, JSON.stringify(fresh))
    } catch {
      // Keep local data if API fails
    }
    await resolveRole()
  }

  const loginUser = async (payload: LoginPayload) => {
    const data = await login(payload)
    user.value = data
    token.value = data.token ?? null
    localStorage.setItem(USER_KEY, JSON.stringify(data))
    if (data.token) {
      localStorage.setItem(TOKEN_KEY, data.token)
    }
    await resolveRole()
  }

  const registerUser = async (payload: RegisterPayload) => {
    const data = await register(payload)
    user.value = data
    token.value = data.token ?? null
    localStorage.setItem(USER_KEY, JSON.stringify(data))
    if (data.token) {
      localStorage.setItem(TOKEN_KEY, data.token)
    }
    await resolveRole()
  }

  const loginWithGoogleOAuth = async (idToken: string) => {
    const data = await loginWithGoogle(idToken)
    user.value = data
    token.value = null
    localStorage.setItem(USER_KEY, JSON.stringify(data))
    localStorage.removeItem(TOKEN_KEY)
    await resolveRole()
  }

  const loginWithFacebookOAuth = async (accessToken: string) => {
    const data = await loginWithFacebook(accessToken)
    user.value = data
    token.value = null
    localStorage.setItem(USER_KEY, JSON.stringify(data))
    localStorage.removeItem(TOKEN_KEY)
    await resolveRole()
  }

  const logout = () => {
    user.value = null
    token.value = null
    roleName.value = null
    localStorage.removeItem(USER_KEY)
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(ROLE_KEY)
    localStorage.removeItem('auth_is_admin')
  }

  const setAdminFlag = (value: boolean) => {
    localStorage.setItem('auth_is_admin', value ? 'true' : 'false')
  }

  const saveProfile = async (payload: Omit<AuthUser, 'UserId' | 'Email'>) => {
    if (!user.value) return
    await updateProfile(user.value.UserId, payload)
    const updated = { ...user.value, ...payload }
    user.value = updated
    localStorage.setItem(USER_KEY, JSON.stringify(updated))
  }

  return {
    user,
    token,
    roleName,
    isAuthenticated,
    isAdmin,
    loadUserFromToken,
    loginUser,
    registerUser,
    loginWithGoogleOAuth,
    loginWithFacebookOAuth,
    logout,
    saveProfile,
    setAdminFlag,
  }
})
