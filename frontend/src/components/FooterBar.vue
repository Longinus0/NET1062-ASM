<template>
  <footer class="footer">
    <div class="container footer-grid">
      <div>
        <h3>Saigon Fast</h3>
        <p>Fast food chuẩn vị Việt, giao nhanh từng phút.</p>
      </div>
      <div>
        <p class="footer-title">Giờ mở cửa</p>
        <p>Thứ 2 - Chủ nhật · 09:00 - 23:00</p>
      </div>
      <div>
        <p class="footer-title">Liên hệ</p>
        <p>Hotline: 1900 9999</p>
        <p>support@saigonfast.vn</p>
      </div>
    </div>
  </footer>
</template>

<style scoped>
.footer {
  background: linear-gradient(120deg, #fff6ee, #ffe7d6);
  padding: 32px 0;
  color: var(--muted);
  border-top: 1px solid rgba(31, 19, 11, 0.08);
}

.footer-grid {
  display: grid;
  gap: 24px;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
}

.footer h3 {
  color: var(--text);
  margin-bottom: 8px;
  font-family: 'Fraunces', serif;
}

.footer-title {
  color: var(--text);
  font-weight: 600;
  margin-bottom: 6px;
}
</style>
