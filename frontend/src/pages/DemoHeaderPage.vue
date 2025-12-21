<script setup lang="ts">
import Header1 from '../components/ui/Header1.vue'
</script>

<template>
  <div class="demo">
    <Header1 />
    <main class="content">
      <div class="block"></div>
      <div class="block short"></div>
      <div v-for="i in 6" :key="i" class="para">
        <div class="line"></div>
        <div class="line"></div>
        <div class="line"></div>
        <div class="line short"></div>
      </div>
    </main>
  </div>
</template>

<style scoped>
.demo {
  min-height: 100vh;
}

.content {
  max-width: 768px;
  margin: 0 auto;
  padding: 32px 16px;
}

.block {
  height: 24px;
  background: rgba(255, 107, 53, 0.2);
  border-radius: var(--radius);
  margin-bottom: 8px;
}

.block.short {
  width: 60%;
}

.para {
  margin: 20px 0;
}

.line {
  height: 16px;
  background: rgba(255, 107, 53, 0.14);
  border-radius: var(--radius);
  margin-bottom: 6px;
}

.line.short {
  width: 50%;
}
</style>
