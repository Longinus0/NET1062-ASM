<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import MenuToggleIcon from './ui/MenuToggleIcon.vue'
import Avatar from './ui/Avatar.vue'
import { useScroll } from './ui/useScroll'
import { useCartStore } from '../stores/cart'
import { useAuthStore } from '../stores/auth'
import CartDrawer from './CartDrawer.vue'
import { getCategories } from '../services/catalog'
import type { CategoryDTO } from '../types/dtos'

const router = useRouter()
const cart = useCartStore()
const auth = useAuthStore()
const showCart = ref(false)
const open = ref(false)
const scrolled = useScroll(10)
const categories = ref<CategoryDTO[]>([])

const navLinks = computed(() => [
  { label: 'Trang chủ', to: '/' },
  { label: 'Thực đơn', to: '/menu', dropdown: true },
  { label: 'Tìm kiếm', to: '/search' },
  ...(auth.isAuthenticated ? [{ label: 'Đơn hàng', to: '/orders' }] : []),
  ...(auth.isAdmin ? [{ label: 'Quản trị', to: '/admin' }] : []),
])

const userInitials = computed(() => {
  const name = auth.user?.FullName?.trim()
  if (!name) return 'U'
  const parts = name.split(/\s+/).slice(0, 2)
  return parts.map((part) => part[0]?.toUpperCase()).join('')
})

const loadCategories = async () => {
  try {
    categories.value = await getCategories()
  } catch {
    categories.value = []
  }
}

watch(open, (value) => {
  document.body.style.overflow = value ? 'hidden' : ''
})

onMounted(loadCategories)

onUnmounted(() => {
  document.body.style.overflow = ''
})
</script>

<template>
  <header class="nav-wrap" :class="{ 'nav-wrap-scrolled': scrolled && !open, 'nav-wrap-open': open }">
    <div class="nav-shell">
      <nav class="nav">
        <button class="brand" type="button" @click="router.push('/')">
          <span class="brand-dot"></span>
          <span class="brand-text">
            <span class="brand-name">Saigon Fast</span>
            <span class="brand-sub">Fast Food · Fresh Mood</span>
          </span>
        </button>
        <div class="nav-links">
          <template v-for="link in navLinks" :key="link.label">
            <div v-if="link.dropdown" class="nav-dropdown">
              <RouterLink :to="link.to" class="nav-link">
                {{ link.label }}
                <i class="pi pi-angle-down dropdown-icon"></i>
              </RouterLink>
              <div class="dropdown-panel">
                <RouterLink class="dropdown-link" to="/menu">Tất cả món</RouterLink>
                <RouterLink class="dropdown-link" to="/combos">Combo siêu lợi</RouterLink>
                <div v-if="categories.length" class="dropdown-divider"></div>
                <RouterLink
                  v-for="category in categories"
                  :key="category.CategoryId"
                  class="dropdown-link"
                  :to="{ path: '/menu', query: { category: category.CategoryId } }"
                >
                  {{ category.Name }}
                </RouterLink>
              </div>
            </div>
            <RouterLink v-else :to="link.to" class="nav-link">
              {{ link.label }}
            </RouterLink>
          </template>
        </div>
        <div class="nav-actions">
          <button
            v-if="!auth.isAuthenticated"
            class="btn btn-outline"
            type="button"
            @click="router.push('/login')"
          >
            Đăng nhập
          </button>
          <button class="btn btn-solid" type="button" @click="router.push('/menu')">Đặt món</button>
          <button class="btn btn-icon" type="button" @click="showCart = true" aria-label="Open cart">
            <i class="pi pi-shopping-cart"></i>
            <span v-if="cart.itemCount" class="cart-count">{{ cart.itemCount }}</span>
          </button>
          <RouterLink v-if="auth.isAuthenticated" to="/profile" class="avatar-link">
            <Avatar :src="auth.user?.AvatarUrl" :alt="auth.user?.FullName" :fallback="userInitials" />
          </RouterLink>
        </div>
        <button class="btn btn-icon mobile-toggle" type="button" @click="open = !open" aria-label="Toggle menu">
          <MenuToggleIcon :open="open" class="menu-icon" :duration="300" />
        </button>
      </nav>
    </div>

    <teleport to="body">
      <transition name="fade-zoom">
        <div v-if="open" class="mobile-menu">
          <div class="mobile-panel">
            <div class="mobile-links">
              <RouterLink
                v-for="link in navLinks"
                :key="link.label"
                :to="link.to"
                class="nav-link"
                @click="open = false"
              >
                {{ link.label }}
              </RouterLink>
              <div v-if="categories.length" class="mobile-divider"></div>
              <RouterLink class="nav-link" to="/menu" @click="open = false">Tất cả món</RouterLink>
              <RouterLink class="nav-link" to="/combos" @click="open = false">
                Combo siêu lợi
              </RouterLink>
              <RouterLink
                v-for="category in categories"
                :key="category.CategoryId"
                class="nav-link"
                :to="{ path: '/menu', query: { category: category.CategoryId } }"
                @click="open = false"
              >
                {{ category.Name }}
              </RouterLink>
            </div>
            <div class="mobile-actions">
              <button
                class="btn btn-outline"
                type="button"
                @click="() => {
                  router.push(auth.isAuthenticated ? '/profile' : '/login')
                  open = false
                }"
              >
                {{ auth.isAuthenticated ? 'Tài khoản' : 'Đăng nhập' }}
              </button>
              <button
                class="btn btn-solid"
                type="button"
                @click="() => {
                  router.push('/menu')
                  open = false
                }"
              >
                Đặt món
              </button>
              <button
                class="btn btn-outline"
                type="button"
                @click="() => {
                  showCart = true
                  open = false
                }"
              >
                Giỏ hàng
              </button>
            </div>
          </div>
        </div>
      </transition>
    </teleport>
    <CartDrawer v-model:visible="showCart" />
  </header>
