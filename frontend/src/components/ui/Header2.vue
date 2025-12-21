<script setup lang="ts">
import { onUnmounted, ref, watch } from 'vue'
import MenuToggleIcon from './MenuToggleIcon.vue'
import WordmarkIcon from './WordmarkIcon.vue'
import { useScroll } from './useScroll'

type NavLink = {
  label: string
  to: string
}

const props = withDefaults(
  defineProps<{
    links: NavLink[]
    signInTo: string
    ctaTo: string
    showCartButton?: boolean
    cartCount?: number
  }>(),
  {
    showCartButton: false,
    cartCount: 0,
  },
)

const emit = defineEmits<{
  (event: 'cart'): void
}>()

const open = ref(false)
const scrolled = useScroll(10)

watch(open, (value) => {
  document.body.style.overflow = value ? 'hidden' : ''
})

onUnmounted(() => {
  document.body.style.overflow = ''
})
</script>

<template>
  <header class="nav-wrap" :class="{ 'nav-wrap-scrolled': scrolled && !open, 'nav-wrap-open': open }">
    <div class="nav-shell">
      <nav class="nav">
        <RouterLink class="brand" to="/">
          <WordmarkIcon class="brand-mark" />
        </RouterLink>
        <div class="nav-links">
          <RouterLink v-for="link in props.links" :key="link.label" :to="link.to" class="nav-link">
            {{ link.label }}
          </RouterLink>
        </div>
        <div class="nav-actions">
          <RouterLink class="btn btn-outline" :to="props.signInTo">Sign In</RouterLink>
          <RouterLink class="btn btn-solid" :to="props.ctaTo">Get Started</RouterLink>
          <button
            v-if="props.showCartButton"
            class="btn btn-icon"
            type="button"
            @click="emit('cart')"
            aria-label="Open cart"
          >
            <i class="pi pi-shopping-cart"></i>
            <span v-if="props.cartCount" class="cart-count">{{ props.cartCount }}</span>
          </button>
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
                v-for="link in props.links"
                :key="link.label"
                :to="link.to"
                class="nav-link"
                @click="open = false"
              >
                {{ link.label }}
              </RouterLink>
            </div>
            <div class="mobile-actions">
              <RouterLink class="btn btn-outline" :to="props.signInTo" @click="open = false">
                Sign In
              </RouterLink>
              <RouterLink class="btn btn-solid" :to="props.ctaTo" @click="open = false">
                Get Started
              </RouterLink>
              <button
                v-if="props.showCartButton"
                class="btn btn-outline"
                type="button"
                @click="() => {
                  emit('cart')
                  open = false
                }"
              >
                Cart
              </button>
            </div>
          </div>
        </div>
      </transition>
    </teleport>
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
  background: rgba(255, 255, 255, 0.92);
  border-color: rgba(15, 15, 15, 0.14);
  box-shadow: 0 14px 28px rgba(15, 15, 15, 0.08);
  backdrop-filter: blur(12px);
}

.nav-wrap-open .nav-shell {
  background: rgba(255, 255, 255, 0.96);
}

.nav-shell {
  margin: 0 auto;
  max-width: 1100px;
  border: 1px solid transparent;
  border-radius: var(--radius);
  background: rgba(255, 255, 255, 0.85);
  transition: all 0.25s ease;
}

.nav {
  height: 56px;
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
  display: flex;
  align-items: center;
}

.brand-mark {
  height: 18px;
}

.nav-links {
  display: none;
  align-items: center;
  gap: 14px;
}

.nav-link {
  font-size: 0.88rem;
  color: var(--text);
  padding: 6px 10px;
  border-radius: var(--radius);
  transition: background 0.2s ease;
}

.nav-link:hover {
  background: rgba(15, 15, 15, 0.06);
}

.nav-actions {
  display: none;
  align-items: center;
  gap: 10px;
}

.btn {
  border-radius: var(--radius);
  font-size: 0.85rem;
  padding: 8px 14px;
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-outline {
  background: #fff;
  border-color: rgba(15, 15, 15, 0.12);
}

.btn-outline:hover {
  background: rgba(15, 15, 15, 0.06);
}

.btn-solid {
  background: #1f1f1f;
  color: #fff;
}

.btn-solid:hover {
  background: #2d2d2d;
}

.btn-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  padding: 0;
  position: relative;
  background: #fff;
  border-color: rgba(15, 15, 15, 0.12);
}

.cart-count {
  position: absolute;
  top: -6px;
  right: -4px;
  background: #111;
  color: #fff;
  font-size: 0.65rem;
  padding: 2px 6px;
  border-radius: var(--radius);
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
  top: 56px;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.96);
  border-top: 1px solid rgba(15, 15, 15, 0.12);
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
    height: 52px;
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
