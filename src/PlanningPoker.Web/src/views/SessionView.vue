<template>
  <div class="min-h-screen bg-gray-50">
    <div v-if="loading" class="flex items-center justify-center min-h-screen">
      <div class="text-center">
        <div class="text-2xl font-semibold text-gray-700">
          Loading session...
        </div>
      </div>
    </div>

    <div
      v-else-if="error"
      class="flex items-center justify-center min-h-screen"
    >
      <div class="bg-white rounded-lg shadow-xl p-8 max-w-md">
        <div class="text-center">
          <div class="text-red-600 text-xl font-semibold mb-4">Error</div>
          <div class="text-gray-700 mb-6">{{ error }}</div>
          <router-link
            to="/"
            class="inline-block bg-primary hover:bg-blue-600 text-white font-semibold py-2 px-6 rounded-lg transition duration-200"
          >
            Back to Home
          </router-link>
        </div>
      </div>
    </div>

    <div v-else-if="session" class="container mx-auto px-4 py-8 max-w-6xl">
      <SessionInfo />

      <div class="mt-6">
        <UserList />
      </div>

      <div class="mt-8">
        <AdminControls v-if="sessionStore.isOwner" />
      </div>

      <div v-if="currentRound" class="mt-8">
        <VotingCards v-if="currentRound.status === 'InProgress'" />
        <VotingResults v-else-if="currentRound.status === 'Revealed'" />
      </div>

      <div v-else class="mt-8 text-center">
        <div class="bg-white rounded-lg shadow p-8">
          <p class="text-gray-600 text-lg">
            {{
              sessionStore.isOwner
                ? 'Click "Start Voting" to begin a new round'
                : "Waiting for the admin to start voting..."
            }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { useApi } from "@/composables/useApi";
import { useSse } from "@/composables/useSse";
import { useSessionStore } from "@/stores/sessionStore";
import SessionInfo from "@/components/session/SessionInfo.vue";
import UserList from "@/components/users/UserList.vue";
import AdminControls from "@/components/admin/AdminControls.vue";
import VotingCards from "@/components/voting/VotingCards.vue";
import VotingResults from "@/components/voting/VotingResults.vue";

interface Props {
  code: string;
}

const props = defineProps<Props>();
const router = useRouter();
const api = useApi();
const sse = useSse();
const sessionStore = useSessionStore();

const loading = ref(true);
const error = ref<string | null>(null);

const session = computed(() => sessionStore.session);
const currentRound = computed(() => sessionStore.currentRound);

onMounted(async () => {
  try {
    // If we don't have session data, fetch it
    if (
      !sessionStore.session ||
      sessionStore.session.sessionCode !== props.code
    ) {
      const sessionData = await api.getSession(props.code);
      sessionStore.setSession(sessionData);
    }

    // If we don't have current user, try to restore from localStorage
    if (!sessionStore.currentUser) {
      const { user, sessionCode } = sessionStore.restoreFromStorage();

      if (user && sessionCode === props.code) {
        // Try to reconnect
        try {
          const reconnectedUser = await api.joinSession(props.code, user.name);
          sessionStore.setCurrentUser(reconnectedUser);
        } catch {
          // Reconnection failed, redirect to join
          router.push(`/join/${props.code}`);
          return;
        }
      } else {
        // No stored data, redirect to join
        router.push(`/join/${props.code}`);
        return;
      }
    }

    // Connect to SSE
    sse.connect(props.code, sessionStore.currentUser!.id);

    loading.value = false;
  } catch (err) {
    error.value = err instanceof Error ? err.message : "Failed to load session";
    loading.value = false;
  }
});

onUnmounted(() => {
  sse.disconnect();
});
</script>
