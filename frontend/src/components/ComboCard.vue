<script setup lang="ts">
import { useRouter } from 'vue-router'
import type { ComboDTO } from '../types/dtos'

const props = defineProps<{
  combo: ComboDTO
}>()

const router = useRouter()
const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })
</script>

<template>
  <PCard class="combo-card">
    <template #content>
      <div class="combo-body">
        <div class="combo-top">
          <h3>{{ props.combo.Name }}</h3>
          <span class="price">{{ currency.format(props.combo.Price) }}</span>
        </div>
        <p class="desc">{{ props.combo.Description }}</p>
        <div class="combo-tags">
          <PTag :value="props.combo.IsActive ? 'Đang bán' : 'Tạm dừng'" :severity="props.combo.IsActive ? 'success' : 'danger'" />
        </div>
      </div>
    </template>
    <template #footer>
      <div class="card-footer">
        <PButton
          label="Xem chi tiết"
          class="p-button-text"
          icon="pi pi-arrow-right"
          @click="router.push(`/combo/${props.combo.ComboId}`)"
        />
        <PButton label="Thêm combo" icon="pi pi-shopping-cart" class="p-button-warning" />
      </div>
    </template>
  </PCard>
</template>

<style scoped>
.combo-card {
  background: var(--surface-2);
  border: 1px solid rgba(31, 19, 11, 0.08);
  color: var(--text);
  box-shadow: var(--shadow);
}

.combo-body {
  display: grid;
  gap: 12px;
}

.combo-top {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: baseline;
}

.price {
  color: var(--accent);
  font-weight: 600;
}

.desc {
  color: var(--muted);
  min-height: 44px;
}

.combo-tags {
  display: flex;
  gap: 8px;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: center;
}
</style>
