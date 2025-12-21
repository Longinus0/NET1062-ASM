<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useAuthStore } from '../stores/auth'
import { getOrdersByUser } from '../services/orders'
import type { OrderSummaryDTO } from '../types/dtos'
import { useRouter } from 'vue-router'

const auth = useAuthStore()
const router = useRouter()
const orders = ref<OrderSummaryDTO[]>([])
const isLoading = ref(true)
const error = ref('')

const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

const loadOrders = async () => {
  if (!auth.user) return
  try {
    orders.value = await getOrdersByUser(auth.user.UserId)
  } catch {
    error.value = 'Không thể tải lịch sử đơn.'
  } finally {
    isLoading.value = false
  }
}

onMounted(loadOrders)
</script>

<template>
  <section class="container orders">
    <div class="orders-header">
      <div>
        <h2>Đơn hàng của tôi</h2>
        <p>Theo dõi lịch sử mua hàng và trạng thái đơn.</p>
      </div>
    </div>

    <div v-if="isLoading">Đang tải...</div>
    <div v-else-if="!orders.length" class="empty">
      <p>Bạn chưa có đơn hàng nào.</p>
      <PButton label="Đặt món ngay" class="p-button-text" @click="router.push('/menu')" />
    </div>
    <div v-else class="list">
      <PCard v-for="order in orders" :key="order.OrderId" class="order-card">
        <template #content>
          <div class="row">
            <div>
              <h3>{{ order.OrderCode }}</h3>
              <p>{{ order.CreatedAt }}</p>
            </div>
            <div>
              <span class="status">{{ order.Status }}</span>
              <strong>{{ currency.format(order.GrandTotal) }}</strong>
            </div>
          </div>
          <PButton label="Xem chi tiết" class="p-button-text" @click="router.push(`/orders/${order.OrderId}`)" />
        </template>
      </PCard>
    </div>
    <div v-if="error" class="error">{{ error }}</div>
  </section>
</template>

<style scoped>
.orders {
  padding: 28px 0 48px;
}

.orders-header h2 {
  font-family: 'Fraunces', serif;
  font-size: clamp(1.8rem, 3vw, 2.4rem);
}

.orders-header p {
  color: var(--muted);
}

.list {
  display: grid;
  gap: 14px;
  margin-top: 16px;
}

.order-card {
  border: 1px solid rgba(31, 19, 11, 0.08);
  box-shadow: var(--shadow);
}

.row {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: center;
}

.status {
  display: inline-block;
  margin-right: 8px;
  padding: 4px 10px;
  border-radius: var(--radius);
  background: rgba(255, 107, 53, 0.12);
  font-size: 0.8rem;
}

.empty {
  color: var(--muted);
  padding: 20px 0;
}

.error {
  color: #ef4444;
  padding-top: 12px;
}
</style>
