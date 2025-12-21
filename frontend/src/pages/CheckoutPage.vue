<script setup lang="ts">
import { computed, ref } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useRouter } from 'vue-router'
import { useCartStore } from '../stores/cart'
import { useAuthStore } from '../stores/auth'
import { createOrder } from '../services/orders'
import { validatePromoCode } from '../services/promo'

const cart = useCartStore()
const auth = useAuthStore()
const toast = useToast()
const router = useRouter()

const address = ref(auth.user?.Address || '')
const note = ref('')
const paymentMethod = ref('Tiền mặt')
const promoCode = ref('')
const promoInfo = ref<{ Type: string; Value: number } | null>(null)
const promoMessage = ref('')
const promoLoading = ref(false)
const idempotencyKey = ref<string | null>(null)
const isSubmitting = ref(false)

const paymentOptions = [
  { label: 'Tiền mặt', value: 'Tiền mặt' },
  { label: 'MoMo', value: 'MoMo' },
  { label: 'VNPay', value: 'VNPay' },
  { label: 'ZaloPay', value: 'ZaloPay' },
  { label: 'Thẻ', value: 'Thẻ' },
]

const currency = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

const promoDiscount = computed(() => {
  if (!promoInfo.value) return 0
  if (promoInfo.value.Type === 'Phần trăm') {
    return Math.min(cart.subTotal, Math.round((cart.subTotal * promoInfo.value.Value) / 100))
  }
  return Math.min(cart.subTotal, promoInfo.value.Value)
})

const comboDiscount = computed(() => Math.min(cart.subTotal, cart.comboDiscount))
const totalDiscount = computed(() => Math.min(cart.subTotal, promoDiscount.value + comboDiscount.value))
const finalTotal = computed(() => Math.max(0, cart.subTotal - totalDiscount.value + cart.deliveryFee))

const qrImage = computed(() => {
  const method = paymentMethod.value
  if (method === 'Tiền mặt') return null
  const data = encodeURIComponent(`Thanh toán ${method} - ${finalTotal.value} VND`)
  return `https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${data}`
})

const applyPromo = async () => {
  if (!promoCode.value.trim()) {
    promoInfo.value = null
    promoMessage.value = ''
    return
  }
  promoLoading.value = true
  promoMessage.value = ''
  try {
    const result = await validatePromoCode(promoCode.value.trim())
    promoInfo.value = { Type: result.Type, Value: result.Value }
    promoMessage.value = `Đã áp dụng mã ${result.Code}`
  } catch (err: any) {
    promoInfo.value = null
    promoMessage.value = err?.response?.data?.message || 'Không thể áp dụng mã.'
  } finally {
    promoLoading.value = false
  }
}

