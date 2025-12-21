/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_GOOGLE_CLIENT_ID?: string
  readonly VITE_FACEBOOK_APP_ID?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

interface Window {
  google?: {
    accounts?: {
      id?: {
        initialize: (options: { client_id: string; callback: (response: { credential?: string }) => void }) => void
        prompt: () => void
      }
    }
  }
  FB?: {
    init: (options: { appId: string; cookie: boolean; xfbml: boolean; version: string }) => void
    login: (callback: (response: { authResponse?: { accessToken?: string } }) => void, options: { scope: string }) => void
  }
  fbAsyncInit?: () => void
}
