import api from './api'

export interface AuthUser {
  UserId: number
  FullName: string
  Email: string
  Phone?: string | null
  Address?: string | null
  AvatarUrl?: string | null
  ForcePasswordReset?: number
}

export interface UserRole {
  RoleName: string
}

export interface LoginPayload {
  Email: string
  Password: string
}

export interface RegisterPayload {
  FullName: string
  Email: string
  Password: string
  Phone?: string | null
  Address?: string | null
  AvatarUrl?: string | null
}

export const login = async (payload: LoginPayload) => {
  const { data } = await api.post<AuthUser & { token?: string }>('/auth/login', payload)
  return data
}

export const register = async (payload: RegisterPayload) => {
  const { data } = await api.post<AuthUser & { token?: string }>('/auth/register', payload)
  return data
}

export const loginWithGoogle = async (idToken: string) => {
  const { data } = await api.post<AuthUser>('/auth/oauth/google', { IdToken: idToken })
  return data
}

export const loginWithFacebook = async (accessToken: string) => {
  const { data } = await api.post<AuthUser>('/auth/oauth/facebook', { AccessToken: accessToken })
  return data
}

export const getProfile = async (userId: number) => {
  const { data } = await api.get<AuthUser>(`/users/${userId}`)
  return data
}

export const updateProfile = async (userId: number, payload: Omit<AuthUser, 'UserId' | 'Email'>) => {
  await api.put(`/users/${userId}`, payload)
}

export const getUserRole = async (userId: number) => {
  const { data } = await api.get<UserRole>(`/users/${userId}/role`)
  return data
}

export const changePassword = async (userId: number, payload: { CurrentPassword: string; NewPassword: string }) => {
  await api.put(`/users/${userId}/password`, payload)
}
