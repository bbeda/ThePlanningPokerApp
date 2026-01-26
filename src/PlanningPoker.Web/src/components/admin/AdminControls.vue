<template>
  <div class="bg-white rounded-lg shadow-lg p-6">
    <h2 class="text-xl font-bold text-gray-800 mb-4">Admin Controls</h2>

    <div class="flex flex-wrap gap-3">
      <button
        v-if="sessionStore.canStartVoting"
        @click="handleStartVoting"
        :disabled="loading"
        class="bg-secondary hover:bg-green-600 text-white font-semibold py-3 px-6 rounded-lg transition duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {{ loading ? "Starting..." : "Start Voting" }}
      </button>

      <button
        v-if="sessionStore.canReveal"
        @click="handleRevealVotes"
        :disabled="loading"
        class="bg-primary hover:bg-blue-600 text-white font-semibold py-3 px-6 rounded-lg transition duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {{ loading ? "Revealing..." : "Reveal Votes" }}
      </button>

      <button
        v-if="sessionStore.canReset"
        @click="handleResetVotes"
        :disabled="loading"
        class="bg-gray-600 hover:bg-gray-700 text-white font-semibold py-3 px-6 rounded-lg transition duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {{ loading ? "Resetting..." : "New Round" }}
      </button>

      <button
        @click="handleCloseSession"
        :disabled="loading"
        class="bg-red-600 hover:bg-red-700 text-white font-semibold py-3 px-6 rounded-lg transition duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {{ loading ? "Closing..." : "Close Session" }}
      </button>
    </div>

    <div
      v-if="error"
      class="mt-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg"
    >
      {{ error }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { useApi } from "@/composables/useApi";
import { useSessionStore } from "@/stores/sessionStore";

const api = useApi();
const router = useRouter();
const sessionStore = useSessionStore();

const loading = ref(false);
const error = ref<string | null>(null);

async function handleStartVoting() {
  if (!sessionStore.session || !sessionStore.currentUser) return;

  loading.value = true;
  error.value = null;

  try {
    await api.startVoting(
      sessionStore.session.sessionCode,
      sessionStore.currentUser.id,
    );
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Failed to start voting";
  } finally {
    loading.value = false;
  }
}

async function handleRevealVotes() {
  if (!sessionStore.session || !sessionStore.currentUser) return;

  loading.value = true;
  error.value = null;

  try {
    await api.revealVotes(
      sessionStore.session.sessionCode,
      sessionStore.currentUser.id,
    );
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Failed to reveal votes";
  } finally {
    loading.value = false;
  }
}

async function handleResetVotes() {
  if (!sessionStore.session || !sessionStore.currentUser) return;

  loading.value = true;
  error.value = null;

  try {
    await api.resetVotes(
      sessionStore.session.sessionCode,
      sessionStore.currentUser.id,
    );
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Failed to reset votes";
  } finally {
    loading.value = false;
  }
}

async function handleCloseSession() {
  if (!sessionStore.session || !sessionStore.currentUser) return;

  const confirmed = confirm(
    "Are you sure you want to close this session? All participants will be disconnected.",
  );
  if (!confirmed) return;

  loading.value = true;
  error.value = null;

  try {
    await api.closeSession(
      sessionStore.session.sessionCode,
      sessionStore.currentUser.id,
    );
    sessionStore.reset();
    router.push("/");
  } catch (err) {
    error.value =
      err instanceof Error ? err.message : "Failed to close session";
  } finally {
    loading.value = false;
  }
}
</script>
