<script setup lang="ts">
import { ref, watch } from 'vue'
import MenuToggleIcon from './MenuToggleIcon.vue'
import WordmarkIcon from './WordmarkIcon.vue'
import { useScroll } from './useScroll'

const open = ref(false)
const scrolled = useScroll(10)

const links = [
  { label: 'Features', href: '#' },
  { label: 'Pricing', href: '#' },
  { label: 'About', href: '#' },
]

watch(open, (value) => {
  document.body.style.overflow = value ? 'hidden' : ''
})
</script>

<template>
  <header
    class="header"
    :class="{ 'header-scrolled': scrolled }"
  >
    <nav class="nav">
      <div class="brand">
        <WordmarkIcon />
      </div>
      <div class="nav-links">
        <a v-for="link in links" :key="link.label" :href="link.href" class="link">
          {{ link.label }}
        </a>
        <PButton class="p-button-outlined">Sign In</PButton>
        <PButton class="p-button-warning">Get Started</PButton>
      </div>
      <PButton
        size="icon"
        class="p-button-outlined mobile-toggle"
        @click="open = !open"
        aria-label="Toggle menu"
      >
        <MenuToggleIcon :open="open" class="icon" :duration="300" />
      </PButton>
    </nav>

    <teleport to="body">
      <div v-if="open" class="mobile-menu">
        <div class="mobile-panel">
          <div class="mobile-links">
            <a v-for="link in links" :key="link.label" :href="link.href" class="link">
              {{ link.label }}
            </a>
          </div>
          <div class="mobile-actions">
            <PButton class="p-button-outlined">Sign In</PButton>
            <PButton class="p-button-warning">Get Started</PButton>
          </div>
        </div>
      </div>
    </teleport>
  </header>
</template>

<style scoped>
.header {
  position: sticky;
  top: 0;
  z-index: 50;
  border-bottom: 1px solid transparent;
}

.header-scrolled {
  background: rgba(255, 250, 245, 0.85);
  border-color: rgba(31, 19, 11, 0.12);
  backdrop-filter: blur(12px);
}

.nav {
  margin: 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  max-width: 1024px;
  padding: 0 16px;
  height: 56px;
}

.brand {
  display: flex;
  align-items: center;
  padding: 6px;
  border-radius: var(--radius);
}

.nav-links {
  display: none;
  align-items: center;
  gap: 8px;
}

.link {
  padding: 6px 10px;
  border-radius: var(--radius);
  font-size: 0.9rem;
  text-decoration: none;
  color: var(--text);
}

.link:hover {
  background: rgba(255, 107, 53, 0.1);
}

.mobile-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
}

.icon {
  width: 20px;
  height: 20px;
}

.mobile-menu {
  position: fixed;
  top: 56px;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 40;
  background: rgba(255, 250, 245, 0.95);
  backdrop-filter: blur(12px);
  border-top: 1px solid rgba(31, 19, 11, 0.12);
}

.mobile-panel {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.mobile-links {
  display: grid;
  gap: 8px;
}

.mobile-actions {
  display: grid;
  gap: 8px;
}

@media (min-width: 768px) {
  .nav-links {
    display: flex;
  }

  .mobile-toggle {
    display: none;
  }
}
</style>
