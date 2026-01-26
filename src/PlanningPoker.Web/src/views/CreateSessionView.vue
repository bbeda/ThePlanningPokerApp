<template>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
    <div class="max-w-md w-full">
      <div class="bg-white rounded-lg shadow-xl p-8">
        <div class="text-center mb-6">
          <h1 class="text-3xl font-bold text-gray-800 mb-2">Create Session</h1>
          <p class="text-gray-600">Start a new planning poker session</p>
        </div>

        <form @submit.prevent="handleCreate" class="space-y-6">
          <div>
            <label for="name" class="block text-sm font-medium text-gray-700 mb-2">
              Your Name
            </label>
            <input
              id="name"
              v-model="ownerName"
              type="text"
              required
              placeholder="Enter your name"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
            />
          </div>

          <div v-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
            {{ error }}
          </div>

          <button
            type="submit"
            :disabled="loading || !ownerName.trim()"
            class="w-full bg-primary hover:bg-blue-600 text-white font-semibold py-3 px-6 rounded-lg transition duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ loading ? 'Creating...' : 'Create Session' }}
          </button>

          <router-link
            to="/"
            class="block text-center text-gray-600 hover:text-gray-800 transition duration-200"
          >
            Back to Home
          </router-link>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '@/composables/useApi'
import { useSessionStore } from '@/stores/sessionStore'

const router = useRouter()
const api = useApi()
const sessionStore = useSessionStore()

const ownerName = ref('')
const loading = ref(false)
const error = ref<string | null>(null)

async function handleCreate() {
  if (!ownerName.value.trim()) return

  loading.value = true
  error.value = null

  try {
    const session = await api.createSession(ownerName.value.trim())

    // Set session and user in store
    sessionStore.setSession(session)
    const owner = session.users.find(u => u.isOwner)
    if (owner) {
      sessionStore.setCurrentUser(owner)
    }

    // Navigate to session
    router.push(`/session/${session.sessionCode}`)
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to create session'
  } finally {
    loading.value = false
  }
}
</script>
