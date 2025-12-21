<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getOrderById, getOrderHistory } from '../services/orders'
import type { OrderDetailDTO } from '../types/dtos'
import OrderTimeline from '../components/OrderTimeline.vue'

const route = useRoute()
const router = useRouter()
const order = ref<OrderDetailDTO | null>(null)
const history = ref<Array<{ FromStatus: string; ToStatus: string; ChangedAt: string; Note?: string | null }>>([])
const isLoading = ref(true)
const error = ref('')

const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

const loadOrder = async () => {
  try {
    const orderId = Number(route.params.id)
    order.value = await getOrderById(orderId)
    history.value = await getOrderHistory(orderId)
  } catch {
    error.value = 'Không thể tải đơn hàng.'
  } finally {
    isLoading.value = false
  }
}

onMounted(loadOrder)
</script>

<template>
  <section class="container order-detail">
    <PButton icon="pi pi-arrow-left" class="p-button-text" label="Quay lại" @click="router.back()" />

    <div v-if="isLoading">Đang tải...</div>
    <div v-else-if="order" class="detail-grid">
      <PCard class="panel">
        <template #title>Thông tin đơn</template>
        <template #content>
          <div class="meta">
            <span>Mã đơn</span>
            <strong>{{ order.Order.OrderCode }}</strong>
          </div>
          <div class="meta">
            <span>Trạng thái</span>
            <strong>{{ order.Order.Status }}</strong>
          </div>
          <div class="meta">
            <span>Tổng tiền</span>
            <strong>{{ currency.format(order.Order.GrandTotal) }}</strong>
          </div>
        </template>
      </PCard>

      <PCard class="panel">
        <template #title>Danh sách món</template>
        <template #content>
          <div v-for="item in order.Items" :key="item.OrderItemId" class="item">
            <div>
              <h4>{{ item.ProductNameSnapshot }}</h4>
              <span>{{ item.Quantity }} x {{ item.UnitPriceSnapshot }}</span>
            </div>
            <strong>{{ currency.format(item.LineTotal) }}</strong>
          </div>
        </template>
      </PCard>
    </div>

    <div v-if="history.length" class="history">
      <h3>Tiến trình đơn hàng</h3>
      <OrderTimeline :history="history" />
    </div>

    <div v-if="error" class="error">{{ error }}</div>
  </section>
</template>

<style scoped>
.order-detail {
  padding: 28px 0 48px;
}

.detail-grid {
  display: grid;
  gap: 20px;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  margin-top: 16px;
}

.panel {
  border: 1px solid rgba(31, 19, 11, 0.08);
  box-shadow: var(--shadow);
}

.meta {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
}

.item {
  display: flex;
  justify-content: space-between;
  padding: 10px 0;
  border-bottom: 1px solid rgba(31, 19, 11, 0.08);
}

.history {
  margin-top: 24px;
}

.history h3 {
  margin-bottom: 12px;
}

.error {
  color: #ef4444;
  padding-top: 12px;
}
</style>
