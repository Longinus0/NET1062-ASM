import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import PublicLayout from '../layouts/PublicLayout.vue'
import HomePage from '../pages/HomePage.vue'
import MenuPage from '../pages/MenuPage.vue'
import CombosPage from '../pages/CombosPage.vue'
import ProductDetailPage from '../pages/ProductDetailPage.vue'
import ComboDetailPage from '../pages/ComboDetailPage.vue'
import SearchPage from '../pages/SearchPage.vue'
import LoginPage from '../pages/LoginPage.vue'
import RegisterPage from '../pages/RegisterPage.vue'
import ProfilePage from '../pages/ProfilePage.vue'
import CartPage from '../pages/CartPage.vue'
import CheckoutPage from '../pages/CheckoutPage.vue'
import OrdersPage from '../pages/OrdersPage.vue'
import OrderDetailPage from '../pages/OrderDetailPage.vue'
import { useAuthStore } from '../stores/auth'
import AdminLayout from '../layouts/AdminLayout.vue'
import AdminDashboardPage from '../pages/admin/AdminDashboardPage.vue'
import AdminUsersPage from '../pages/admin/AdminUsersPage.vue'
import AdminProductsPage from '../pages/admin/AdminProductsPage.vue'
import AdminCombosPage from '../pages/admin/AdminCombosPage.vue'
import AdminOrdersPage from '../pages/admin/AdminOrdersPage.vue'
import AdminAuditLogsPage from '../pages/admin/AdminAuditLogsPage.vue'
import AdminRolePage from '../pages/admin/AdminRolePage.vue'
import DemoHeaderPage from '../pages/DemoHeaderPage.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: PublicLayout,
    children: [
      { path: '', component: HomePage },
      { path: 'menu', component: MenuPage },
      { path: 'combos', component: CombosPage },
      { path: 'product/:id', component: ProductDetailPage },
      { path: 'combo/:id', component: ComboDetailPage },
      { path: 'search', component: SearchPage },
      { path: 'login', component: LoginPage },
      { path: 'register', component: RegisterPage },
      { path: 'profile', component: ProfilePage, meta: { requiresAuth: true } },
      { path: 'cart', component: CartPage },
      { path: 'checkout', component: CheckoutPage, meta: { requiresAuth: true } },
      { path: 'orders', component: OrdersPage, meta: { requiresAuth: true } },
      { path: 'orders/:id', component: OrderDetailPage, meta: { requiresAuth: true } },
      { path: 'demo-header', component: DemoHeaderPage },
    ],
  },
  {
    path: '/admin',
    component: AdminLayout,
    meta: { requiresAuth: true, requiresAdmin: true },
    children: [
      { path: '', component: AdminDashboardPage },
      { path: 'users', component: AdminUsersPage },
      { path: 'products', component: AdminProductsPage },
      { path: 'combos', component: AdminCombosPage },
      { path: 'orders', component: AdminOrdersPage },
      { path: 'audit-logs', component: AdminAuditLogsPage },
      { path: 'role', component: AdminRolePage },
    ],
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach(async (to) => {
  const authStore = useAuthStore()
  if (!authStore.user) {
    await authStore.loadUserFromToken()
  }

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return { path: '/login', query: { redirect: to.fullPath } }
  }

  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    return { path: '/' }
  }
})

export default router
