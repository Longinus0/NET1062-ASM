<script setup lang="ts">
import { Form, Field } from 'vee-validate'

const props = defineProps<{
  schema: unknown
  heading?: string
  subheading?: string
  imageUrl?: string
}>()

const emit = defineEmits<{
  (event: 'submit', values: Record<string, unknown>): void
  (event: 'google'): void
  (event: 'facebook'): void
}>()

const handleSubmit = (values: Record<string, unknown>) => {
  emit('submit', values)
}
</script>

<template>
  <section class="register-shell">
    <div class="register-media">
      <img
        :src="
          props.imageUrl ||
          'https://images.unsplash.com/photo-1490645935967-10de6ba17061?auto=format&fit=crop&w=1000&q=80'
        "
        alt="Bàn tiệc"
      />
      <div class="media-overlay">
        <span class="eyebrow">Tươi &amp; tiện</span>
        <h3>Hành trình vị ngon</h3>
        <p>Đăng ký để lưu món yêu thích, nhận ưu đãi và đặt món nhanh hơn.</p>
      </div>
    </div>

    <div class="register-panel">
      <Form :validation-schema="props.schema" class="register-form" @submit="handleSubmit">
        <div class="register-header">
          <h2>{{ props.heading || 'Đăng ký' }}</h2>
          <p>{{ props.subheading || 'Tạo tài khoản mới chỉ trong vài phút.' }}</p>
        </div>

        <div class="social-buttons">
          <button type="button" class="social-button" @click="emit('google')">
            <span class="social-icon" aria-hidden="true">
              <svg width="18" height="18" viewBox="0 0 18 18" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path
                  d="M17.64 9.2045C17.64 8.56632 17.5827 7.95268 17.4764 7.36316H9V10.844H13.8436C13.635 11.969 12.9991 12.9227 12.0395 13.5618V15.8195H14.9564C16.6618 14.2468 17.64 11.8245 17.64 9.2045Z"
                  fill="#4285F4"
                />
                <path
                  d="M9 18C11.43 18 13.4673 17.1941 14.9564 15.8195L12.0395 13.5618C11.2336 14.1018 10.2027 14.4205 9 14.4205C6.65591 14.4205 4.67182 12.8373 3.96409 10.71H0.948181V13.0418C2.42955 15.9832 5.47545 18 9 18Z"
                  fill="#34A853"
                />
                <path
                  d="M3.96409 10.71C3.78409 10.17 3.68182 9.59273 3.68182 9C3.68182 8.40727 3.78409 7.83 3.96409 7.29V4.95818H0.948181C0.338182 6.17364 0 7.54818 0 9C0 10.4518 0.338182 11.8264 0.948181 13.0418L3.96409 10.71Z"
                  fill="#FBBC05"
                />
                <path
                  d="M9 3.57955C10.3132 3.57955 11.4941 4.03182 12.4218 4.91955L15.0218 2.31955C13.4632 0.867273 11.4259 0 9 0C5.47545 0 2.42955 2.01682 0.948181 4.95818L3.96409 7.29C4.67182 5.16273 6.65591 3.57955 9 3.57955Z"
                  fill="#EA4335"
                />
              </svg>
            </span>
            <span>Đăng ký với Google</span>
          </button>

          <button type="button" class="social-button facebook-button" @click="emit('facebook')">
            <span class="social-icon" aria-hidden="true">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path
                  d="M22.675 0h-21.35C.597 0 0 .597 0 1.325v21.351C0 23.403.597 24 1.325 24h11.495v-9.294H9.692V11.01h3.128V8.309c0-3.1 1.893-4.788 4.659-4.788 1.325 0 2.463.099 2.795.143v3.24l-1.918.001c-1.504 0-1.796.715-1.796 1.763v2.312h3.587l-.467 3.696h-3.12V24h6.116C23.403 24 24 23.403 24 22.676V1.325C24 .597 23.403 0 22.675 0z"
                  fill="#1877F2"
                />
              </svg>
            </span>
            <span>Đăng ký với Facebook</span>
          </button>
        </div>

        <div class="divider">
          <span></span>
          <p>hoặc đăng ký bằng email</p>
          <span></span>
        </div>

        <div class="fields-grid">
          <div class="field">
            <label>Họ tên</label>
            <Field name="FullName" v-slot="{ field, errorMessage }">
              <div class="input-pill">
                <i class="pi pi-user" aria-hidden="true"></i>
                <PInputText v-bind="field" placeholder="Nguyễn Văn A" class="field-input" />
              </div>
              <span class="error">{{ errorMessage || '\u00A0' }}</span>
            </Field>
          </div>

          <div class="field">
            <label>Email</label>
            <Field name="Email" v-slot="{ field, errorMessage }">
              <div class="input-pill">
                <i class="pi pi-envelope" aria-hidden="true"></i>
                <PInputText v-bind="field" placeholder="name@email.com" class="field-input" />
              </div>
              <span class="error">{{ errorMessage || '\u00A0' }}</span>
            </Field>
          </div>

          <div class="field">
            <label>Số điện thoại</label>
            <Field name="Phone" v-slot="{ field, errorMessage }">
              <div class="input-pill">
                <i class="pi pi-phone" aria-hidden="true"></i>
                <PInputText v-bind="field" placeholder="0909xxxxxx" class="field-input" />
              </div>
              <span class="error">{{ errorMessage || '\u00A0' }}</span>
            </Field>
          </div>

          <div class="field">
            <label>Địa chỉ</label>
            <Field name="Address" v-slot="{ field, errorMessage }">
              <div class="input-pill">
                <i class="pi pi-map-marker" aria-hidden="true"></i>
                <PInputText v-bind="field" placeholder="Quận 1, TP.HCM" class="field-input" />
              </div>
              <span class="error">{{ errorMessage || '\u00A0' }}</span>
            </Field>
          </div>

          <div class="field">
            <label>Avatar URL</label>
            <Field name="AvatarUrl" v-slot="{ field, errorMessage }">
              <div class="input-pill">
                <i class="pi pi-image" aria-hidden="true"></i>
                <PInputText v-bind="field" placeholder="https://..." class="field-input" />
              </div>
              <span class="error">{{ errorMessage || '\u00A0' }}</span>
            </Field>
          </div>

          <div class="field">
            <label>Mật khẩu</label>
            <Field name="Password" v-slot="{ field, errorMessage }">
              <div class="input-pill">
                <i class="pi pi-lock" aria-hidden="true"></i>
                <PPassword
                  :model-value="field.value"
                  placeholder="******"
                  :feedback="true"
                  toggle-mask
                  input-class="field-input"
                  @update:model-value="field.onChange"
                  @blur="field.onBlur"
                />
              </div>
              <span class="error">{{ errorMessage || '\u00A0' }}</span>
            </Field>
          </div>

          <div class="field">
            <label>Xác nhận mật khẩu</label>
            <Field name="ConfirmPassword" v-slot="{ field, errorMessage }">
              <div class="input-pill">
                <i class="pi pi-lock" aria-hidden="true"></i>
                <PPassword
                  :model-value="field.value"
                  placeholder="******"
                  :feedback="false"
                  toggle-mask
                  input-class="field-input"
                  @update:model-value="field.onChange"
                  @blur="field.onBlur"
                />
              </div>
              <span class="error">{{ errorMessage || '\u00A0' }}</span>
            </Field>
          </div>
        </div>

        <PButton label="Tạo tài khoản" type="submit" class="p-button-warning register-submit" />

        <slot name="footer" />
      </Form>
    </div>
  </section>
