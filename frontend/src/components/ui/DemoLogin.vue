<script setup lang="ts">
import * as yup from 'yup'
import LoginForm from './LoginForm.vue'

const schema = yup.object({
  Email: yup.string().email().required(),
  Password: yup.string().required(),
})

const onSubmit = (values: Record<string, unknown>) => {
  void values
}
</script>

<template>
  <LoginForm :schema="schema" heading="Demo login" subheading="This is a demo layout." @submit="onSubmit" />
</template>
