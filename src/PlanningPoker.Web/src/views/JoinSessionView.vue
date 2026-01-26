<template>
  <div
    class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4"
  >
    <div class="max-w-md w-full">
      <div class="bg-white rounded-lg shadow-xl p-8">
        <div class="text-center mb-6">
          <h1 class="text-3xl font-bold text-gray-800 mb-2">Join Session</h1>
          <p class="text-gray-600">Enter session code and your name</p>
        </div>

        <form @submit.prevent="handleJoin" class="space-y-6">
          <div>
            <label
              for="sessionCode"
              class="block text-sm font-medium text-gray-700 mb-2"
            >
              Session Code
            </label>
            <input
              id="sessionCode"
              v-model="sessionCode"
              type="text"
              required
              placeholder="e.g., ABCD1234"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent uppercase"
            />
          </div>

          <div>
            <label
              for="userName"
              class="block text-sm font-medium text-gray-700 mb-2"
            >
              Your Name
            </label>
            <input
              id="userName"
              v-model="userName"
              type="text"
              required
              placeholder="Enter your name"
              class="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent"
            />
          </div>

          <div
            v-if="error"
            class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg"
          >
            {{ error }}
          </div>

          <button
            type="submit"
            :disabled="loading || !sessionCode.trim() || !userName.trim()"
            class="w-full bg-primary hover:bg-blue-600 text-white font-semibold py-3 px-6 rounded-lg transition duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ loading ? "Joining..." : "Join Session" }}
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
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useApi } from "@/composables/useApi";
import { useSessionStore } from "@/stores/sessionStore";

interface Props {
  code?: string;
}

const props = defineProps<Props>();
const router = useRouter();
const api = useApi();
const sessionStore = useSessionStore();

const sessionCode = ref("");
const userName = ref("");
const loading = ref(false);
const error = ref<string | null>(null);

onMounted(() => {
  // Pre-fill session code if provided in URL
  if (props.code) {
    sessionCode.value = props.code.toUpperCase();
  }
});

async function handleJoin() {
  if (!sessionCode.value.trim() || !userName.value.trim()) return;

  loading.value = true;
  error.value = null;

  try {
    const code = sessionCode.value.trim().toUpperCase();

    // Join the session
    const user = await api.joinSession(code, userName.value.trim());
    sessionStore.setCurrentUser(user);

    // Navigate to session - SessionView will fetch the complete session data
    router.push(`/session/${code}`);
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Failed to join session";
  } finally {
    loading.value = false;
  }
}
</script>