</template>

<style scoped>
.register-shell {
  display: grid;
  grid-template-columns: minmax(0, 1.1fr) minmax(0, 1fr);
  background: var(--surface);
  border-radius: 18px;
  overflow: hidden;
  border: 1px solid rgba(31, 19, 11, 0.08);
  box-shadow: var(--shadow);
  min-height: 620px;
}

.register-media {
  position: relative;
  min-height: 560px;
}

.register-media img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.media-overlay {
  position: absolute;
  inset: auto 24px 24px 24px;
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(8px);
  border-radius: 14px;
  padding: 16px 18px;
  display: grid;
  gap: 6px;
  color: #2b1b10;
}

.media-overlay h3 {
  font-size: 1.2rem;
  font-family: 'Fraunces', serif;
}

.eyebrow {
  text-transform: uppercase;
  letter-spacing: 0.2em;
  font-size: 0.7rem;
  color: #9b6d4a;
  font-weight: 600;
}

.register-panel {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 32px 28px;
  background: linear-gradient(160deg, rgba(255, 246, 235, 0.7), rgba(255, 255, 255, 0.9));
}

.register-form {
  width: min(520px, 100%);
  display: grid;
  gap: 14px;
}

.register-header {
  display: grid;
  gap: 6px;
  text-align: center;
}

.register-header h2 {
  font-family: 'Fraunces', serif;
  font-size: clamp(1.6rem, 2.5vw, 2.2rem);
}

.register-header p {
  color: var(--muted);
  font-size: 0.95rem;
}

.social-buttons {
  display: grid;
  gap: 10px;
}

.social-button {
  border: 1px solid rgba(31, 19, 11, 0.15);
  border-radius: 999px;
  background: #fff;
  padding: 10px 16px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  font-weight: 600;
  color: #2b1b10;
  cursor: pointer;
}

.social-button:hover {
  background: rgba(255, 245, 234, 0.9);
}

.facebook-button {
  background: rgba(24, 119, 242, 0.08);
  border-color: rgba(24, 119, 242, 0.25);
  color: #1d4ed8;
}

.facebook-button:hover {
  background: rgba(24, 119, 242, 0.16);
}

.social-icon {
  display: inline-flex;
}

.divider {
  display: flex;
  align-items: center;
  gap: 10px;
  color: var(--muted);
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

.divider span {
  flex: 1;
  height: 1px;
  background: rgba(31, 19, 11, 0.12);
}

.field {
  display: grid;
  gap: 6px;
  margin-bottom: 1px;
}

.field label {
  font-weight: 600;
  font-size: 0.9rem;
}

.input-pill {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 14px;
  height: 46px;
  border-radius: 999px;
  border: 1px solid rgba(31, 19, 11, 0.15);
  background: rgba(255, 255, 255, 0.9);
}

.input-pill :deep(.p-inputtext),
.input-pill :deep(.p-password-input) {
  border: none;
  background: transparent;
  box-shadow: none;
  padding: 0;
  font-size: 0.92rem;
  width: 100%;
}

.input-pill :deep(.p-password) {
  width: 100%;
}

.field-input {
  width: 100%;
}

.register-submit {
  border-radius: 999px;
  height: 44px;
}

.fields-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}

.field.full {
  grid-column: 1 / -1;
}

.error {
  color: #ef4444;
  font-size: 0.8rem;
  min-height: 1rem;
  line-height: 0.9rem;
  margin-top: 0;
}

@media (max-width: 900px) {
  .register-shell {
    grid-template-columns: 1fr;
  }

  .register-media {
    display: none;
  }

  .register-form {
    width: min(360px, 100%);
  }

  .fields-grid {
    grid-template-columns: 1fr;
  }
}
</style>