const submitOrder = async () => {
  if (!auth.user) {
    toast.add({ severity: 'warn', summary: 'Vui lòng đăng nhập', life: 2000 })
    router.push('/login')
    return
  }
  if (!cart.items.length) {
    toast.add({ severity: 'info', summary: 'Giỏ hàng trống', life: 2000 })
    return
  }
  if (!address.value.trim()) {
    toast.add({ severity: 'warn', summary: 'Vui lòng nhập địa chỉ giao hàng', life: 2000 })
    return
  }

  if (isSubmitting.value) return
  isSubmitting.value = true
  try {
    if (!idempotencyKey.value) {
      idempotencyKey.value = crypto.randomUUID ? crypto.randomUUID() : `order-${Date.now()}`
    }
    const payload = {
      UserId: auth.user.UserId,
      PaymentMethod: paymentMethod.value,
      PromoCode: promoInfo.value ? promoCode.value.trim().toUpperCase() : null,
      ComboDiscount: comboDiscount.value || null,
      Note: note.value || null,
      Items: cart.items.map((item) => ({
        ProductId: item.product.ProductId,
        Quantity: item.quantity,
      })),
    }
    const result = await createOrder(payload, idempotencyKey.value)
    cart.clearCart()
    toast.add({ severity: 'success', summary: 'Đặt hàng thành công', detail: `Mã đơn ${result.OrderCode}`, life: 3000 })
    router.push(`/orders/${result.OrderId}`)
    idempotencyKey.value = null
  } catch (err: any) {
    toast.add({
      severity: 'error',
      summary: 'Không thể đặt hàng',
      detail: err?.response?.data?.message || 'Vui lòng thử lại.',
      life: 3000,
    })
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <section class="container checkout">
    <div class="checkout-header">
      <div>
        <h2>Thanh toán</h2>
        <p>Nhập địa chỉ giao hàng và phương thức thanh toán.</p>
      </div>
    </div>

    <div class="checkout-grid">
      <PCard class="panel">
        <template #title>Thông tin giao hàng</template>
        <template #content>
          <div class="field">
            <label>Địa chỉ giao hàng</label>
            <PInputText v-model="address" placeholder="Số nhà, đường, quận..." />
          </div>
          <div class="field">
            <label>Ghi chú</label>
            <PTextarea v-model="note" rows="3" placeholder="Ghi chú thêm cho cửa hàng" />
          </div>
          <div class="field">
            <label>Phương thức thanh toán</label>
            <PDropdown v-model="paymentMethod" :options="paymentOptions" option-label="label" option-value="value" />
          </div>
          <div class="field promo">
            <label>Mã giảm giá</label>
            <div class="promo-row">
              <PInputText v-model="promoCode" placeholder="Ví dụ: TET2025" />
              <PButton
                label="Áp dụng"
                class="p-button-warning"
                :loading="promoLoading"
                :disabled="promoLoading"
                @click="applyPromo"
              />
            </div>
            <small v-if="promoMessage" :class="{ error: !promoInfo }">{{ promoMessage }}</small>
          </div>
          <div class="field qr">
            <label>QR thanh toán</label>
            <div class="qr-box">
              <img v-if="qrImage" :src="qrImage" :alt="`QR ${paymentMethod}`" />
              <p v-else>Thanh toán khi nhận hàng.</p>
            </div>
          </div>
        </template>
      </PCard>

      <PCard class="panel">
        <template #title>Tóm tắt đơn</template>
        <template #content>
          <div class="summary-row">
            <span>Tạm tính</span>
            <strong>{{ currency.format(cart.subTotal) }}</strong>
          </div>
          <div class="summary-row" v-if="comboDiscount">
            <span>Giảm giá combo</span>
            <strong>-{{ currency.format(comboDiscount) }}</strong>
          </div>
          <div class="summary-row">
            <span>Giảm giá mã</span>
            <strong>-{{ currency.format(promoDiscount) }}</strong>
          </div>
          <div class="summary-row">
            <span>Phí giao hàng</span>
            <strong>{{ currency.format(cart.deliveryFee) }}</strong>
          </div>
          <div class="summary-row total">
            <span>Tổng cộng</span>
            <strong>{{ currency.format(finalTotal) }}</strong>
          </div>
          <PButton
            label="Xác nhận đặt hàng"
            class="p-button-warning"
            :loading="isSubmitting"
            :disabled="isSubmitting"
            @click="submitOrder"
          />
        </template>
      </PCard>
    </div>
  </section>
</template>

<style scoped>
.checkout {
  padding: 28px 0 48px;
}

.checkout-header h2 {
  font-family: 'Fraunces', serif;
  font-size: clamp(1.8rem, 3vw, 2.4rem);
}

.checkout-header p {
  color: var(--muted);
}

.promo .promo-row {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 10px;
}

.promo small {
  display: inline-block;
  margin-top: 6px;
  color: #2ecc71;
}

.promo small.error {
  color: #c0392b;
}

.qr .qr-box {
  border: 1px dashed rgba(31, 19, 11, 0.15);
  border-radius: 16px;
  padding: 12px;
  display: grid;
  place-items: center;
  background: #fffaf5;
}

.qr img {
  width: 160px;
  height: 160px;
  object-fit: cover;
}

.checkout-grid {
  display: grid;
  gap: 20px;
  grid-template-columns: minmax(0, 2fr) minmax(0, 1fr);
  margin-top: 16px;
}

.panel {
  background: var(--surface);
  border: 1px solid rgba(31, 19, 11, 0.08);
  box-shadow: var(--shadow);
}

.field {
  display: grid;
  gap: 6px;
  margin-bottom: 12px;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
}

.summary-row.total {
  font-size: 1.05rem;
}

@media (max-width: 960px) {
  .checkout-grid {
    grid-template-columns: 1fr;
  }
}
</style>