</template>

<style scoped>
.nav-wrap {
  position: sticky;
  top: 0;
  z-index: 50;
  padding: 10px 12px;
}

.nav-wrap-scrolled .nav-shell {
  background: rgba(255, 250, 245, 0.92);
  border-color: rgba(31, 19, 11, 0.14);
  box-shadow: 0 14px 28px rgba(17, 12, 8, 0.08);
  backdrop-filter: blur(12px);
}

.nav-wrap-open .nav-shell {
  background: rgba(255, 250, 245, 0.96);
}

.nav-shell {
  margin: 0 auto;
  width: min(1200px, 92vw);
  max-width: 1200px;
  border: 1px solid transparent;
  border-radius: var(--radius);
  background: rgba(255, 250, 245, 0.85);
  transition: all 0.25s ease;
}

.nav {
  height: 58px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 18px;
  gap: 16px;
}

.brand {
  border: none;
  background: transparent;
  cursor: pointer;
  padding: 6px;
  display: inline-flex;
  align-items: center;
  gap: 12px;
}

.brand-dot {
  width: 12px;
  height: 12px;
  border-radius: var(--radius);
  background: linear-gradient(140deg, var(--accent), var(--accent-2));
  box-shadow: 0 0 0 6px rgba(255, 107, 53, 0.2);
}

.brand-text {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  line-height: 1.1;
}

.brand-name {
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  font-size: 0.9rem;
}

.brand-sub {
  font-size: 0.72rem;
  color: var(--muted);
}

.nav-links {
  display: none;
  align-items: center;
  gap: 12px;
}

.nav-dropdown {
  position: relative;
}

.dropdown-icon {
  font-size: 0.7rem;
  margin-left: 6px;
  opacity: 0.7;
}

.dropdown-panel {
  position: absolute;
  top: calc(100% + 10px);
  left: 0;
  min-width: 200px;
  padding: 10px;
  border-radius: var(--radius);
  background: var(--surface);
  border: 1px solid rgba(31, 19, 11, 0.12);
  box-shadow: 0 12px 28px rgba(17, 12, 8, 0.12);
  display: grid;
  gap: 6px;
  opacity: 0;
  transform: translateY(-6px);
  pointer-events: none;
  transition: all 0.2s ease;
  z-index: 20;
}

.nav-dropdown:hover .dropdown-panel,
.nav-dropdown:focus-within .dropdown-panel {
  opacity: 1;
  transform: translateY(0);
  pointer-events: auto;
}

.dropdown-link {
  font-size: 0.84rem;
  color: var(--text);
  padding: 6px 10px;
  border-radius: var(--radius);
  transition: background 0.2s ease;
}

.dropdown-link:hover {
  background: rgba(31, 19, 11, 0.08);
}

.dropdown-divider {
  height: 1px;
  background: rgba(31, 19, 11, 0.08);
  margin: 4px 2px;
}

.nav-link {
  font-size: 0.86rem;
  color: var(--text);
  padding: 6px 12px;
  border-radius: var(--radius);
  transition: background 0.2s ease;
}

.nav-link:hover {
  background: rgba(31, 19, 11, 0.08);
}

.nav-actions {
  display: none;
  align-items: center;
  gap: 10px;
}

.btn {
  border-radius: var(--radius);
  font-size: 0.82rem;
  padding: 8px 14px;
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.2s ease;
  color: var(--text);
  background: transparent;
}

.btn-outline {
  background: var(--surface);
  border-color: rgba(31, 19, 11, 0.16);
}

.btn-outline:hover {
  background: rgba(31, 19, 11, 0.06);
}

.btn-solid {
  background: var(--accent);
  color: #fff;
}

.btn-solid:hover {
  background: #ff8355;
}

.btn-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  padding: 0;
  position: relative;
  background: var(--surface);
  border-color: rgba(31, 19, 11, 0.16);
}

.cart-count {
  position: absolute;
  top: -6px;
  right: -4px;
  background: var(--text);
  color: #fff;
  font-size: 0.65rem;
  padding: 2px 6px;
  border-radius: var(--radius);
}

.avatar-link {
  display: inline-flex;
  align-items: center;
}

.menu-icon {
  width: 20px;
  height: 20px;
}

.mobile-toggle {
  margin-left: auto;
}

.mobile-menu {
  position: fixed;
  top: 60px;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 250, 245, 0.96);
  border-top: 1px solid rgba(31, 19, 11, 0.12);
  backdrop-filter: blur(12px);
  z-index: 40;
}

.mobile-panel {
  height: 100%;
  padding: 18px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 12px;
}

.mobile-links {
  display: grid;
  gap: 10px;
}

.mobile-divider {
  height: 1px;
  background: rgba(31, 19, 11, 0.12);
}

.mobile-actions {
  display: grid;
  gap: 10px;
}

.fade-zoom-enter-active,
.fade-zoom-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.fade-zoom-enter-from,
.fade-zoom-leave-to {
  opacity: 0;
  transform: scale(0.97);
}

@media (min-width: 768px) {
  .nav-wrap {
    padding: 12px 18px;
  }

  .nav {
    height: 56px;
  }

  .nav-links,
  .nav-actions {
    display: flex;
  }

  .mobile-toggle {
    display: none;
  }
}
</style>
