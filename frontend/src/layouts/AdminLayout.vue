<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import Avatar from '../components/ui/Avatar.vue'

const router = useRouter()
const auth = useAuthStore()
</script>

<template>
  <div class="admin-shell">
    <aside class="sidebar">
      <div class="brand" @click="router.push('/')">
        <span class="dot"></span>
        <div>
          <p class="name">Saigon Fast</p>
          <p class="sub">Admin Console</p>
        </div>
      </div>
      <nav class="nav">
        <RouterLink to="/admin" class="link" exact-active-class="link-active">Tổng quan</RouterLink>
        <RouterLink to="/admin/users" class="link" exact-active-class="link-active">Users</RouterLink>
        <RouterLink to="/admin/products" class="link" exact-active-class="link-active">Products</RouterLink>
        <RouterLink to="/admin/combos" class="link" exact-active-class="link-active">Combos</RouterLink>
        <RouterLink to="/admin/orders" class="link" exact-active-class="link-active">Orders</RouterLink>
        <RouterLink to="/admin/audit-logs" class="link" exact-active-class="link-active">Nhật ký</RouterLink>
        <RouterLink to="/admin/role" class="link" exact-active-class="link-active">Vai trò</RouterLink>
      </nav>
      <div class="footer">
        <div class="user-block">
          <Avatar
            class="user-avatar"
            :src="auth.user?.AvatarUrl"
            :alt="auth.user?.FullName"
            :fallback="auth.user?.FullName?.slice(0, 1)?.toUpperCase()"
          />
          <div class="user-meta">
            <p class="user-name">{{ auth.user?.FullName }}</p>
            <p class="user-email">{{ auth.user?.Email }}</p>
          </div>
        </div>
        <PButton label="Đăng xuất" class="p-button-text" @click="auth.logout(); router.push('/')" />
      </div>
    </aside>
    <main class="content">
      <router-view />
    </main>
  </div>
</template>

<style scoped>
.admin-shell {
  display: grid;
  grid-template-columns: 240px 1fr;
  min-height: 100vh;
}

.sidebar {
  background: var(--surface);
  border-right: 1px solid rgba(31, 19, 11, 0.08);
  padding: 20px 16px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.brand {
  display: flex;
  gap: 10px;
  align-items: center;
  cursor: pointer;
}

.dot {
  width: 12px;
  height: 12px;
  border-radius: var(--radius);
  background: linear-gradient(140deg, var(--accent), var(--accent-2));
}

.name {
  font-weight: 700;
}

.sub {
  font-size: 0.75rem;
  color: var(--muted);
}

.nav {
  display: grid;
  gap: 8px;
}

.link {
  padding: 10px 12px;
  border-radius: var(--radius);
  color: var(--text);
  font-weight: 600;
}

.link:hover {
  background: rgba(255, 107, 53, 0.08);
}

.link-active {
  background: rgba(255, 107, 53, 0.12);
}

.footer {
  margin-top: auto;
  display: grid;
  gap: 6px;
  color: var(--muted);
}

.user-block {
  display: flex;
  gap: 10px;
  align-items: center;
  min-width: 0;
}

.user-avatar {
  width: 36px;
  height: 36px;
  flex: 0 0 auto;
}

.user-meta {
  min-width: 0;
  display: grid;
  gap: 2px;
}

.user-name {
  font-weight: 600;
  color: var(--text);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 160px;
}

.user-email {
  font-size: 0.75rem;
  color: var(--muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 160px;
}

.content {
  padding: 24px 28px;
}

@media (max-width: 960px) {
  .admin-shell {
    grid-template-columns: 1fr;
  }

  .sidebar {
    flex-direction: row;
    align-items: center;
    overflow-x: auto;
  }

  .nav {
    display: flex;
    gap: 8px;
  }

  .footer {
    display: none;
  }
}
</style>
