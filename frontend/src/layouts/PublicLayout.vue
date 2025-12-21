<script setup lang="ts">
import NavBar from '../components/NavBar.vue'
import FooterBar from '../components/FooterBar.vue'
</script>

<template>
  <div class="page-shell">
    <NavBar />
    <main class="main">
      <router-view />
    </main>
    <FooterBar />
  </div>
</template>

<style scoped>
.main {
  flex: 1;
  padding: 32px 0 48px;
}
</style>
